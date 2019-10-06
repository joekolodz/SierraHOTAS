using Newtonsoft.Json;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SierraHOTAS.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HOTASCollection
    {
        public const bool IsDebug = false;

        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;

        [JsonProperty]
        public List<HOTASDevice> Devices { get; set; }

        private HOTASDevice _selectedDevice;

        public void Start()
        {
            LoadAllDevices();
            ListenToAllDevices();
        }

        public void Stop()
        {
            foreach (var device in Devices)
            {
                device.ButtonPressed += Device_ButtonPressed;
                device.Stop();
            }
        }

        /// <summary>
        /// Get all devices attached and load their custom button map profiles
        /// </summary>
        private void LoadAllDevices()
        {
            Devices = PopulateDevices();
            HandleDeviceEvents();

            Debug.WriteLine("\n\nLoading...");
            foreach (var entry in Devices)
            {
                Debug.WriteLine($"ProductName:{entry}");
                LoadDevice(entry);
            }
        }

        public void ListenToAllDevices()
        {
            if (HOTASCollection.IsDebug) return;
            foreach (var device in Devices)
            {
                device.Listen();
            }
        }

        private void HandleDeviceEvents()
        {
            foreach (var device in Devices)
            {
                device.ButtonPressed += Device_ButtonPressed;
            }
        }

        private void Device_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            OnButtonPress(sender, e);
        }

        private void OnButtonPress(object sender, ButtonPressedEventArgs e)
        {
            _selectedDevice = e.Device;
            ButtonPressed?.Invoke(sender, e);
        }

        private void LoadDevice(HOTASDevice hotasDevice)
        {
            //TODO: this would be iterating each device and loading the button map from JSON file
            //TODO: merge the default template with what was actually saved

            if (hotasDevice == null)
                throw new NullReferenceException("No devices found with GUID: b7e383f0-bac7-11e9-8002-444553540000");

            if (hotasDevice.InstanceId == Guid.Parse("f0bac120-bac7-11e9-8004-444553540000"))
                BuildTestData_Stick(hotasDevice.ButtonMap);

            if (hotasDevice.InstanceId == Guid.Parse("b7e383f0-bac7-11e9-8002-444553540000"))
                BuildTestData_Throttle(hotasDevice.ButtonMap);
        }

        private List<HOTASDevice> PopulateDevices()
        {
            var i = new DirectInput();
            var deviceList = new List<HOTASDevice>();

            if (IsDebug)
            {
                deviceList.Add(new HOTASDevice(Guid.NewGuid(), "Throttle"));
                return deviceList;
            }

            if (!IsDebug)
            {
                foreach (var device in i.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly))
                {
                    deviceList.Add(new HOTASDevice(device.InstanceGuid, device.ProductName));
                }
            }

            foreach (var device in i.GetDevices(DeviceType.FirstPerson, DeviceEnumerationFlags.AttachedOnly))
            {
                deviceList.Add(new HOTASDevice(device.InstanceGuid, device.ProductName));
            }

            return deviceList;
        }

        public HOTASDevice GetDevice(Guid instanceId)
        {
            return Devices.FirstOrDefault(d => d.InstanceId == instanceId);
        }

        private void BuildTestData_Throttle(List<HOTASMap> map)
        {
            var button15 = map.First(x => x.ButtonName == "Buttons15"); //Id=63
            button15.Actions = new List<ButtonAction>()
            {
                new ButtonAction()
                {
                    ScanCode = (int) Win32Structures.ScanCodeShort.SPACE,
                    Flags = 0,
                    TimeInMilliseconds = 0
                },
                new ButtonAction()
                {
                    ScanCode = (int) Win32Structures.ScanCodeShort.SPACE,
                    Flags = (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP,
                    TimeInMilliseconds = 0
                }
                };

            var button17 = map.First(x => x.ButtonName == "Buttons17");//65
            var x2 = new HOTASMap();
            button17.Actions = new List<ButtonAction>()
            {
                new ButtonAction()
                {
                    ScanCode = (int) Win32Structures.ScanCodeShort.LMENU,
                    Flags = (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED,
                    TimeInMilliseconds = 0
                },
                new ButtonAction()
                {
                    ScanCode = (int) Win32Structures.ScanCodeShort.KEY_G,
                    Flags = 0,
                    TimeInMilliseconds = 0
                },
                new ButtonAction()
                {
                    ScanCode = (int) Win32Structures.ScanCodeShort.KEY_G,
                    Flags = (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP,
                    TimeInMilliseconds = 0
                },
                new ButtonAction()
                {
                    ScanCode = (int) Win32Structures.ScanCodeShort.LMENU,
                    Flags = (int) (Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP |
                                   Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED),
                    TimeInMilliseconds = 0
                }
            };
        }

        private void BuildTestData_Stick(List<HOTASMap> map)
        {
            var button0 = map.First(x => x.ButtonName == "Buttons0"); //45

            button0.Actions = new List<ButtonAction>()
            {
                new ButtonAction()
                {
                    ScanCode = (int)Win32Structures.ScanCodeShort.SPACE,
                    Flags = 0,
                    TimeInMilliseconds = 0
                },
                new ButtonAction()
                {
                    ScanCode = (int)Win32Structures.ScanCodeShort.SPACE,
                    Flags = (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP,
                    TimeInMilliseconds = 0
                },
            };
        }
    }
}
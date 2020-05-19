using Newtonsoft.Json;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using SierraHOTAS.ViewModels;

namespace SierraHOTAS.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HOTASCollection
    {
        public event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        public event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        public event EventHandler<AxisChangedEventArgs> AxisChanged;
        public event EventHandler<ModeProfileChangedEventArgs> ModeProfileChanged;


        [JsonProperty]
        public ObservableCollection<HOTASDevice> Devices { get; set; }

        public int Mode { get; set; } = 1;

        private HOTASDevice _selectedDevice;
        public Dictionary<int, (Guid, int)> ModeProfileActivationButtons { get; }


        public void RemoveDevice(Guid instanceId)
        {
            HOTASDevice remove = null;
            foreach (var d in Devices)
            {
                if (d.InstanceId == instanceId)
                {
                    remove = d;
                }
            }

            if (remove != null)
            {
                Devices.Remove(remove);
            }
        }

        public HOTASCollection()
        {
            Devices = new ObservableCollection<HOTASDevice>();
            ModeProfileActivationButtons = new Dictionary<int, (Guid, int)>();
        }

        public void Start()
        {
            LoadAllDevices();
            ListenToAllDevices();
        }

        public void Stop()
        {
            foreach (var device in Devices)
            {
                device.ButtonPressed -= Device_ButtonPressed;
                device.AxisChanged -= Device_AxisChanged;
                device.KeystrokeDownSent -= Device_KeystrokeDownSent;
                device.KeystrokeUpSent -= Device_KeystrokeUpSent;
                device.ModeProfileSelected -= Device_ModeProfileSelected;

                device.Stop();
            }
        }

        public void ClearButtonMap()
        {
            foreach (var d in Devices)
            {
                d.ClearButtonMap();
            }
        }

        /// <summary>
        /// Get all devices attached and load their custom button map profiles
        /// </summary>
        private void LoadAllDevices()
        {
            if (MainWindow.IsDebug) return;
            Devices = QueryOperatingSystemForDevices();
        }

        public void ListenToAllDevices()
        {
            foreach (var device in Devices)
            {
                ListenToDevice(device);
            }
        }

        public void ListenToDevice(HOTASDevice device)
        {
            device.ButtonPressed += Device_ButtonPressed;
            device.AxisChanged += Device_AxisChanged;
            device.KeystrokeDownSent += Device_KeystrokeDownSent;
            device.KeystrokeUpSent += Device_KeystrokeUpSent;
            device.ModeProfileSelected += Device_ModeProfileSelected;
            device.ListenAsync();
        }

        public int SetupNewModeProfile()
        {
            var newMode = 0;
            foreach (var d in Devices)
            {
                newMode = d.SetupNewModeProfile();
            }

            return newMode;
        }

        private void Device_ModeProfileSelected(object sender, ModeProfileSelectedEventArgs e)
        {
            if (!(sender is HOTASDevice device)) return;
            SetMode(e.Mode);
        }

        private void Device_KeystrokeDownSent(object sender, KeystrokeSentEventArgs e)
        {
            KeystrokeDownSent?.Invoke(sender, e);
        }

        private void Device_KeystrokeUpSent(object sender, KeystrokeSentEventArgs e)
        {
            KeystrokeUpSent?.Invoke(sender, e);
        }

        public void ForceButtonPress(HOTASDevice device, JoystickOffset offset, bool isDown)
        {
            device.ForceButtonPress(offset, isDown);
        }

        private void Device_AxisChanged(object sender, AxisChangedEventArgs e)
        {
            _selectedDevice = e.Device;
            AxisChanged?.Invoke(sender, e);
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

        private static ObservableCollection<HOTASDevice> QueryOperatingSystemForDevices()
        {
            var deviceList = new ObservableCollection<HOTASDevice>();

            using (var i = new DirectInput())
            {
                foreach (var device in i.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly))
                {
                    deviceList.Add(new HOTASDevice(device.InstanceGuid, device.ProductName));
                }

                foreach (var device in i.GetDevices(DeviceType.FirstPerson, DeviceEnumerationFlags.AttachedOnly))
                {
                    deviceList.Add(new HOTASDevice(device.InstanceGuid, device.ProductName));
                }
            }
            return deviceList;
        }

        public HOTASDevice GetDevice(Guid instanceId)
        {
            return Devices.FirstOrDefault(d => d.InstanceId == instanceId);
        }

        public void ClearUnassignedActions()
        {
            foreach (var d in Devices)
            {
                d.ClearUnassignedActions();
            }
        }

        public ObservableCollection<HOTASDevice> RescanDevices()
        {
            var rescannedDevices = QueryOperatingSystemForDevices();
            var newDevices = new ObservableCollection<HOTASDevice>();

            foreach (var n in rescannedDevices)
            {
                if (Devices.Any(d => d.InstanceId == n.InstanceId)) continue;
                newDevices.Add(n);
            }

            return newDevices;
        }

        /// <summary>
        /// Activate the profile for the given Mode
        /// </summary>
        /// <param name="mode"></param>
        public void SetMode(int mode)
        {
            if (Mode == mode) return;
            Logging.Log.Info($"Mode Profile changed to: {mode}");
            Mode = mode;

            foreach (var d in Devices)
            {
                d.SetMode(mode);
            }
            ModeProfileChanged?.Invoke(this, new ModeProfileChangedEventArgs() { Mode = mode });
        }

        /// <summary>
        /// Automatically determine if a Mode button is selected. If so, activate that Mode's profile.
        /// </summary>
        public void AutoSetMode()
        {
            foreach (var d in Devices)
            {
                foreach (var map in d.ButtonMap)
                {
                    if (!(map is HOTASButtonMap buttonMap)) continue;
                    if (buttonMap.ShiftModePage <= 0) continue;

                    var isOn = d.GetButtonState(buttonMap.MapId);
                    if (!isOn) continue;

                    SetMode(buttonMap.ShiftModePage);
                    return;
                }
            }
        }

        /// <summary>
        /// The activation button should be applied across all profiles in the set to ensure the new mode can be reached from any device.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="deviceInstanceId"></param>
        /// <param name="activationButtonId"></param>
        public void ApplyActivationButton(int mode, Guid deviceInstanceId, int activationButtonId)
        {
            ModeProfileActivationButtons.Add(mode, (deviceInstanceId, activationButtonId));

            //new profiles, wont have the link to previous profiles, so we iterate the entire dictionary to sync them all 
            foreach (var ab in ModeProfileActivationButtons)
            {
                var device = Devices.FirstOrDefault(d => d.InstanceId == ab.Value.Item1);
                if (device == null) continue;

                foreach (var profile in device.ModeProfiles)
                {
                    var map = profile.Value.FirstOrDefault(m => m.MapId == ab.Value.Item2);

                    switch (map)
                    {
                        case HOTASAxisMap axisMap:
                            {
                                ApplyShiftModePage(ab.Key, ab.Value.Item2, axisMap.ButtonMap);
                                ApplyShiftModePage(ab.Key, ab.Value.Item2, axisMap.ReverseButtonMap);
                                break;
                            }
                        case HOTASButtonMap buttonMap:
                            {
                                ApplyShiftModePage(ab.Key, ab.Value.Item2, buttonMap);
                                break;
                            }
                    }
                }
            }
        }

        private static void ApplyShiftModePage(int mode, int activationButtonId, ObservableCollection<HOTASButtonMap> buttonMap)
        {
            foreach (var b in buttonMap)
            {
                ApplyShiftModePage(mode, activationButtonId, buttonMap);
            }
        }

        private static void ApplyShiftModePage(int mode, int activationButtonId, HOTASButtonMap buttonMap)
        {
            if (buttonMap.MapId == activationButtonId)
            {
                buttonMap.ShiftModePage = mode;
            }
        }
    }
}
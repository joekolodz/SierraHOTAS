using Newtonsoft.Json;
using SharpDX.DirectInput;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        [JsonProperty]
        public ObservableCollection<HOTASDevice> Devices { get; set; }

        private HOTASDevice _selectedDevice;

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
            device.ShiftModeChanged += Device_ShiftModeChanged;
            device.ListenAsync();
        }

        private void Device_ShiftModeChanged(object sender, ShiftModeChangedEventArgs e)
        {
            //swap out activities and then notify VM/UI to redraw
            if (sender is HOTASDevice device)
            {
                Logging.Log.Debug($"yo from: {device.Name}");
            }
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
    }
}
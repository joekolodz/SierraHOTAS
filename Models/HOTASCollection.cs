using Newtonsoft.Json;
using SharpDX.DirectInput;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SierraHOTAS.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HOTASCollection
    {
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
                device.Stop();
            }
        }

        /// <summary>
        /// Get all devices attached and load their custom button map profiles
        /// </summary>
        private void LoadAllDevices()
        {
            Devices = QueryOperatingSystemForDevices();
        }

        public void ListenToAllDevices()
        {
            foreach (var device in Devices)
            {
                device.ButtonPressed += Device_ButtonPressed;
                device.AxisChanged += Device_AxisChanged;
                device.ListenAsync();
            }
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

        private ObservableCollection<HOTASDevice> QueryOperatingSystemForDevices()
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
    }
}
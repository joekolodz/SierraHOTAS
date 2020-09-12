﻿using Newtonsoft.Json;
using SharpDX.DirectInput;
using SierraHOTAS.ModeProfileWindow.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SierraHOTAS.Factories;

namespace SierraHOTAS.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HOTASCollection : IHOTASCollection
    {
        private readonly JoystickFactory _joystickFactory;
        private readonly DirectInputFactory _directInputFactory;
        private readonly HOTASQueueFactory _hotasQueueFactory;

        public event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        public event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        public event EventHandler<AxisChangedEventArgs> AxisChanged;
        public event EventHandler<ModeProfileChangedEventArgs> ModeProfileChanged;
        public event EventHandler<LostConnectionToDeviceEventArgs> LostConnectionToDevice;

        [JsonProperty]
        public ObservableCollection<HOTASDevice> Devices { get; set; }

        public int Mode { get; set; } = 1;

        private HOTASDevice _selectedDevice;

        [JsonProperty]
        public Dictionary<int, ModeActivationItem> ModeProfileActivationButtons { get; }

        public void AddDevice(HOTASDevice device)
        {
            var newDevice = new HOTASDevice(_directInputFactory.CreateDirectInput(), device.DeviceId, device.Name, _hotasQueueFactory.CreateHOTASQueue());
            RebuildMapForNewDevice(device, newDevice);
        }

        public void ReplaceDevice(HOTASDevice newDevice)
        {
            var deviceToReplace = Devices.FirstOrDefault(e => e.DeviceId == newDevice.DeviceId);
            
            if (deviceToReplace == null) return;

            Devices.Remove(deviceToReplace);
            RebuildMapForNewDevice(deviceToReplace, newDevice);
        }

        private void RebuildMapForNewDevice(HOTASDevice device, HOTASDevice newDevice)
        {
            newDevice.SetButtonMap(device.ButtonMap.ToObservableCollection());
            newDevice.SetModeProfile(device.ModeProfiles);
            Devices.Add(newDevice);
        }

        public HOTASCollection(DirectInputFactory directInputFactory, JoystickFactory joystickFactory, HOTASQueueFactory hotasQueueFactory)
        {
            _directInputFactory = directInputFactory;
            _joystickFactory = joystickFactory;
            _hotasQueueFactory = hotasQueueFactory;

            Devices = new ObservableCollection<HOTASDevice>();
            ModeProfileActivationButtons = new Dictionary<int, ModeActivationItem>();
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
                StopDevice(device);
            }
        }

        private void StopDevice(HOTASDevice device)
        {
            if (device == null) return;

            device.ButtonPressed -= Device_ButtonPressed;
            device.AxisChanged -= Device_AxisChanged;
            device.KeystrokeDownSent -= Device_KeystrokeDownSent;
            device.KeystrokeUpSent -= Device_KeystrokeUpSent;
            device.ModeProfileSelected -= Device_ModeProfileSelected;

            device.Stop();
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
            device.LostConnectionToDevice += Device_LostConnectionToDevice;
            device.ListenAsync();
        }

        private void Device_LostConnectionToDevice(object sender, LostConnectionToDeviceEventArgs e)
        {
            LostConnectionToDevice?.Invoke(sender, e);
        }

        //TODO - make this public and add the ability to remove a connected device from the UI
        private void RemoveDevice(HOTASDevice device)
        {
            if (device == null) return;
            StopDevice(device);
            Devices.Remove(device);
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

        private ObservableCollection<HOTASDevice> QueryOperatingSystemForDevices()
        {
            var deviceList = new ObservableCollection<HOTASDevice>();
            var di = _directInputFactory.CreateDirectInput();
            
            foreach (var device in di.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {
                var queue = _hotasQueueFactory.CreateHOTASQueue();
                deviceList.Add(new HOTASDevice(di, _joystickFactory, device.InstanceGuid, device.ProductName, queue));
            }
            return deviceList;
        }

        public HOTASDevice GetDevice(Guid instanceId)
        {
            return Devices.FirstOrDefault(d => d.DeviceId == instanceId);
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
                var existingDevice = Devices.FirstOrDefault(d => d.DeviceId == n.DeviceId);

                if (existingDevice != null)
                {
                    if (existingDevice.Capabilities != null) continue;
                }
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





            //TODO: after removing a row, it wants to fire a selected event on the grid that still is referring to the removed row. FIX IT







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

        public bool RemoveModeProfile(ModeActivationItem item)
        {
            if (ModeProfileActivationButtons.Remove(item.Mode))
            {
                RemoveActivationButtonModeFromAllProfiles(item);
                foreach (var d in Devices)
                {
                    d.ModeProfiles.Remove(item.Mode);
                }

                Logging.Log.Debug($"DELETED Mode {item.Mode}!");
                if (ModeProfileActivationButtons.Count > 0)
                {
                    var firstMode = ModeProfileActivationButtons.Keys.FirstOrDefault();
                    SetMode(firstMode);
                    Logging.Log.Debug($"Setting Mode to {firstMode}!");
                }
                return true;
            }
            return false;
        }

        private void RemoveActivationButtonModeFromAllProfiles(ModeActivationItem item)
        {
            foreach (var button in ModeProfileActivationButtons)
            {
                var device = Devices.FirstOrDefault(d => d.DeviceId == button.Value.DeviceId);
                if (device == null) continue;

                foreach (var profile in device.ModeProfiles)
                {
                    var map = profile.Value.FirstOrDefault(m => m.MapId == item.ButtonId);

                    switch (map)
                    {
                        case HOTASAxisMap axisMap:
                            {
                                //ApplyShiftModePage(item.Key, item.Value.ButtonId, axisMap.ButtonMap);
                                //ApplyShiftModePage(item.Key, item.Value.ButtonId, axisMap.ReverseButtonMap);
                                break;
                            }
                        case HOTASButtonMap buttonMap:
                            {
                                buttonMap.ShiftModePage = 0;
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// The activation button should be applied across all profiles in the set to ensure the new mode can be reached from any device.
        /// </summary>
        public void ApplyActivationButtonToAllProfiles()
        {
            //new profiles, wont have the link to previous profiles, so we iterate the entire dictionary to sync them all 
            foreach (var item in ModeProfileActivationButtons)
            {
                var device = Devices.FirstOrDefault(d => d.DeviceId == item.Value.DeviceId);
                if (device == null) continue;

                foreach (var profile in device.ModeProfiles)
                {
                    var map = profile.Value.FirstOrDefault(m => m.MapId == item.Value.ButtonId);

                    switch (map)
                    {
                        case HOTASAxisMap axisMap:
                            {
                                ApplyShiftModePage(item.Key, item.Value.ButtonId, axisMap.ButtonMap);
                                ApplyShiftModePage(item.Key, item.Value.ButtonId, axisMap.ReverseButtonMap);
                                break;
                            }
                        case HOTASButtonMap buttonMap:
                            {
                                ApplyShiftModePage(item.Key, item.Value.ButtonId, buttonMap);
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
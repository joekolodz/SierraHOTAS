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
        private const string CURRENT_FORMAT_VERSION = "1.0";
        private readonly JoystickFactory _joystickFactory;
        private readonly DirectInputFactory _directInputFactory;
        private readonly IDirectInput _directInput;
        private readonly MediaPlayerFactory _mediaPlayerFactory;
        private readonly HOTASQueueFactory _hotasQueueFactory;
        private bool _isShiftStateActive;
        private int _previousMode;

        public event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        public event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        public event EventHandler<AxisChangedEventArgs> AxisChanged;
        public event EventHandler<ModeProfileChangedEventArgs> ModeProfileChanged;
        public event EventHandler<LostConnectionToDeviceEventArgs> LostConnectionToDevice;

        [JsonProperty]
        public string JsonFormatVersion { get; } = CURRENT_FORMAT_VERSION;

        [JsonProperty]
        public ObservableCollection<HOTASDevice> Devices { get; set; }

        public int Mode { get; set; } = 1;

        private HOTASDevice _selectedDevice;

        [JsonProperty]
        public Dictionary<int, ModeActivationItem> ModeProfileActivationButtons { get; }

        public void AddDevice(HOTASDevice device)
        {
            var newDevice = new HOTASDevice(_directInput, device.ProductId, device.DeviceId, device.Name, _hotasQueueFactory.CreateHOTASQueue());
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

        public HOTASCollection(DirectInputFactory directInputFactory, JoystickFactory joystickFactory, HOTASQueueFactory hotasQueueFactory, MediaPlayerFactory mediaPlayerFactory)
        {
            _directInputFactory = directInputFactory;
            _joystickFactory = joystickFactory;
            _hotasQueueFactory = hotasQueueFactory;
            _mediaPlayerFactory = mediaPlayerFactory;

            Devices = new ObservableCollection<HOTASDevice>();
            ModeProfileActivationButtons = new Dictionary<int, ModeActivationItem>();
            _directInput = _directInputFactory?.CreateDirectInput();

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
            device.ShiftReleased -= Device_ShiftReleased;
            device.LostConnectionToDevice -= Device_LostConnectionToDevice;

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
            if (App.IsDebug) return;
            Devices = GetHOTASDevices();
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
            device.ShiftReleased += Device_ShiftReleased;
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

        public void CopyModeProfileFromTemplate(int templateModeSource, int destinationMode)
        {
            foreach (var d in Devices)
            {
                d.CopyModeProfileFromTemplate(templateModeSource, destinationMode);
            }
        }


        private void Device_ModeProfileSelected(object sender, ModeProfileSelectedEventArgs e)
        {
            if (!(sender is HOTASDevice)) return;

            if (e.IsShift)
            {
                //don't set state to false if IsShift is false because a mode change could take place after shift button is pressed
                _isShiftStateActive = true;
                _previousMode = Mode;
            }

            SetMode(e.Mode);
        }

        private void Device_ShiftReleased(object sender, EventArgs e)
        {
            if (!_isShiftStateActive) return;
            _isShiftStateActive = false;
            SetMode(_previousMode);
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

        public ObservableCollection<HOTASDevice> GetHOTASDevices()
        {
            var rescannedDevices = _directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

            var newDevices = new ObservableCollection<HOTASDevice>();

            foreach (var n in rescannedDevices)
            {
                var existingDevice = Devices.FirstOrDefault(d => d.DeviceId == n.InstanceGuid);

                if (existingDevice != null)
                {
                    if (existingDevice.Capabilities != null) continue;
                }

                //TODO remove log
                Logging.Log.Info("adding a device");

                var queue = _hotasQueueFactory.CreateHOTASQueue();
                newDevices.Add(new HOTASDevice(_directInput, _joystickFactory, n.ProductGuid, n.InstanceGuid, n.ProductName, queue));
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
            foreach (var d in Devices.Where(m => m.IsDeviceLoaded))
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
            if (ModeProfileActivationButtons.ContainsValue(item))
            {
                RemoveActivationButtonModeFromAllProfiles(item);
                ModeProfileActivationButtons.Remove(item.Mode);

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
                                buttonMap.ResetShift();
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
                var buttonId = item.Value.ButtonId;
                var isShift = item.Value.IsShift;
                var key = item.Key;
                var deviceId = item.Value.DeviceId;

                var device = Devices.FirstOrDefault(d => d.DeviceId == deviceId);
                if (device == null) continue;

                foreach (var profile in device.ModeProfiles)
                {
                    var map = profile.Value.FirstOrDefault(m => m.MapId == buttonId);

                    switch (map)
                    {
                        case HOTASAxisMap axisMap:
                            {
                                ApplyShiftModePage(key, buttonId, axisMap.ButtonMap);
                                ApplyShiftModePage(key, buttonId, axisMap.ReverseButtonMap);
                                break;
                            }
                        case HOTASButtonMap buttonMap:
                            {
                                ApplyShiftModePage(key, buttonId, isShift, buttonMap);
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

        private static void ApplyShiftModePage(int mode, int activationButtonId, bool isShift, HOTASButtonMap buttonMap)
        {
            if (buttonMap.MapId != activationButtonId) return;

            buttonMap.ShiftModePage = mode;
            buttonMap.IsShift = isShift;
        }
    }
}
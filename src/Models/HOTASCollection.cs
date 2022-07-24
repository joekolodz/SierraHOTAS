using Newtonsoft.Json;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SierraHOTAS.Factories;

namespace SierraHOTAS.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HOTASCollection : IHOTASCollection
    {
        public const string FileFormatVersion = "1.0.0";
        private readonly JoystickFactory _joystickFactory;
        private readonly DirectInputFactory _directInputFactory;
        private readonly IDirectInput _directInput;
        private readonly HOTASQueueFactory _hotasQueueFactory;
        private readonly HOTASDeviceFactory _hotasDeviceFactory;
        private bool _isShiftStateActive;
        private int _previousMode;

        public virtual event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        public virtual event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        public virtual event EventHandler<MacroStartedEventArgs> MacroStarted;
        public virtual event EventHandler<MacroCancelledEventArgs> MacroCancelled;
        public virtual event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        public virtual event EventHandler<AxisChangedEventArgs> AxisChanged;
        public virtual event EventHandler<ModeChangedEventArgs> ModeChanged;
        public virtual event EventHandler<LostConnectionToDeviceEventArgs> LostConnectionToDevice;

        [JsonProperty]
        public string JsonFormatVersion { get; set; }

        [JsonProperty]
        public ObservableCollection<IHOTASDevice> Devices { get; set; }

        public int Mode { get; set; } = 1;

        private IHOTASDevice _selectedDevice;

        [JsonProperty]
        public ActionCatalog ActionCatalog { get; private set; }

        [JsonProperty]
        public Dictionary<int, ModeActivationItem> ModeActivationButtons { get; private set; }

        public HOTASCollection()
        {
            Initialize();
        }

        public HOTASCollection(DirectInputFactory directInputFactory, JoystickFactory joystickFactory, HOTASQueueFactory hotasQueueFactory, HOTASDeviceFactory hotasDeviceFactory, ActionCatalog actionCatalog)
        {
            _directInputFactory = directInputFactory ?? throw new ArgumentNullException(nameof(directInputFactory));
            _joystickFactory = joystickFactory ?? throw new ArgumentNullException(nameof(joystickFactory));
            _hotasQueueFactory = hotasQueueFactory ?? throw new ArgumentNullException(nameof(hotasQueueFactory));
            _hotasDeviceFactory = hotasDeviceFactory ?? throw new ArgumentNullException(nameof(hotasDeviceFactory));
            _directInput = _directInputFactory?.CreateDirectInput();
            SetCatalog(actionCatalog);
            Initialize();
        }

        private void Initialize()
        {
            Devices = new ObservableCollection<IHOTASDevice>();
            ModeActivationButtons = new Dictionary<int, ModeActivationItem>();
            JsonFormatVersion = FileFormatVersion;
        }

        public void SetCatalog(ActionCatalog catalog)
        {
            ActionCatalog = catalog;
        }

        public void AddDevice(IHOTASDevice device)
        {
            var newDevice = _hotasDeviceFactory.CreateHOTASDevice(_directInput, device.ProductId, device.DeviceId, device.Name, _hotasQueueFactory.CreateHOTASQueue());
            RebuildMapForNewDevice(device, newDevice);
        }

        public void ReplaceDevice(IHOTASDevice newDevice)
        {
            var deviceToReplace = Devices.FirstOrDefault(e => e.DeviceId == newDevice.DeviceId);

            if (deviceToReplace == null) return;

            Devices.Remove(deviceToReplace);
            RebuildMapForNewDevice(deviceToReplace, newDevice);
        }

        private void RebuildMapForNewDevice(IHOTASDevice device, IHOTASDevice newDevice)
        {
            newDevice.ApplyButtonMap(device.ButtonMap.ToObservableCollection());
            newDevice.SetMode(device.Modes);
            newDevice.SetModeActivation(ModeActivationButtons);
            Devices.Add(newDevice);
        }

        public void Start()
        {
            if (!App.IsDebug)
            {
                Devices = RefreshMissingDevices();
            }

            ListenToAllDevices();
        }

        public void Stop()
        {
            foreach (var device in Devices)
            {
                StopDevice(device);
            }
        }

        private void StopDevice(IHOTASDevice device)
        {
            if (device == null) return;

            device.ButtonPressed -= Device_ButtonPressed;
            device.AxisChanged -= Device_AxisChanged;
            device.KeystrokeDownSent -= Device_KeystrokeDownSent;
            device.KeystrokeUpSent -= Device_KeystrokeUpSent;
            device.MacroStarted -= Device_MacroStarted;
            device.MacroCancelled -= Device_MacroCancelled;
            device.ModeSelected -= device_modeSelected;
            device.ShiftReleased -= Device_ShiftReleased;
            device.LostConnectionToDevice -= Device_LostConnectionToDevice;

            device.Stop();
        }

        public void ResetProfile()
        {
            foreach (var d in Devices)
            {
                d.Reset();
            }
            
            Mode = 1;
            ActionCatalog.Clear();
            ModeActivationButtons.Clear();
        }

        public void ApplyButtonMapToAllProfiles()
        {
            foreach (var d in Devices)
            {
                d.OverlayAllModesToDevice();
            }
        }

        public void ListenToAllDevices()
        {
            foreach (var device in Devices)
            {
                ListenToDevice(device);
            }
        }

        public void ListenToDevice(IHOTASDevice device)
        {
            device.ButtonPressed += Device_ButtonPressed;
            device.AxisChanged += Device_AxisChanged;
            device.KeystrokeDownSent += Device_KeystrokeDownSent;
            device.KeystrokeUpSent += Device_KeystrokeUpSent;
            device.MacroStarted += Device_MacroStarted;
            device.MacroCancelled += Device_MacroCancelled;
            device.ModeSelected += device_modeSelected;
            device.ShiftReleased += Device_ShiftReleased;
            device.LostConnectionToDevice += Device_LostConnectionToDevice;
            
            device.SetModeActivation(ModeActivationButtons);
            device.ListenAsync();
        }

        private void Device_LostConnectionToDevice(object sender, LostConnectionToDeviceEventArgs e)
        {
            LostConnectionToDevice?.Invoke(sender, e);
        }

        //TODO - make this public and add the ability to remove a connected device from the UI
        //you'd want to do this to keep the save file size down; or during save, remove items from the collection that don't have any assigned actions
        [ExcludeFromCodeCoverage]
        private void RemoveDevice(IHOTASDevice device)
        {
            if (device == null) return;
            StopDevice(device);
            Devices.Remove(device);
        }

        public int SetupNewMode()
        {
            var newMode = 0;
            foreach (var d in Devices)
            {
                newMode = d.SetupNewMode();
            }

            return newMode;
        }

        public void CopyModeFromTemplate(int templateModeSource, int destinationMode)
        {
            foreach (var d in Devices)
            {
                d.CopyModeFromTemplate(templateModeSource, destinationMode);
            }
        }


        private void device_modeSelected(object sender, ModeSelectedEventArgs e)
        {
            if (!(sender is IHOTASDevice)) return;

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

        private void Device_MacroStarted(object sender, MacroStartedEventArgs e)
        {
            MacroStarted?.Invoke(sender, e);
        }

        private void Device_MacroCancelled(object sender, MacroCancelledEventArgs e)
        {
            MacroCancelled?.Invoke(sender, e);
        }

        public void ForceButtonPress(IHOTASDevice device, JoystickOffset offset, bool isDown)
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

        public IHOTASDevice GetDevice(Guid instanceId)
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

        /// <summary>
        /// Rescan all devices from DirectInput. Any devices scanned that are not already in the Device list will be added
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<IHOTASDevice> RefreshMissingDevices()
        {
            var rescannedDevices = _directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

            var newDevices = new ObservableCollection<IHOTASDevice>();

            if (rescannedDevices == null) return newDevices;

            foreach (var n in rescannedDevices)
            {
                var existingDevice = Devices.FirstOrDefault(d => d.DeviceId == n.InstanceGuid);

                if (existingDevice != null)
                {
                    if (existingDevice.Capabilities != null) continue;
                }

                var queue = _hotasQueueFactory.CreateHOTASQueue();
                var device = _hotasDeviceFactory.CreateHOTASDevice(_directInput, _joystickFactory, n.ProductGuid, n.InstanceGuid, n.ProductName, queue);
                newDevices.Add(device);
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
            Logging.Log.Debug($"Mode Profile changed to: {mode}");
            Mode = mode;

            foreach (var d in Devices)
            {
                d.SetMode(mode);
            }
            ModeChanged?.Invoke(this, new ModeChangedEventArgs() { Mode = mode });
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
                    if (!(map is HOTASButton buttonMap)) continue;
                    if (buttonMap.ShiftModePage <= 0) continue;

                    var isOn = d.GetButtonState(buttonMap.MapId);
                    if (!isOn) continue;

                    SetMode(buttonMap.ShiftModePage);
                    return;
                }
            }
        }

        public bool RemoveMode(ModeActivationItem item)
        {
            if (ModeActivationButtons.ContainsValue(item))
            {
                RemoveActivationButtonModeFromAllProfiles(item);
                ModeActivationButtons.Remove(item.Mode);

                foreach (var d in Devices)
                {
                    d.RemoveMode(item.Mode);
                } 

                Logging.Log.Debug($"DELETED Mode {item.Mode}!");
                
                if (ModeActivationButtons.Count <= 0) return true;

                var firstMode = ModeActivationButtons.Keys.FirstOrDefault();
                SetMode(firstMode);
                Logging.Log.Debug($"Setting Mode to {firstMode}!");
                return true;
            }
            return false;
        }

        private void RemoveActivationButtonModeFromAllProfiles(ModeActivationItem item)
        {
            foreach (var button in ModeActivationButtons)
            {
                var device = Devices.FirstOrDefault(d => d.DeviceId == button.Value.DeviceId);
                if (device == null) continue;

                foreach (var profile in device.Modes)
                {
                    var map = profile.Value.FirstOrDefault(m => m.MapId == item.ButtonId);

                    switch (map)
                    {
                        case HOTASAxis axisMap:
                            {
                                //ApplyShiftModePage(item.Key, item.Value.ButtonId, axisMap.ButtonMap);
                                //ApplyShiftModePage(item.Key, item.Value.ButtonId, axisMap.ReverseButtonMap);
                                break;
                            }
                        case HOTASButton buttonMap:
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
            //new profiles wont have the link to previous profiles, so we iterate the entire dictionary to sync them all 
            foreach (var item in ModeActivationButtons)
            {
                var buttonId = item.Value.ButtonId;
                var isShift = item.Value.IsShift;
                var key = item.Key;
                var deviceId = item.Value.DeviceId;

                var device = Devices.FirstOrDefault(d => d.DeviceId == deviceId);
                if (device == null) continue;

                foreach (var profile in device.Modes)
                {
                    var map = profile.Value.FirstOrDefault(m => m.MapId == buttonId);

                    switch (map)
                    {
                        case HOTASAxis axisMap:
                            {
                                ApplyShiftModePage(key, buttonId, axisMap.ButtonMap);
                                ApplyShiftModePage(key, buttonId, axisMap.ReverseButtonMap);
                                break;
                            }
                        case HOTASButton buttonMap:
                            {
                                ApplyShiftModePage(key, buttonId, isShift, buttonMap);
                                break;
                            }
                    }
                }
            }
        }

        private static void ApplyShiftModePage(int mode, int activationButtonId, ObservableCollection<HOTASButton> buttonMap)
        {
            foreach (var b in buttonMap)
            {
                ApplyShiftModePage(mode, activationButtonId, false, b);
            }
        }

        private static void ApplyShiftModePage(int mode, int activationButtonId, bool isShift, HOTASButton button)
        {
            if (button.MapId != activationButtonId) return;

            button.ShiftModePage = mode;
            button.IsShift = isShift;
        }
    }
}
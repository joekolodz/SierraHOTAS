using Newtonsoft.Json;
using SharpDX.DirectInput;
using SierraHOTAS.Factories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SierraHOTAS.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HOTASDevice : IHOTASDevice
    {
        private readonly JoystickFactory _joystickFactory;

        public event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        public event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        public event EventHandler<MacroStartedEventArgs> MacroStarted;
        public event EventHandler<MacroCancelledEventArgs> MacroCancelled;
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        public event EventHandler<ModeProfileSelectedEventArgs> ModeProfileSelected;
        public event EventHandler<EventArgs> ShiftReleased;
        public event EventHandler<AxisChangedEventArgs> AxisChanged;
        public event EventHandler<LostConnectionToDeviceEventArgs> LostConnectionToDevice;

        [JsonProperty]
        public Guid DeviceId { get; set; }

        [JsonProperty]
        public Guid ProductId { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        public Capabilities Capabilities { get; set; }

        public bool IsDeviceLoaded => Capabilities != null;

        public ObservableCollection<IHotasBaseMap> ButtonMap { get; private set; } = new ObservableCollection<IHotasBaseMap>();


        [JsonProperty]
        [JsonConverter(typeof(CustomJsonConverter))]
        public Dictionary<int, ObservableCollection<IHotasBaseMap>> ModeProfiles { get; private set; } = new Dictionary<int, ObservableCollection<IHotasBaseMap>>();

        private readonly IDirectInput _directInput;
        private IJoystick Joystick { get; set; }
        private IHOTASQueue _hotasQueue;
        private Dictionary<int, ModeActivationItem> _modeProfileActivationButtons;

        public HOTASDevice() { }

        public HOTASDevice(IDirectInput directInput, Guid productGuid, Guid deviceId, string name, IHOTASQueue hotasQueue)
        {
            _directInput = directInput ?? throw new ArgumentNullException(nameof(directInput));
            _hotasQueue = hotasQueue ?? throw new ArgumentNullException(nameof(hotasQueue));

            if (deviceId == Guid.Empty) return; //can occur when loading an unsupported json format and the device id isn't deserialized correctly or a non-connected device that was previously saved
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            DeviceId = deviceId;
            ProductId = productGuid;
            Name = name;
            InitializeModeProfile();
        }

        public HOTASDevice(IDirectInput directInput, JoystickFactory joystickFactory, Guid productGuid, Guid deviceId, string name, IHOTASQueue hotasQueue) :
            this(directInput, productGuid, deviceId, name, hotasQueue)
        {
            _directInput = directInput ?? throw new ArgumentNullException(nameof(directInput));
            _joystickFactory = joystickFactory ?? throw new ArgumentNullException(nameof(joystickFactory));
            _hotasQueue = hotasQueue ?? throw new ArgumentNullException(nameof(hotasQueue));
            if (deviceId == Guid.Empty) throw new ArgumentNullException(nameof(deviceId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            if (App.IsDebug) return;
            AcquireJoystick();
            LoadCapabilitiesMapping();
        }

        public void RemoveModeProfile(int mode)
        {
            ModeProfiles.Remove(mode);
            if (ModeProfiles.Count == 0)
            {
                InitializeModeProfile();
            }
        }

        private void InitializeModeProfile()
        {
            ModeProfiles.Add(1, ButtonMap);
        }

        public void SetModeProfile(Dictionary<int, ObservableCollection<IHotasBaseMap>> profile)
        {
            ModeProfiles = profile;
            if (ModeProfiles.Count < 1) return;
            ApplyButtonMap(profile[ModeProfiles.Keys.Min()].ToObservableCollection());
        }

        //todo pass in the mode profile key for the profile you want to template from
        public int SetupNewModeProfile()
        {
            var maxKey = ModeProfiles.OrderByDescending(x => x.Key).First();
            var newMode = maxKey.Key + 1;

            var newButtonMap = new ObservableCollection<IHotasBaseMap>();
            ModeProfiles.Add(newMode, newButtonMap);

            //create an empty button map, but do not switch to it yet
            SeedButtonMapProfileFromDeviceCapabilities(newButtonMap);

            return newMode;
        }

        public void CopyModeProfileFromTemplate(int templateModeSource, int destinationMode)
        {
            if (!ModeProfiles.ContainsKey(templateModeSource)) return;

            var sourceMap = ModeProfiles[templateModeSource];

            var isDestinationMapFound = ModeProfiles.TryGetValue(destinationMode, out var destinationMap);
            if (!isDestinationMapFound)
            {
                //if a mode is expected to be there but isn't. rebuild its profile list from the first entry (mode=1)
                var firstMap = ModeProfiles[1];
                var modeCount = ModeProfiles.Count;
                var missingMapCount = destinationMode - modeCount;
                for (var i = 1; i <= missingMapCount; i++)
                {
                    ModeProfiles.Add(i + modeCount, firstMap.ToObservableCollection());
                }
                destinationMap = ModeProfiles[destinationMode];
            }

            destinationMap.Clear();
            CopyButtonMapProfile(sourceMap, destinationMap);
        }

        public static void CopyButtonMapProfile(ObservableCollection<IHotasBaseMap> source, ObservableCollection<IHotasBaseMap> destination)
        {
            foreach (var map in source)
            {
                switch (map)
                {
                    case HOTASAxis axisMap:
                        {
                            var newAxisMap = new HOTASAxis()
                            {
                                MapId = axisMap.MapId,
                                MapName = axisMap.MapName,
                                Type = axisMap.Type,
                                IsDirectional = axisMap.IsDirectional,
                                IsMultiAction = axisMap.IsMultiAction,
                                SoundFileName = axisMap.SoundFileName,
                                SoundVolume = axisMap.SoundVolume,
                                Segments = axisMap.Segments.ToObservableCollection()
                            };

                            foreach (var b in axisMap.ButtonMap)
                            {
                                var newMap = BuildButtonMap(b);
                                newAxisMap.ButtonMap.Add(newMap);
                            }

                            foreach (var b in axisMap.ReverseButtonMap)
                            {
                                var newMap = BuildButtonMap(b);
                                newAxisMap.ReverseButtonMap.Add(newMap);
                            }
                            destination.Add(newAxisMap);
                            break;
                        }
                    case HOTASButton buttonMap:
                        {
                            var newMap = BuildButtonMap(buttonMap);
                            destination.Add(newMap);
                            break;
                        }
                }
            }
        }

        private static HOTASButton BuildButtonMap(HOTASButton map)
        {
            var newMap = new HOTASButton
            {
                MapId = map.MapId,
                MapName = map.MapName,
                ShiftModePage = 0,
                ActionName = map.ActionName,
                Type = map.Type,
                ActionCatalogItem = new ActionCatalogItem()
                {
                    ActionName = map.ActionCatalogItem.ActionName,
                    NoAction = map.ActionCatalogItem.NoAction,
                    Actions = map.ActionCatalogItem.Actions.ToObservableCollection()
                }
            };
            return newMap;
        }

        //public void ReAcquireJoystick()
        //{
        //    Joystick?.Unacquire();
        //    Joystick?.Dispose();
        //    AcquireJoystick();
        //}

        private void AcquireJoystick()
        {
            Joystick = _joystickFactory.CreateJoystick(_directInput, DeviceId);
            Joystick.BufferSize = 4096;
            Joystick.Acquire();
        }


        public void ListenAsync()
        {
            if (_modeProfileActivationButtons == null) throw new InvalidOperationException("ModeProfileActivationButtons must be set before listening to device");

            RemoveQueueHandlers();
            AddQueueHandlers();
            _hotasQueue.Listen(Joystick, ModeProfiles, _modeProfileActivationButtons);
        }

        private void AddQueueHandlers()
        {
            _hotasQueue.KeystrokeDownSent += OnKeystrokeDownSent;
            _hotasQueue.KeystrokeUpSent += OnKeystrokeUpSent;
            _hotasQueue.MacroStarted += OnMacroStarted;
            _hotasQueue.MacroCancelled += OnMacroCancelled;
            _hotasQueue.ButtonPressed += OnButtonPress;
            _hotasQueue.AxisChanged += OnAxisChanged;
            _hotasQueue.ModeProfileSelected += OnModeProfileSelected;
            _hotasQueue.ShiftReleased += OnShiftReleased;
            _hotasQueue.LostConnectionToDevice += OnLostConnectionToDevice;
        }

        private void RemoveQueueHandlers()
        {
            _hotasQueue.KeystrokeDownSent -= OnKeystrokeDownSent;
            _hotasQueue.KeystrokeUpSent -= OnKeystrokeUpSent;
            _hotasQueue.MacroStarted -= OnMacroStarted;
            _hotasQueue.MacroCancelled -= OnMacroCancelled;
            _hotasQueue.ButtonPressed -= OnButtonPress;
            _hotasQueue.AxisChanged -= OnAxisChanged;
            _hotasQueue.ModeProfileSelected -= OnModeProfileSelected;
            _hotasQueue.ShiftReleased -= OnShiftReleased;
            _hotasQueue.LostConnectionToDevice -= OnLostConnectionToDevice;
        }

        public void OverlayAllProfilesToDevice()
        {
            var mergedModeProfiles = new Dictionary<int, ObservableCollection<IHotasBaseMap>>();
            var deviceButtons = new ObservableCollection<IHotasBaseMap>();
            SeedButtonMapProfileFromDeviceCapabilities(deviceButtons);

            foreach (var p in ModeProfiles)
            {
                var d = deviceButtons.ToObservableCollection();//make a copy since we could have more than 1 profile and we don't need to rescan caps
                mergedModeProfiles.Add(p.Key, MergeMaps(d, p.Value));
            }

            ModeProfiles = mergedModeProfiles;
        }

        private ObservableCollection<IHotasBaseMap> MergeMaps(ObservableCollection<IHotasBaseMap> sourceMap, ObservableCollection<IHotasBaseMap> destinationMap)
        {
            var merged = new ObservableCollection<IHotasBaseMap>();
            foreach (var source in sourceMap)
            {
                var i = destinationMap.FirstOrDefault(b => b.MapId == source.MapId);
                merged.Add(i ?? source);
            }
            return merged;
        }


        /// <summary>
        /// Re-seed the device before overlaying the given button map
        /// </summary>
        /// <param name="existingButtonMap"></param>
        public void ApplyButtonMap(ObservableCollection<IHotasBaseMap> existingButtonMap)
        {
            if (IsDeviceLoaded)
            {
                //Reset the button map back to default hardware capabilities and then overlay the given button map on top
                //We rebuild the button map in this manner, because additional hardware buttons may be recognized at a later time as well as the fact that the map loaded from the JSON file only has modified buttons
                //For instance, the virpil side cars can be linked to an existing device which makes that original device look like it has more buttons.
                //existingButtonMap is assumed to have less button entries in this scenario. So we want to copy the data from existingButtonMap for any buttons that match between the two lists
                //Any buttons that don't match means that the device has more buttons than are in the existingButtonMap list and so there is nothing to copy
                //If the device has fewer buttons than exist on the existingButtonMap, then those buttons are not copied over and lost
                ButtonMap = new ObservableCollection<IHotasBaseMap>();
                SeedButtonMapProfileFromDeviceCapabilities(ButtonMap);
                foreach (var source in existingButtonMap)
                {
                    var i = ButtonMap.FirstOrDefault(b => b.MapId == source.MapId);
                    if (i == null) continue;

                    var index = ButtonMap.IndexOf(i);
                    ButtonMap[index] = source;
                }
            }
            else
            {
                //if the device isn't loaded, then copy from the profile directly
                ButtonMap = existingButtonMap;
            }
        }

        public void SetMode(int mode)
        {
            if (!ModeProfiles.ContainsKey(mode))
            {
                Logging.Log.Debug($"Tried to change device to Mode {mode}, but there was no profile for it. Profile will remain unchanged. Device ID: {DeviceId}, Device Name: {Name}");
                return;
            }
            ButtonMap = ModeProfiles[mode];
            _hotasQueue.SetMode(mode);
        }

        public void SetModeActivation(Dictionary<int, ModeActivationItem> modeProfileActivationButtons)
        {
            _modeProfileActivationButtons = modeProfileActivationButtons;
        }

        public void ForceButtonPress(JoystickOffset offset, bool isDown)
        {
            _hotasQueue.ForceButtonPress(offset, isDown);
        }

        private void LoadCapabilitiesMapping()
        {
            LoadCapabilities();
            SeedButtonMapProfileFromDeviceCapabilities(ButtonMap);
        }

        private void SeedButtonMapProfileFromDeviceCapabilities(ObservableCollection<IHotasBaseMap> buttonMap)
        {
            if (Capabilities?.AxeCount > 0) SeedAxisMap(Capabilities.AxeCount, buttonMap);
            if (Capabilities?.ButtonCount > 0) SeedButtonMap(JoystickOffset.Button1, Capabilities.ButtonCount, HOTASButton.ButtonType.Button, buttonMap);
            if (Capabilities?.PovCount > 0) SeedPointOfViewMap(JoystickOffset.POV1, Capabilities.PovCount, HOTASButton.ButtonType.POV, buttonMap);
        }

        private void LoadCapabilities()
        {
            if (Joystick == null) return;

            Logging.Log.Debug($"\nLoading device capabilities for ...{Name}");

            Capabilities = Joystick.Capabilities;

            Logging.Log.Debug("AxeCount {0}", Capabilities.AxeCount);
            Logging.Log.Debug("ButtonCount {0}", Capabilities.ButtonCount);
            Logging.Log.Debug("PovCount {0}", Capabilities.PovCount);
            Logging.Log.Debug("Flags {0}", Capabilities.Flags);
        }

        private void SeedPointOfViewMap(JoystickOffset startFrom, int length, HOTASButton.ButtonType type, ObservableCollection<IHotasBaseMap> buttonMap)
        {
            var indexStart = JoystickOffsetValues.GetIndex(startFrom.ToString());
            for (var count = indexStart; count < indexStart + length; count++)
            {
                var offset = JoystickOffsetValues.GetOffset(count);

                //each of the eight POV positions needs a unique offset number so that we don't have to have a compound index to do lookups with later.
                //so POV1 Offset is 0x00000020 and the value of the EAST position = 0x2328, then assign translated offset of 0x23280020
                //so POV2 Offset is 0x00000024 and the value of the SOUTH EAST position = 0x34BC then assign translated offset of 0x34BC0024
                for (uint position = 0; position < 8; position++)
                {
                    var translatedOffset = HOTASQueue.TranslatePointOfViewOffset(offset, 4500 * (int)position);

                    buttonMap.Add(new HOTASButton()
                    {
                        MapId = (int)translatedOffset,
                        Type = type,
                        MapName = Enum.GetName(typeof(JoystickOffsetValues.PointOfViewPositionValues), 4500 * position)
                    });
                }
            }
        }

        private void SeedAxisMap(int deviceAxeCount, ObservableCollection<IHotasBaseMap> buttonMap)
        {
            var foundDevices = 0;
            for (var i = 0; i < JoystickOffsetValues.AxisNames.Length; i++)
            {
                if (!Joystick.IsAxisPresent(JoystickOffsetValues.AxisNames[i])) continue;

                var offset = JoystickOffsetValues.GetOffset(i);
                var axisType = HOTASButton.ButtonType.AxisLinear;

                if (offset == JoystickOffset.RX ||
                    offset == JoystickOffset.RY ||
                    offset == JoystickOffset.RZ)
                {
                    axisType = HOTASButton.ButtonType.AxisRadial;
                }

                buttonMap.Add(new HOTASAxis()
                {
                    MapId = (int)offset,
                    Type = axisType,
                    MapName = $"{JoystickOffsetValues.GetName(offset)}"
                });

                if (++foundDevices >= deviceAxeCount) break;
            }
        }

        private void SeedButtonMap(JoystickOffset startFrom, int length, HOTASButton.ButtonType type, ObservableCollection<IHotasBaseMap> buttonMap)
        {
            var indexStart = JoystickOffsetValues.GetIndex(startFrom.ToString());
            for (var count = indexStart; count < indexStart + length; count++)
            {
                var offset = JoystickOffsetValues.GetOffset(count);
                buttonMap.Add(new HOTASButton()
                {
                    MapId = (int)offset,
                    Type = type,
                    MapName = $"{JoystickOffsetValues.GetName(offset)}"
                });
            }
        }

        private void OnKeystrokeUpSent(object sender, KeystrokeSentEventArgs e)
        {
            KeystrokeUpSent?.Invoke(sender, e);
        }

        private void OnKeystrokeDownSent(object sender, KeystrokeSentEventArgs e)
        {
            KeystrokeDownSent?.Invoke(sender, e);
        }

        private void OnMacroStarted(object sender, MacroStartedEventArgs e)
        {
            MacroStarted?.Invoke(sender, e);
        }

        private void OnMacroCancelled(object sender, MacroCancelledEventArgs e)
        {
            MacroCancelled?.Invoke(sender, e);
        }

        private void OnButtonPress(object sender, ButtonPressedEventArgs e)
        {
            ButtonPressed?.Invoke(this, new ButtonPressedEventArgs() { ButtonId = e.ButtonId, Device = this });
        }

        private void OnAxisChanged(object sender, AxisChangedEventArgs e)
        {
            e.Device = this;
            AxisChanged?.Invoke(this, e);
        }

        public void Stop()
        {
            _hotasQueue?.Stop();
        }

        private void OnModeProfileSelected(object sender, ModeProfileSelectedEventArgs e)
        {
            ModeProfileSelected?.Invoke(this, e);
        }

        private void OnShiftReleased(object sender, EventArgs e)
        {
            ShiftReleased?.Invoke(sender, e);
        }

        private void OnLostConnectionToDevice(object sender, EventArgs e)
        {
            Joystick = null;
            Capabilities = null;
            LostConnectionToDevice?.Invoke(sender, new LostConnectionToDeviceEventArgs(this));
        }

        public void ClearUnassignedActions()
        {
            foreach (var m in ButtonMap)
            {
                m.ClearUnassignedActions();
            }
        }

        public void ClearButtonMap()
        {
            ButtonMap.Clear();
            if (App.IsDebug) return;
            LoadCapabilitiesMapping();
        }

        public bool GetButtonState(int mapId)
        {
            var rawOffset = JoystickOffsetValues.GetButtonIndexForJoystickState(mapId);
            var js = new JoystickState();
            Joystick.GetCurrentState(ref js);
            return js.Buttons[rawOffset];
        }
    }
}

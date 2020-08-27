﻿using Newtonsoft.Json;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace SierraHOTAS.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HOTASDevice
    {
        public event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        public event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        public event EventHandler<ModeProfileSelectedEventArgs> ModeProfileSelected;
        public event EventHandler<AxisChangedEventArgs> AxisChanged;

        [JsonProperty]
        public Guid DeviceId { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        public Capabilities Capabilities { get; set; }

        public ObservableCollection<IHotasBaseMap> ButtonMap
        {
            get => _buttonMap;
            private set => _buttonMap = value;
        }


        [JsonProperty]
        [JsonConverter(typeof(CustomJsonConverter))]
        public Dictionary<int, ObservableCollection<IHotasBaseMap>> ModeProfiles { get; private set; }



        private Joystick Joystick { get; set; }
        private HOTASQueue _hotasQueue;
        private ObservableCollection<IHotasBaseMap> _buttonMap;


        public HOTASDevice()
        {
            InitializeModeProfileDictionary();
        }

        public HOTASDevice(Guid deviceId, string name)
        {
            if (deviceId == Guid.Empty || deviceId == null || string.IsNullOrWhiteSpace(name))
            {
                throw new NullReferenceException("Information about Joystick is unavailable (this should only disable the Save and Load options");
            }

            DeviceId = deviceId;
            Name = name;
            InitializeModeProfileDictionary();

            Initialize();
        }

        private void InitializeModeProfileDictionary()
        {
            ModeProfiles = new Dictionary<int, ObservableCollection<IHotasBaseMap>>();
            ButtonMap = new ObservableCollection<IHotasBaseMap>();
            ModeProfiles.Add(1, ButtonMap);
        }

        public void SetModeProfile(Dictionary<int, ObservableCollection<IHotasBaseMap>> profile)
        {
            ModeProfiles = profile;
            SetButtonMap(profile[ModeProfiles.Keys.Min()].ToObservableCollection());
        }

        public int SetupNewModeProfile()
        {
            var maxKey = ModeProfiles.OrderByDescending(x=>x.Key).First();
            var newMode = maxKey.Key + 1;

            var newButtonMap = new ObservableCollection<IHotasBaseMap>();
            ModeProfiles.Add(newMode, newButtonMap);

            //create the button map, but do not switch to it yet
            CopyButtonMapProfile(ModeProfiles[maxKey.Key], newButtonMap);

            return newMode;
        }

        private void CopyButtonMapProfile(ObservableCollection<IHotasBaseMap> source, ObservableCollection<IHotasBaseMap> destination)
        {
            foreach (var map in source)
            {
                switch (map)
                {
                    case HOTASAxisMap axisMap:
                        {
                            var newAxisMap = new HOTASAxisMap()
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
                                var newMap = BuildAxisButtonMap(b);
                                newAxisMap.ButtonMap.Add(newMap);
                            }

                            foreach (var b in axisMap.ReverseButtonMap)
                            {
                                var newMap = BuildAxisButtonMap(b);
                                newAxisMap.ReverseButtonMap.Add(newMap);
                            }
                            destination.Add(newAxisMap);
                            break;
                        }
                    case HOTASButtonMap buttonMap:
                        {
                            var newMap = BuildAxisButtonMap(buttonMap);
                            destination.Add(newMap);
                            break;
                        }
                }
            }
        }

        private static HOTASButtonMap BuildAxisButtonMap(HOTASButtonMap map)
        {
            var newMap = new HOTASButtonMap
            {
                MapId = map.MapId,
                MapName = map.MapName,
                ShiftModePage = 0,
                ActionName = map.ActionName,
                Type = map.Type,
                ActionCatalogItem = new ActionCatalogItem()
                {
                    ActionName = map.ActionCatalogItem.ActionName,
                    NoAction = false,
                    Actions = map.ActionCatalogItem.Actions.ToObservableCollection()
                }
            };
            return newMap;
        }

        private void Initialize()
        {
            if (MainWindow.IsDebug) return;
            AcquireJoystick();
            LoadCapabilitiesMapping();
        }

        public void ReAcquireJoystick()
        {
            Joystick?.Unacquire();
            Joystick?.Dispose();
            AcquireJoystick();
        }

        private void AcquireJoystick()
        {
            var i = new DirectInput();
            Joystick = new Joystick(i, DeviceId);
            Joystick.Properties.BufferSize = 4096;
            Joystick.Acquire();
        }

        public void ListenAsync()
        {
            _hotasQueue = new HOTASQueue();
            _hotasQueue.KeystrokeDownSent += OnKeystrokeDownSent;
            _hotasQueue.KeystrokeUpSent += OnKeystrokeUpSent;
            _hotasQueue.ButtonPressed += OnButtonPress;
            _hotasQueue.AxisChanged += OnAxisChanged;
            _hotasQueue.ModeProfileSelected += OnModeProfileSelected;
            _hotasQueue.ListenAsync(Joystick, ButtonMap);

            Debug.WriteLine($"\n\nListening for joystick events ({Name})...!");
        }

        public void SetButtonMap(ObservableCollection<IHotasBaseMap> buttonMap)
        {
            ButtonMap = buttonMap;
        }

        public void SetMode(int mode)
        {
            if (!ModeProfiles.ContainsKey(mode))
            {
                Logging.Log.Warn($"Tried to change device to Mode {mode}, but there was no profile for it. Profile will remain unchanged. Device ID: {DeviceId}, Device Name: {Name}");
                return;
            }
            ButtonMap = ModeProfiles[mode];
            _hotasQueue.SetButtonMap(ButtonMap);
        }

        public void ForceButtonPress(JoystickOffset offset, bool isDown)
        {
            _hotasQueue.ForceButtonPress(offset, isDown);
        }

        private void LoadCapabilitiesMapping()
        {
            LoadCapabilities();

            Debug.WriteLine("\nBuilding button maps...");

            BuildButtonMapProfile();
        }

        private void BuildButtonMapProfile()
        {
            if (Capabilities.AxeCount > 0) SeedAxisMap(JoystickOffset.X, 6);
            if (Capabilities.ButtonCount > 0) SeedButtonMap(JoystickOffset.Buttons0, Capabilities.ButtonCount, HOTASButtonMap.ButtonType.Button);
            if (Capabilities.PovCount > 0) SeedPointOfViewMap(JoystickOffset.PointOfViewControllers0, Capabilities.PovCount, HOTASButtonMap.ButtonType.POV);
        }

        public void LoadCapabilities()
        {
            Debug.WriteLine($"\nLoading device capabilities for ...{Name}");

            Capabilities = Joystick.Capabilities;

            Debug.WriteLine("AxeCount {0}", Capabilities.AxeCount);
            Debug.WriteLine("ButtonCount {0}", Capabilities.ButtonCount);
            Debug.WriteLine("PovCount {0}", Capabilities.PovCount);
            Debug.WriteLine("Flags {0}", Capabilities.Flags);
        }

        private void SeedPointOfViewMap(JoystickOffset startFrom, int length, HOTASButtonMap.ButtonType type)
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
                    var translatedOffset = HOTAS.TranslatePointOfViewOffset(offset, 4500 * position);

                    ButtonMap.Add(new HOTASButtonMap()
                    {
                        MapId = (int)translatedOffset,
                        Type = type,
                        MapName = Enum.GetName(typeof(JoystickOffsetValues.PointOfViewPositionValues), 4500 * position)
                    });
                }
            }
        }

        private void SeedAxisMap(JoystickOffset startFrom, int length)
        {
            var indexStart = JoystickOffsetValues.GetIndex(startFrom.ToString());
            for (var count = indexStart; count < indexStart + length; count++)
            {
                var offset = JoystickOffsetValues.GetOffset(count);
                var axisType = HOTASButtonMap.ButtonType.AxisLinear;

                if (offset == JoystickOffset.RotationX ||
                    offset == JoystickOffset.RotationY ||
                    offset == JoystickOffset.RotationZ)
                {
                    axisType = HOTASButtonMap.ButtonType.AxisRadial;
                }

                ButtonMap.Add(new HOTASAxisMap()
                {
                    MapId = (int)offset,
                    Type = axisType,
                    MapName = $"{JoystickOffsetValues.GetName(offset)}"
                });
            }
        }

        private void SeedButtonMap(JoystickOffset startFrom, int length, HOTASButtonMap.ButtonType type)
        {
            var indexStart = JoystickOffsetValues.GetIndex(startFrom.ToString());
            for (var count = indexStart; count < indexStart + length; count++)
            {
                var offset = JoystickOffsetValues.GetOffset(count);
                ButtonMap.Add(new HOTASButtonMap()
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
            if (MainWindow.IsDebug) return;
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
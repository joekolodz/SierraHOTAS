using SharpDX.DirectInput;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SierraHOTAS.Models
{
    public class HOTASQueue : IHOTASQueue
    {
        public event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        public event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        public event EventHandler<MacroStartedEventArgs> MacroStarted;
        public event EventHandler<MacroCancelledEventArgs> MacroCancelled;
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        public event EventHandler<ButtonPressedEventArgs> ButtonReleased;
        public event EventHandler<AxisChangedEventArgs> AxisChanged;
        public event EventHandler<ModeProfileSelectedEventArgs> ModeProfileSelected;
        public event EventHandler ShiftReleased;
        public event EventHandler LostConnectionToDevice;

        private bool _isStopRequested;

        private BlockingCollection<ActionJobItem> _actionJobs;

        private Dictionary<int, JitterDetection> _jitterDetectionDictionary;


        private IJoystick Joystick { get; set; }
        private ObservableCollection<IHotasBaseMap> _buttonMap;

        public void Listen(IJoystick joystick, ObservableCollection<IHotasBaseMap> buttonMap)
        {
            Joystick = joystick;
            _buttonMap = buttonMap;
            _jitterDetectionDictionary = new Dictionary<int, JitterDetection>();

            _actionJobs = new BlockingCollection<ActionJobItem>();

            _isStopRequested = false;
            Task.Factory.StartNew(ListenLoop, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(DequeueLoop, TaskCreationOptions.LongRunning);
        }

        public void ForceButtonPress(JoystickOffset offset, bool isDown)
        {
            HandleStandardButton((int)offset, isDown ? 128 : 0);
        }

        public void Stop()
        {
            if (App.IsDebug) return;
            _actionJobs.CompleteAdding();
            _isStopRequested = true;
        }

        public static int TranslatePointOfViewOffset(JoystickOffset offset, int value)
        {
            var translatedOffset = value;
            translatedOffset <<= 8;
            translatedOffset |= (int)offset;
            return translatedOffset;
        }

        private void ListenLoop()
        {
            while (!_isStopRequested)
            {
                if (Joystick == null) return;
                Thread.Sleep(10);//give CPU back

                //if exception due to lost device, then break out of loop so the user can refresh the device list and start over
                Joystick.Poll();
                JoystickUpdate[] data = { };
                try
                {
                    data = Joystick.GetBufferedData();
                }
                catch (Exception e)
                {
                    Logging.Log.Info(e);
                    _actionJobs.CompleteAdding();
                    _isStopRequested = true;
                    LostConnectionToDevice?.Invoke(this, EventArgs.Empty);
                }

                foreach (var state in data)
                {
                    var offset = (JoystickOffset)state.Offset;
                    if (offset >= JoystickOffset.Button1 && offset <= JoystickOffset.Button128)
                    {
                        Logging.Log.Debug($"Offset:{offset}({state.RawOffset}), Seq:{state.Sequence}, Value:{state.Value}");
                        HandleStandardButton((int)offset, state.Value);
                        continue;
                    }

                    if (offset == JoystickOffset.POV1 ||
                        offset == JoystickOffset.POV2 ||
                        offset == JoystickOffset.POV3 ||
                        offset == JoystickOffset.POV4)
                    {
                        Logging.Log.Debug($"Offset:{offset}({state.RawOffset}), POV:{TranslatePointOfViewOffset(offset, state.Value)}, Seq:{state.Sequence}, Value:{state.Value}");
                        HandlePovButton((JoystickOffset)state.Offset, state.Value);
                        continue;
                    }

                    if (offset == JoystickOffset.X ||
                        offset == JoystickOffset.Y ||
                        offset == JoystickOffset.Z ||
                        offset == JoystickOffset.RX ||
                        offset == JoystickOffset.RY ||
                        offset == JoystickOffset.RZ ||
                        offset == JoystickOffset.Slider1 ||
                        offset == JoystickOffset.Slider2
                        )
                    {

                        if (!_jitterDetectionDictionary.ContainsKey(state.RawOffset))
                        {
                            _jitterDetectionDictionary.Add(state.RawOffset, new JitterDetection());
                        }
                        if (_jitterDetectionDictionary[state.RawOffset].IsJitter(state.Value)) continue;

                        HandleAxis(state);
                        OnAxisChanged(state);
                        continue;
                    }
                }
            }
        }

        private void DequeueLoop()
        {
            var keyUpList = new List<Tuple<int, ButtonAction>>();

            Logging.Log.Debug("Dequeue loop started");

            foreach (var job in _actionJobs.GetConsumingEnumerable())
            {
                if (keyUpList.Count > 0)
                {
                    foreach (var keyUp in keyUpList.Where(i => i.Item1 == job.Offset).ToList())
                    {
                        Keyboard.SendKeyPress(keyUp.Item2.ScanCode, keyUp.Item2.IsKeyUp, keyUp.Item2.IsExtended);
                        keyUpList.Remove(keyUp);

                        KeystrokeUpSent?.Invoke(this, new KeystrokeSentEventArgs(job.MapId, keyUp.Item1, keyUp.Item2.ScanCode, keyUp.Item2.IsKeyUp, keyUp.Item2.IsExtended));
                    }
                }

                if (job.Actions == null)
                {
                    continue;
                }

                foreach (var action in job.Actions)
                {
                    if (action.IsKeyUp)
                    {
                        keyUpList.Add(new Tuple<int, ButtonAction>(job.Offset, action));
                    }
                    else
                    {
                        Keyboard.SendKeyPress(action.ScanCode, action.IsKeyUp, action.IsExtended);
                        KeystrokeDownSent?.Invoke(this, new KeystrokeSentEventArgs(job.MapId, job.Offset, action.ScanCode, action.IsKeyUp, action.IsExtended));
                    }
                }
            }

            Logging.Log.Debug("Dequeue loop stopped");
        }

        private bool IsButtonDown(int value)
        {
            return value == (int)JoystickOffsetValues.ButtonState.ButtonPressed;
        }

        private static readonly ConcurrentDictionary<JoystickOffset, int> _lastPovButton = new ConcurrentDictionary<JoystickOffset, int>();
        private void HandlePovButton(JoystickOffset offset, int value)
        {
            if (_lastPovButton.ContainsKey(offset) || value == -1)
            {
                Logging.Log.Debug($"POV button release: {offset} - {value}");

                var success = _lastPovButton.TryRemove(offset, out var translatedOffset);

                if (!success) return;
                if (!(GetMap(translatedOffset) is HOTASButton map)) return;
                HandleButtonReleased(map, translatedOffset);
                OnButtonRelease(translatedOffset);
            }
            else
            {
                var translatedOffset = TranslatePointOfViewOffset(offset, value);

                _lastPovButton.TryAdd(offset, translatedOffset);
                Logging.Log.Debug($"Pressing POV button: {offset} - {value}");

                if (!(GetMap(translatedOffset) is HOTASButton map)) return;

                HandleButtonPressed(map, translatedOffset);
                OnButtonPress(translatedOffset);
            }
        }

        private static readonly ConcurrentDictionary<int, bool> _activeMacros = new ConcurrentDictionary<int, bool>();
        private static readonly ConcurrentDictionary<int, Task> _activeButtons = new ConcurrentDictionary<int, Task>();
        private void HandleStandardButton(int offset, int value)
        {
            var map = GetMap(offset) as HOTASButton;
            if (IsButtonDown(value))
            {
                if (map != null && (map.ActionCatalogItem.Actions.Count > 0 || map.ShiftModePage > 0))
                {
                    //if action list has a timer in it, then it is a macro and executes on another thread independently. does not interrupt other buttons
                    HandleButtonPressed(map, offset);
                }
                OnButtonPress(offset);
            }
            else
            {
                HandleButtonReleased(map, offset);
                OnButtonRelease(offset);
            }
        }

        private void HandleButtonPressed(HOTASButton button, int offset)
        {
            if (button == null) return;

            if (button.IsMacro)
            {
                HandleMacro(button, offset);
                return;
            }

            if (button.ShiftModePage > 0)
            {
                HandleShiftMode(button.ShiftModePage, button.IsShift);
                if (button.ActionCatalogItem.Actions.Count == 0) return;
            }

            _actionJobs.Add(new ActionJobItem() { Offset = offset, MapId = button.MapId, Actions = button.ActionCatalogItem.Actions });
        }

        private void HandleMacro(HOTASButton button, int offset)
        {
            if (_activeMacros.TryGetValue(offset, out _))
            {
                //cancel a macro already in progress
                _activeMacros.TryRemove(offset, out _);
                MacroCancelled?.Invoke(this, new MacroCancelledEventArgs(offset, (int)Win32Structures.ScanCodeShort.MACRO_CANCELLED));
                return;
            }

            _activeMacros.TryAdd(offset, true);
            Task.Run(() => PlayMacroOnce(offset, button.ActionCatalogItem.Actions));
        }

        private void HandleShiftMode(int mode, bool isShift)
        {
            ModeProfileSelected?.Invoke(this, new ModeProfileSelectedEventArgs() { Mode = mode, IsShift = isShift });
        }

        private async Task PlayMacroOnce(int offset, ObservableCollection<ButtonAction> actions)
        {
            MacroStarted?.Invoke(this, new MacroStartedEventArgs(offset, (int)Win32Structures.ScanCodeShort.MACRO_STARTED));

            foreach (var action in actions)
            {
                if (_activeMacros.ContainsKey(offset) == false) break;
                
                if (action.TimeInMilliseconds > 0)
                {
                    //yes this is precise only to the nearest KeyDownRepeatDelay milliseconds. repeated keys are on a 60 millisecond boundary, so the UI could be locked to 60ms increments only
                    var timeLeft = action.TimeInMilliseconds;
                    while (timeLeft > 0)
                    {
                        await Task.Delay(Keyboard.KeyDownRepeatDelay);
                        timeLeft -= Keyboard.KeyDownRepeatDelay;
                    }
                }

                Keyboard.SendKeyPress(action.ScanCode, action.IsKeyUp, action.IsExtended);

                if (action.IsKeyUp)
                {
                    KeystrokeUpSent?.Invoke(this, new KeystrokeSentEventArgs(offset, offset, action.ScanCode, action.IsKeyUp, action.IsExtended));
                }
                else
                {
                    KeystrokeDownSent?.Invoke(this, new KeystrokeSentEventArgs(offset, offset, action.ScanCode, action.IsKeyUp, action.IsExtended));
                }
            }
            _activeMacros.TryRemove(offset, out _);
        }

        private void HandleButtonReleased(HOTASButton button, int offset)
        {
            if (button == null) return;

            var mapId = button.MapId;

            if (button.IsShift)
            {
                ShiftReleased?.Invoke(this, new EventArgs());
            }

            _actionJobs.Add(new ActionJobItem() { Offset = offset, MapId = mapId, Actions = null });
        }

        private void HandleAxis(JoystickUpdate state)
        {
            var offset = (int)state.Offset;

            if (!(GetMap(offset) is HOTASAxis axis)) return;

            axis.SetAxis(state.Value);

            if (!axis.IsSegmentChanged) return;

            var map = axis.GetButtonMapFromRawValue(state.Value);
            HandleButtonPressed(map, offset);
            HandleButtonReleased(map, offset);
        }

        private void OnAxisChanged(JoystickUpdate state)
        {
            AxisChanged?.Invoke(this, new AxisChangedEventArgs() { AxisId = (int)state.Offset, Value = state.Value, Device = null });
        }

        public IHotasBaseMap GetMap(int buttonOffset)
        {
            return _buttonMap.FirstOrDefault(m => m.MapId == buttonOffset);
        }

        private void OnButtonPress(int buttonId)
        {
            ButtonPressed?.Invoke(this, new ButtonPressedEventArgs() { ButtonId = buttonId, Device = null });
        }

        private void OnButtonRelease(int buttonId)
        {
            ButtonReleased?.Invoke(this, new ButtonPressedEventArgs() { ButtonId = buttonId, Device = null });
        }

        public void SetButtonMap(ObservableCollection<IHotasBaseMap> buttonMap)
        {
            _buttonMap = buttonMap;
        }
    }

}

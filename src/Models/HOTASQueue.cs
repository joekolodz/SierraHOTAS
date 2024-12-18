﻿//todo can remove sharpdx dependency...replace JoystickUpdate with custom array. can do this in the joystickwrapper for the call to GetCurrentState
using SharpDX.DirectInput;
using SierraHOTAS.Win32;
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
        public event EventHandler<RepeatStartedEventArgs> RepeatStarted;
        public event EventHandler<RepeatCancelledEventArgs> RepeatCancelled;
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        public event EventHandler<ButtonPressedEventArgs> ButtonReleased;
        public event EventHandler<AxisChangedEventArgs> AxisChanged;
        public event EventHandler<ModeSelectedEventArgs> ModeSelected;
        public event EventHandler<EventArgs> ShiftReleased;
        public event EventHandler<EventArgs> LostConnectionToDevice;

        private bool _isStopRequested;

        private BlockingCollection<ActionJobItem> _actionJobs;

        private Dictionary<int, JitterDetection> _jitterDetectionDictionary;

        private Dictionary<int, ModeActivationItem> _modeActivationButtons;

        private int _mode;

        private IKeyboard Keyboard { get; set; }
        private IJoystick Joystick { get; set; }
        private Dictionary<int, ObservableCollection<IHotasBaseMap>> _modes;

        public HOTASQueue(IKeyboard keyboard)
        {
            Keyboard = keyboard;
        }

        public void Listen(IJoystick joystick, Dictionary<int, ObservableCollection<IHotasBaseMap>> modes, Dictionary<int, ModeActivationItem> modeActivationButtons)
        {
            Joystick = joystick;
            _modes = modes;
            _mode = (_modes != null && _modes.Any()) ? _modes.First().Key : 1;
            _modeActivationButtons = modeActivationButtons;

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

        private async Task ListenLoop()
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
                    Logging.Log.Info($"Stopping in HOTAS Queue ListenLoop: {e}");
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
                        await HandleStandardButton((int)offset, state.Value);
                        continue;
                    }

                    if (offset == JoystickOffset.POV1 ||
                        offset == JoystickOffset.POV2 ||
                        offset == JoystickOffset.POV3 ||
                        offset == JoystickOffset.POV4)
                    {
                        Logging.Log.Debug($"Offset:{offset}({state.RawOffset}), POV:{TranslatePointOfViewOffset(offset, state.Value)}, Seq:{state.Sequence}, Value:{state.Value}");
                        await HandlePovButton((JoystickOffset)state.Offset, state.Value);
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

                        await HandleAxis(state);
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
        private async Task HandlePovButton(JoystickOffset offset, int value)
        {
            if (_lastPovButton.ContainsKey(offset) || value == (int)JoystickOffsetValues.PointOfViewPositionValues.Released)
            {
                Logging.Log.Debug($"POV button release: {offset} - {value}");

                var success = _lastPovButton.TryRemove(offset, out var translatedOffset);

                if (!success) return;

                await HandleStandardButton(translatedOffset, (int)JoystickOffsetValues.ButtonState.ButtonReleased);
            }
            else
            {
                var translatedOffset = TranslatePointOfViewOffset(offset, value);

                _lastPovButton.TryAdd(offset, translatedOffset);
                Logging.Log.Debug($"Pressing POV button: {offset} - {value}");

                await HandleStandardButton(translatedOffset, (int)JoystickOffsetValues.ButtonState.ButtonPressed);
            }
        }

        private static readonly ConcurrentDictionary<int, bool> _activeRepeat = new ConcurrentDictionary<int, bool>();
        private static readonly ConcurrentDictionary<int, bool> _activeMacros = new ConcurrentDictionary<int, bool>();
        private async Task HandleStandardButton(int offset, int value)
        {
            var map = GetMap(offset) as HOTASButton;
            if (IsButtonDown(value))
            {
                if (map != null && (map.ActionCatalogItem.Actions.Count > 0 || map.ShiftModePage > 0))
                {
                    //if action list has a timer in it, then it is a macro and executes on another thread independently. does not interrupt other buttons
                    await HandleButtonPressed(map, offset);
                }
                else
                {
                    if (_modeActivationButtons.ContainsKey(_mode) &&
                        _modeActivationButtons[_mode].InheritFromMode > 0)
                    {
                        map = GetMapFromParentMode(_modeActivationButtons[_mode].InheritFromMode, offset) as HOTASButton;
                        ;if (map != null)
                        {
                            await HandleButtonPressed(map, offset);
                        }
                    }
                }

                OnButtonPress(offset);
            }
            else
            {
                HandleButtonReleased(map, offset);
                OnButtonRelease(offset);
            }
        }

        private async Task HandleButtonPressed(HOTASButton button, int offset)
        {
            if (button == null) return;

            //macros and oneshots can be repeated so check repeats first
            if (button.RepeatCount != 0)
            {
                HandleRepeat(button, offset);
                return;
            }

            if (button.IsOneShot)
            {
                HandleOneShot(button, offset);
                return;
            }

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

        private void HandleRepeat(HOTASButton button, int offset)
        {
            if (_activeRepeat.TryGetValue(offset, out _))
            {
                //cancel a repeat already in progress
                _activeRepeat.TryRemove(offset, out _);
                RepeatCancelled?.Invoke(this, new RepeatCancelledEventArgs(offset, (int)Win32Structures.ScanCodeShort.REPEAT_CANCELLED));
                return;
            }

            _activeRepeat.TryAdd(offset, true);

            Task.Run(() => PlayRepeat(button, offset));
        }

        private async Task PlayRepeat(HOTASButton button, int offset)
        {
            Logging.Log.Debug("HOTASQueue - repeat started event");
            RepeatStarted?.Invoke(this, new RepeatStartedEventArgs(offset, (int)Win32Structures.ScanCodeShort.REPEAT_STARTED));

            var repeatedButton = new HOTASButton()
            {
                RepeatCount = 0,
                ActionCatalogItem = button.ActionCatalogItem,
                ActionId = button.ActionId,
                ActionName = button.ActionName,
                IsOneShot = button.IsOneShot,
                IsShift = button.IsShift,
                MapId = button.MapId,
                MapName = button.MapName,
                ShiftModePage = button.ShiftModePage,
                Type = button.Type
            };


            var repeatsLeft = button.RepeatCount;

            if (repeatsLeft == -1)
            {
                while (true)
                {
                    if (await BaseRepeat(button, offset, repeatedButton)) break;
                }
            }
            else
            {
                while (repeatsLeft-- > 0)
                {
                    if (await BaseRepeat(button, offset, repeatedButton)) break;
                }
            }

            _activeRepeat.TryRemove(offset, out _);
        }

        private async Task<bool> BaseRepeat(HOTASButton button, int offset, HOTASButton repeatedButton)
        {
            await HandleButtonPressed(repeatedButton, offset);

            if (_activeRepeat.ContainsKey(offset) == false) return true;

            if (button.IsMacro)
            {
                while (_activeMacros.ContainsKey(offset))
                {
                    await Task.Delay(500);
                    if (_activeRepeat.ContainsKey(offset) == false) break;
                }
            }

            return false;
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
            ModeSelected?.Invoke(this, new ModeSelectedEventArgs() { Mode = mode, IsShift = isShift });
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
                        if (!_activeMacros.ContainsKey(offset)) return;
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

        private void HandleOneShot(HOTASButton button, int offset)
        {
            Task.Run(() => PlayOneShot(offset, button.ActionCatalogItem.Actions));
        }

        private async Task PlayOneShot(int offset, ObservableCollection<ButtonAction> actions)
        {
            foreach (var action in actions)
            {
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

        private async Task HandleAxis(JoystickUpdate state)
        {
            var offset = (int)state.Offset;

            if (!(GetMap(offset) is HOTASAxis axis)) return;

            axis.SetAxis(state.Value);

            if (!axis.IsSegmentChanged) return;

            var map = axis.GetButtonMapFromRawValue(state.Value);
            await HandleButtonPressed(map, offset);
            HandleButtonReleased(map, offset);
        }

        private void OnAxisChanged(JoystickUpdate state)
        {
            AxisChanged?.Invoke(this, new AxisChangedEventArgs() { AxisId = (int)state.Offset, Value = state.Value, Device = null });
        }

        public IHotasBaseMap GetMap(int buttonOffset)
        {
            return _modes[_mode].FirstOrDefault(m => m.MapId == buttonOffset);
        }

        private IHotasBaseMap GetMapFromParentMode(int parentModeId, int buttonOffset)
        {
            var parentMode = _modes[parentModeId];
            var map = parentMode.FirstOrDefault(m => m.MapId == buttonOffset) as HOTASButton;
            return map;
        }

        private void OnButtonPress(int buttonId)
        {
            ButtonPressed?.Invoke(this, new ButtonPressedEventArgs() { ButtonId = buttonId, Device = null });
        }

        private void OnButtonRelease(int buttonId)
        {
            ButtonReleased?.Invoke(this, new ButtonPressedEventArgs() { ButtonId = buttonId, Device = null });
        }

        public void ActivateMode(int mode)
        {
            _mode = mode;
        }

        public void SetModesCollection(Dictionary<int, ObservableCollection<IHotasBaseMap>> modes)
        {
            _modes = modes;
        }
    }
}

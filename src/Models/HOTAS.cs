using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharpDX.DirectInput;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Reactive.Linq;
using System.Threading;

namespace SierraHOTAS.Models
{
    public class HOTAS
    {
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;

        private Task _deviceTask;
        private CancellationTokenSource _tokenSourceListenLoop;
        private CancellationToken _tokenListenLoop;
        private IDisposable _disposableSubscription = null;

        private Joystick Joystick { get; set; }
        private ObservableCollection<HOTASButtonMap> _buttonMap;

        public void ListenAsync(Joystick joystick, ObservableCollection<HOTASButtonMap> buttonMap)
        {
            Joystick = joystick;
            _buttonMap = buttonMap;
            _tokenSourceListenLoop = new CancellationTokenSource();
            _tokenListenLoop = _tokenSourceListenLoop.Token;
            _deviceTask = Task.Run(ListenLoop, _tokenListenLoop);
        }

        public void Stop()
        {
            if (App.IsDebug) return;

            _disposableSubscription?.Dispose();
            if (_tokenSourceListenLoop == null) return;

            _tokenSourceListenLoop.Cancel();
            if (!_tokenSourceListenLoop.IsCancellationRequested) return;
            if (_deviceTask.IsCanceled || _deviceTask.IsCompleted)
            {
                _deviceTask.Dispose();
            }
        }
        public static uint TranslatePointOfViewOffset(JoystickOffset offset, uint value)
        {
            var translatedOffset = value;
            translatedOffset <<= 8;
            translatedOffset |= (uint)offset;
            return translatedOffset;
        }

        private void ListenLoop()
        {

            while (!_tokenListenLoop.IsCancellationRequested)
            {
                if (Joystick == null) return;
                Joystick.Poll();
                Thread.Sleep(1);//give CPU back
                var data = Joystick.GetBufferedData();

                foreach (var state in data)
                {
                    var offset = state.Offset;
                    if (offset >= JoystickOffset.Buttons0 && offset <= JoystickOffset.Buttons127)
                    {
                        Debug.WriteLine($"Offset:{state.Offset}, RawOdffset:{state.RawOffset}, Seq:{state.Sequence}, Val:{state.Value}");
                        HandleButton(state);
                        OnButtonPress((int)offset);
                        break;
                    }

                    if (offset == JoystickOffset.PointOfViewControllers0 ||
                        offset == JoystickOffset.PointOfViewControllers1 ||
                        offset == JoystickOffset.PointOfViewControllers2 ||
                        offset == JoystickOffset.PointOfViewControllers3)
                    {
                        var translatedOffset = TranslatePointOfViewOffset(offset, (uint)state.Value);
                        Debug.WriteLine($"Offset:{state.Offset}, translated:{translatedOffset}, RawOffset:{state.RawOffset}, Seq:{state.Sequence}, Val:{state.Value}");
                        HandleButton(state);
                        OnButtonPress((int)translatedOffset);
                        break;
                    }

                    if (offset == JoystickOffset.RotationX ||
                        offset == JoystickOffset.RotationY ||
                        offset == JoystickOffset.RotationZ)
                    {
                        HandleAxis(state);
                        break;
                    }
                }
            }
        }

        private static Dictionary<JoystickOffset, ButtonAction> _lstKeyUpBuffer = new Dictionary<JoystickOffset, ButtonAction>();
        private static ButtonAction _repeatKeyDownAction;
        private static JoystickOffset _currentRepeatingOffset;

        private bool IsButtonDown(int value)
        {
            return (value == (int)JoystickOffsetValues.ButtonState.ButtonPressed) || (value == (int)JoystickOffsetValues.PointOfViewPositionValues.Released);
        }

        private void PlayActionOnce(ObservableCollection<ButtonAction> actions, out ButtonAction lastKeyDown, out ButtonAction lastKeyUp)
        {
            //TODO: || current time > before time + action.TimeInMilliseconds
            lastKeyDown = null;
            lastKeyUp = null;

            foreach (var action in actions)
            {
                //find the last down key and buffer it so it can be repeated
                if ((action.Flags & (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP) != (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP)
                {
                    lastKeyDown = action;
                }

                //find last key up and skip it. will be handled on button up
                if (action == actions[actions.Count - 1])
                {
                    lastKeyUp = action;
                    break;
                }
                Keyboard.SendKeyPress(action.ScanCode, action.Flags);
            }
        }

        private async Task PlayMacroOnce(JoystickOffset offset, ObservableCollection<ButtonAction> actions)
        {
            Debug.WriteLine($"playing macro for:{offset}");
            foreach (var action in actions)
            {
                Debug.WriteLine($" - - sending key:{action.ScanCode}");
                Keyboard.SendKeyPress(action.ScanCode, action.Flags);
                if (action.TimeInMilliseconds > 0)
                {
                    //yes this is precise only to the nearest 60 milliseconds. repeated keys are on a 60 millisecond boundary, so the UI could be locked to 60ms increments only
                    var timeLeft = action.TimeInMilliseconds;
                    while (timeLeft > 0)
                    {
                        Debug.WriteLine($" - - repeating last key:{action.ScanCode}");
                        await Task.Delay(Keyboard.KeyDownRepeatDelay);
                        Keyboard.SendKeyPress(action.ScanCode, action.Flags);
                        timeLeft -= Keyboard.KeyDownRepeatDelay;
                    }
                }
            }
            _activeMacros.TryRemove(offset, out var ignore);
        }

        private Task _buttonCancelTask;
        private static ButtonAction _lastKeyUpAction;
        private static ConcurrentDictionary<JoystickOffset, bool> _activeMacros = new ConcurrentDictionary<JoystickOffset, bool>();
        private void HandleButton(JoystickUpdate state)
        {
            var offset = state.Offset;
            var map = GetMap(offset);

            if (map != null && map.ActionCatalogItem.Actions.Count > 0)
            {
                if (IsButtonDown(state.Value))
                {
                    //if action list has a timer in it, then it is a macro and executes on another thread independently. does not interrupt other buttons
                    if (map.IsMacro)
                    {
                        if (_activeMacros.TryGetValue(offset, out var exists))
                        {
                            //prevent a given macro from running more than once
                            Debug.WriteLine($"Preventing macro for button {offset} from running again!");
                            return;
                        }

                        _activeMacros.TryAdd(offset, true);
                        Task.Run(() => PlayMacroOnce(offset, map.ActionCatalogItem.Actions));
                        return;
                    }

                    StopRepeatingKey();
                    PlayActionOnce(map.ActionCatalogItem.Actions, out _repeatKeyDownAction, out _lastKeyUpAction);
                    if (_lstKeyUpBuffer.ContainsKey(offset))
                    {
                        Debug.WriteLine($"Button pressed before release was recognized! {offset}");
                        _lstKeyUpBuffer.Remove(offset);
                    }
                    _lstKeyUpBuffer.Add(offset, _lastKeyUpAction);
                    _currentRepeatingOffset = offset;
                    _isStopRepeatRequested = false;
                    _buttonCancelTask = Task.Run(() => HandleButtonDownAsync(map));
                }
                else
                {
                    if (offset == _currentRepeatingOffset)
                    {
                        //stop repeating last button pressed
                        _isStopRepeatRequested = true;
                        _currentRepeatingOffset = 0;
                        _buttonCancelTask.Wait(350);
                        Keyboard.SendKeyPress(_lastKeyUpAction.ScanCode, _lastKeyUpAction.Flags);
                    }
                    else
                    {
                        //stop repeating previous button presses
                        if (_lstKeyUpBuffer.ContainsKey(offset))
                        {
                            _lstKeyUpBuffer.TryGetValue(offset, out var keyUp);
                            if (keyUp == null) return;
                            Keyboard.SendKeyPress(keyUp.ScanCode, keyUp.Flags);
                        }
                    }
                    _lstKeyUpBuffer.Remove(offset);
                }
            }
        }

        private void StopRepeatingKey()
        {
            if (_buttonCancelTask == null) return;
            _isStopRepeatRequested = true;
            _buttonCancelTask.Wait(10);
        }

        private static bool _isStopRepeatRequested = false;
        private async Task HandleButtonDownAsync(HOTASButtonMap buttonMap)
        {
            Debug.WriteLine($"===> start repeating {buttonMap}");
            var delay = Keyboard.KeyDownInitialDelay;
            while (!_isStopRepeatRequested && delay > 0)
            {
                await Task.Delay(10);
                delay -= 10;
            }
            while (!_isStopRepeatRequested)
            {
                Keyboard.SendKeyPress(_repeatKeyDownAction.ScanCode, _repeatKeyDownAction.Flags);
                delay = Keyboard.KeyDownRepeatDelay;
                while (!_isStopRepeatRequested && delay > 0)
                {
                    await Task.Delay(10);
                    delay -= 10;
                }
            }
            Debug.WriteLine($"===> end repeating {buttonMap}");
        }

        private void HandleAxis(JoystickUpdate state)
        {

        }

        public HOTASButtonMap GetMap(JoystickOffset buttonOffset)
        {
            return _buttonMap.FirstOrDefault(m => m.MapId == (int)buttonOffset);
        }

        private void OnButtonPress(int buttonId)
        {
            ButtonPressed?.Invoke(this, new ButtonPressedEventArgs() { ButtonId = buttonId, Device = null });
        }
    }
}

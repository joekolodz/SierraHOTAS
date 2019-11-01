using SharpDX.DirectInput;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SierraHOTAS.Models
{
    public class HOTASAsync
    {
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        public event EventHandler<AxisChangedEventArgs> AxisChanged;

        private Task _deviceTask;
        private CancellationTokenSource _tokenSourceListenLoop;
        private CancellationToken _tokenListenLoop;

        private Joystick Joystick { get; set; }
        private ObservableCollection<IHotasBaseMap> _buttonMap;

        public void ListenAsync(Joystick joystick, ObservableCollection<IHotasBaseMap> buttonMap)
        {
            Joystick = joystick;
            _buttonMap = buttonMap;
            _dicTokenRepeatTokenSource = new ConcurrentDictionary<int, CancellationTokenSource>();
            _tokenSourceListenLoop = new CancellationTokenSource();
            _tokenListenLoop = _tokenSourceListenLoop.Token;
            _deviceTask = Task.Run(ListenLoop, _tokenListenLoop);
        }

        public void Stop()
        {
            if (MainWindow.IsDebug) return;
            if (_tokenSourceListenLoop == null) return;

            _tokenSourceListenLoop.Cancel();
            if (!_tokenSourceListenLoop.IsCancellationRequested) return;
            if (_deviceTask.IsCanceled || _deviceTask.IsCompleted)
            {
                _deviceTask.Dispose();
            }
        }

        public static uint TranslatePointOfViewOffset(JoystickOffset offset, int value)
        {
            return TranslatePointOfViewOffset(offset, (uint)value);
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
                        Debug.WriteLine($"{offset} - {state.Value}");
                        Debug.WriteLine($"Offset:{offset}, Raw Offset:{state.RawOffset}, Seq:{state.Sequence}, Val:{state.Value}");
                        HandleStandardButton((int)offset, state.Value);
                        OnButtonPress((int)offset);
                        continue;
                    }

                    if (offset == JoystickOffset.PointOfViewControllers0 ||
                        offset == JoystickOffset.PointOfViewControllers1 ||
                        offset == JoystickOffset.PointOfViewControllers2 ||
                        offset == JoystickOffset.PointOfViewControllers3)
                    {
                        Debug.WriteLine($"Button Press Count: {data.Length}");
                        Debug.WriteLine($"{state.Offset} - {state.Value}");

                        var translatedOffset = TranslatePointOfViewOffset(offset, (uint)state.Value);
                        Debug.WriteLine($"Offset:{state.Offset}, translated:{translatedOffset}, RawOffset:{state.RawOffset}, Seq:{state.Sequence}, Val:{state.Value}");

                        HandlePovButton(state.Offset, state.Value);
                        OnButtonPress((int)translatedOffset);
                        continue;
                    }

                    if (offset == JoystickOffset.X ||
                        offset == JoystickOffset.Y ||
                        offset == JoystickOffset.Z ||
                        offset == JoystickOffset.RotationX || 
                        offset == JoystickOffset.RotationY ||
                        offset == JoystickOffset.RotationZ)
                    {
                        HandleAxis(state);
                        OnAxisChanged(state);
                        continue;
                    }
                }
            }
        }

        private bool IsButtonDown(int value)
        {
            return value == (int)JoystickOffsetValues.ButtonState.ButtonPressed;
        }

        private ConcurrentDictionary<int, CancellationTokenSource> _dicTokenRepeatTokenSource;

        private async Task PlayActionWithRepeat(ObservableCollection<ButtonAction> actions, CancellationToken cancel)
        {

            var keyUpList = new ObservableCollection<ButtonAction>();

            ButtonAction lastKeyDown = null;

            foreach (var action in actions)
            {
                //buffer all of the key up actions and send them later
                if ((action.Flags & (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP) == (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP)
                {
                    keyUpList.Add(action);
                }
                else
                {
                    Debug.WriteLine($"==>Pressing[{action.ScanCode}]-[{action.Flags}]");
                    Keyboard.SendKeyPress(action.ScanCode, action.Flags);
                    lastKeyDown = action;
                }
            }

            if (keyUpList.Count > 0)
            {
                cancel.Register(() =>
                {
                    var msg = "\nKeyDown cancelled list -> ";
                    foreach (var keyUp in keyUpList)
                    {
                        msg += $"\n [{keyUp.ScanCode}]-[{keyUp.Flags}]";
                        Keyboard.SendKeyPress(keyUp.ScanCode, keyUp.Flags);
                        Thread.Sleep(5);
                    }
                    Debug.WriteLine($"{msg} <- KeyDown cancelled - done -\n\n");
                });
            }

            if (cancel.IsCancellationRequested)
            {
                Debug.WriteLine("*Cancel requested. returning*");
                return;
            }


            //Debug.WriteLine($"===> start repeating");
            var delay = Keyboard.KeyDownInitialDelay;
            while (!cancel.IsCancellationRequested && delay > 0)
            {
                await Task.Delay(5);
                delay -= 5;
            }

            while (!cancel.IsCancellationRequested)
            {
                if (lastKeyDown != null)
                {
                    Keyboard.SendKeyPress(lastKeyDown.ScanCode, lastKeyDown.Flags);
                    Debug.WriteLine($"~~~Repeating: {lastKeyDown.ScanCode} - {lastKeyDown.Flags}");
                }
                delay = Keyboard.KeyDownRepeatDelay;
                while (!cancel.IsCancellationRequested && delay > 0)
                {
                    await Task.Delay(5);
                    delay -= 5;
                }
            }

            if (cancel.IsCancellationRequested)
            {
                Debug.WriteLine($"Cancel requested. end of method. {lastKeyDown.ScanCode} - {lastKeyDown.Flags}");
            }

        }

        private static readonly ConcurrentDictionary<JoystickOffset, int> _lastPovButton = new ConcurrentDictionary<JoystickOffset, int>();
        private void HandlePovButton(JoystickOffset offset, int value)
        {
            if (_lastPovButton.ContainsKey(offset) || value == -1)
            {
                Debug.WriteLine($"POV button release: {offset} - {value}");
                var success = _lastPovButton.TryRemove(offset, out var translatedOffset);
                if (!success) return;
                HandleButtonReleased(translatedOffset);
            }
            else
            {
                var translatedOffset = (int)TranslatePointOfViewOffset(offset, value);
                _lastPovButton.TryAdd(offset, translatedOffset);
                Debug.WriteLine($"Pressing POV button: {offset} - {value}");
                if (!(GetMap(translatedOffset) is HOTASButtonMap map)) return;
                HandleButtonPressed(map, translatedOffset);
            }
        }

        private static readonly ConcurrentDictionary<int, bool> _activeMacros = new ConcurrentDictionary<int, bool>();
        private static readonly ConcurrentDictionary<int, Task> _activeButtons = new ConcurrentDictionary<int, Task>();
        private void HandleStandardButton(int offset, int value)
        {
            if (IsButtonDown(value))
            {
                if (GetMap(offset) is HOTASButtonMap map && map.Actions.Count > 0)
                {
                    //if action list has a timer in it, then it is a macro and executes on another thread independently. does not interrupt other buttons
                    HandleButtonPressed(map, offset);
                }
            }
            else
            {
                HandleButtonReleased(offset);
            }
        }

        private void HandleButtonPressed(HOTASButtonMap buttonMap, int offset)
        {
            if (buttonMap == null) return;

            if (buttonMap.IsMacro)
            {
                if (_activeMacros.TryGetValue(offset, out var exists))
                {
                    //prevent a given macro from running more than once
                    return;
                }

                _activeMacros.TryAdd(offset, true);
                Task.Run(() => PlayMacroOnce(offset, buttonMap.Actions));
                return;
            }

            var repeatTokenSource = new CancellationTokenSource();
            var repeatToken = repeatTokenSource.Token;
            var succeed = _dicTokenRepeatTokenSource.TryAdd(offset, repeatTokenSource);
            if (!succeed) return;

            var t = new Task(delegate { PlayActionWithRepeat(buttonMap.Actions, repeatToken); });
            succeed = _activeButtons.TryAdd(offset, t);
            if (!succeed) return;

            t.Start();
        }

        private async Task PlayMacroOnce(int offset, ObservableCollection<ButtonAction> actions)
        {
            //Debug.WriteLine($"playing macro for:{offset}");
            foreach (var action in actions)
            {
                //Debug.WriteLine($" - - sending key:{action.ScanCode}");
                Keyboard.SendKeyPress(action.ScanCode, action.Flags);
                if (action.TimeInMilliseconds > 0)
                {
                    //yes this is precise only to the nearest 60 milliseconds. repeated keys are on a 60 millisecond boundary, so the UI could be locked to 60ms increments only
                    var timeLeft = action.TimeInMilliseconds;
                    while (timeLeft > 0)
                    {
                        //Debug.WriteLine($" - - repeating last key:{action.ScanCode}");
                        await Task.Delay(Keyboard.KeyDownRepeatDelay);
                        Keyboard.SendKeyPress(action.ScanCode, action.Flags);
                        timeLeft -= Keyboard.KeyDownRepeatDelay;
                    }
                }
            }
            _activeMacros.TryRemove(offset, out var ignore);
        }

        private void HandleButtonReleased(int offset)
        {
            if (!_dicTokenRepeatTokenSource.ContainsKey(offset)) return;

            var success = _dicTokenRepeatTokenSource.TryRemove(offset, out var token);
            if (!success) throw new ArgumentException("crap");
            token.Cancel();

            success = _activeButtons.TryRemove(offset, out var t);
            if (!success) throw new ArgumentException("shit");

            while (!t.IsCompleted)
            {
                Thread.Sleep(1);
            }

            token.Dispose();
            t.Dispose();
        }

        private void HandleAxis(JoystickUpdate state)
        {
            var offset = (int)state.Offset;
            //lookup the segment by offset 
            //map each axis segment to a virtual Offset value (greater than 128 to avoid collisions)
            //translate axis movement to offset via segments

            if (!(GetMap(offset) is HOTASAxisMap axis)) return;

            var map = axis.GetButtonMapFromRawValue(state.Value);
            HandleButtonPressed(map, offset);
        }

        private void OnAxisChanged(JoystickUpdate state)
        {
            AxisChanged?.Invoke(this, new AxisChangedEventArgs(){AxisId = (int)state.Offset, Value = state.Value, Device = null});
        }

        public IHotasBaseMap GetMap(int buttonOffset)
        {
            return _buttonMap.FirstOrDefault(m => m.MapId == buttonOffset);
        }

        private void OnButtonPress(int buttonId)
        {
            ButtonPressed?.Invoke(this, new ButtonPressedEventArgs() { ButtonId = buttonId, Device = null });
        }
    }

}

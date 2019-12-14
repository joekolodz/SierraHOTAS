using SharpDX.DirectInput;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SierraHOTAS.Models
{
    public class HOTASQueue
    {
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        public event EventHandler<AxisChangedEventArgs> AxisChanged;

        private Task _deviceListenLoopTask;
        private CancellationTokenSource _tokenSourceListenLoop;
        private CancellationToken _tokenListenLoop;

        private Task _deviceDequeueLoopTask;
        private CancellationTokenSource _tokenSourceDequeueLoop;
        private CancellationToken _tokenDequeueLoop;
        private BlockingCollection<ActionJobItem> _actionJobs;

        private JitterDetection _jitter;


        private Joystick Joystick { get; set; }
        private ObservableCollection<IHotasBaseMap> _buttonMap;

        public void ListenAsync(Joystick joystick, ObservableCollection<IHotasBaseMap> buttonMap)
        {
            Joystick = joystick;
            _buttonMap = buttonMap;
            _jitter = new JitterDetection();

            _actionJobs = new BlockingCollection<ActionJobItem>();

            _tokenSourceListenLoop = new CancellationTokenSource();
            _tokenListenLoop = _tokenSourceListenLoop.Token;
            _deviceListenLoopTask = Task.Factory.StartNew(ListenLoop, _tokenListenLoop, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            _tokenSourceDequeueLoop = new CancellationTokenSource();
            _tokenDequeueLoop = _tokenSourceDequeueLoop.Token;
            _deviceDequeueLoopTask = Task.Factory.StartNew(DequeueLoop, _tokenDequeueLoop, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Stop()
        {
            if (MainWindow.IsDebug) return;

            WaitForLoopToStop(_tokenSourceListenLoop, _deviceListenLoopTask);
            WaitForLoopToStop(_tokenSourceDequeueLoop, _deviceDequeueLoopTask);
        }

        private void WaitForLoopToStop(CancellationTokenSource cancelSource, Task taskToStop)
        {
            if (cancelSource == null) return;
            cancelSource.Cancel();
            while (!taskToStop.IsCanceled && !taskToStop.IsCompleted)
            {
                Thread.Sleep(1);
            }
            taskToStop.Dispose();
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
                Thread.Sleep(1);//give CPU back
                Joystick.Poll();
                var data = Joystick.GetBufferedData();

                foreach (var state in data)
                {
                    var offset = state.Offset;
                    if (offset >= JoystickOffset.Buttons0 && offset <= JoystickOffset.Buttons127)
                    {
                        Logging.Log.Debug($"{offset} - {state.Value}");
                        Logging.Log.Debug($"Offset:{offset}, Raw Offset:{state.RawOffset}, Seq:{state.Sequence}, Val:{state.Value}");
                        HandleStandardButton((int)offset, state.Value);
                        OnButtonPress((int)offset);
                        continue;
                    }

                    if (offset == JoystickOffset.PointOfViewControllers0 ||
                        offset == JoystickOffset.PointOfViewControllers1 ||
                        offset == JoystickOffset.PointOfViewControllers2 ||
                        offset == JoystickOffset.PointOfViewControllers3)
                    {
                        Logging.Log.Debug($"Button Press Count: {data.Length}");
                        Logging.Log.Debug($"{state.Offset} - {state.Value}");

                        var translatedOffset = TranslatePointOfViewOffset(offset, (uint)state.Value);
                        Logging.Log.Debug($"Offset:{state.Offset}, translated:{translatedOffset}, RawOffset:{state.RawOffset}, Seq:{state.Sequence}, Val:{state.Value}");

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
                        if (_jitter.IsJitter(state.Value)) continue;

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

            Logging.Log.Info("Dequeue loop started");

            foreach (var job in _actionJobs.GetConsumingEnumerable(_tokenDequeueLoop))
            {
                Logging.Log.Info($"Job Started ({_actionJobs.Count} left) ==>");

                if (keyUpList.Count > 0)
                {
                    foreach (var keyUp in keyUpList.Where(i=>i.Item1 == job.Offset).ToList())
                    {
                        Logging.Log.Info($"Key Up [scan:{keyUp.Item2.ScanCode}]-[flag:{keyUp.Item2.Flags}]");
                        Keyboard.SendKeyPress(keyUp.Item2.ScanCode, keyUp.Item2.Flags);
                        keyUpList.Remove(keyUp);
                    }
                }

                if (job.Actions == null)
                {
                    Logging.Log.Info("just a key up nothing left to do");
                    continue;
                }

                foreach (var action in job.Actions)
                {
                    if ((action.Flags & (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP) == (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP)
                    {
                        Logging.Log.Info($"KeyUp (storing) [offset:{job.Offset}]-[scan:{action.ScanCode}]-[flag:{action.Flags}]");
                        keyUpList.Add(new Tuple<int, ButtonAction>(job.Offset, action));
                    }
                    else
                    {
                        Logging.Log.Info($"Pressing[offset:{job.Offset}]-[scan:{action.ScanCode}]-[flag:{action.Flags}]");
                        Keyboard.SendKeyPress(action.ScanCode, action.Flags);
                    }
                }
            }

            Logging.Log.Info("Dequeue loop stopped");
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
                HandleButtonReleased(translatedOffset);
            }
            else
            {
                var translatedOffset = (int)TranslatePointOfViewOffset(offset, value);
                _lastPovButton.TryAdd(offset, translatedOffset);
                Logging.Log.Debug($"Pressing POV button: {offset} - {value}");
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
                if (GetMap(offset) is HOTASButtonMap map && map.ActionCatalogItem.Actions.Count > 0)
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
                Task.Run(() => PlayMacroOnce(offset, buttonMap.ActionCatalogItem.Actions));
                return;
            }

            _actionJobs.Add(new ActionJobItem() { Offset = offset, Actions = buttonMap.ActionCatalogItem.Actions }, _tokenDequeueLoop);
        }

        private async Task PlayMacroOnce(int offset, ObservableCollection<ButtonAction> actions)
        {
            foreach (var action in actions)
            {
                Keyboard.SendKeyPress(action.ScanCode, action.Flags);
                if (action.TimeInMilliseconds > 0)
                {
                    //yes this is precise only to the nearest KeyDownRepeatDelay milliseconds. repeated keys are on a 60 millisecond boundary, so the UI could be locked to 60ms increments only
                    var timeLeft = action.TimeInMilliseconds;
                    while (timeLeft > 0)
                    {
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
            _actionJobs.Add(new ActionJobItem(){Offset = offset, Actions = null}, _tokenDequeueLoop);
        }

        private void HandleAxis(JoystickUpdate state)
        {
            var offset = (int)state.Offset;

            if (!(GetMap(offset) is HOTASAxisMap axis)) return;

            axis.SetAxis(state.Value);

            if (!axis.IsSegmentChanged) return;

            var map = axis.GetButtonMapFromRawValue(state.Value);
            HandleButtonPressed(map, offset);
            HandleButtonReleased(offset);
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
    }

}

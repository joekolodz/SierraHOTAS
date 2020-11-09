using SharpDX.DirectInput;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace SierraHOTAS.Models
{
    public class HOTASQueue : IHOTASQueue
    {
        public event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        public event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        public event EventHandler<ButtonPressedEventArgs> ButtonReleased;
        public event EventHandler<AxisChangedEventArgs> AxisChanged;
        public event EventHandler<ModeProfileSelectedEventArgs> ModeProfileSelected;
        public event EventHandler ShiftReleased;
        public event EventHandler LostConnectionToDevice;

        private Task _deviceListenLoopTask;
        private CancellationTokenSource _tokenSourceListenLoop;
        private CancellationToken _tokenListenLoop;

        private Task _deviceDequeueLoopTask;
        private CancellationTokenSource _tokenSourceDequeueLoop;
        private CancellationToken _tokenDequeueLoop;
        private BlockingCollection<ActionJobItem> _actionJobs;

        private Dictionary<int, JitterDetection> _jitterDetectionDictionary;


        private IJoystick Joystick { get; set; }
        private ObservableCollection<IHotasBaseMap> _buttonMap;

        public void ListenAsync(IJoystick joystick, ObservableCollection<IHotasBaseMap> buttonMap)
        {
            Joystick = joystick;
            _buttonMap = buttonMap;
            _jitterDetectionDictionary = new Dictionary<int, JitterDetection>();

            _actionJobs = new BlockingCollection<ActionJobItem>();

            _tokenSourceListenLoop = new CancellationTokenSource();
            _tokenListenLoop = _tokenSourceListenLoop.Token;
            _deviceListenLoopTask = Task.Factory.StartNew(ListenLoop, _tokenListenLoop, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            _tokenSourceDequeueLoop = new CancellationTokenSource();
            _tokenDequeueLoop = _tokenSourceDequeueLoop.Token;
            _deviceDequeueLoopTask = Task.Factory.StartNew(DequeueLoop, _tokenDequeueLoop, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void ForceButtonPress(JoystickOffset offset, bool isDown)
        {
            HandleStandardButton((int)offset, isDown ? 128 : 0);
        }

        public void Stop()
        {
            if (App.IsDebug) return;
            _tokenSourceListenLoop?.Cancel();
            _tokenSourceDequeueLoop?.Cancel();
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
            while (true)
            {
                try
                {
                    _tokenListenLoop.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (Joystick == null) return;
                Thread.Sleep(1);//give CPU back

                //if exception due to lost device, then break out of loop so the user can refresh the device list and start over
                Joystick.Poll();
                JoystickUpdate[] data = { };
                try
                {
                    data = Joystick.GetBufferedData();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    LostConnectionToDevice?.Invoke(this, EventArgs.Empty);
                    _tokenSourceListenLoop.Cancel();
                }

                foreach (var state in data)
                {
                    var offset = state.Offset;
                    if (offset >= JoystickOffset.Buttons0 && offset <= JoystickOffset.Buttons127)
                    {
                        Logging.Log.Debug($"Offset:{offset} - Value:{state.Value}");
                        Logging.Log.Debug($"Offset:{offset}, Raw Offset:{state.RawOffset}, Seq:{state.Sequence}, Val:{state.Value}");
                        HandleStandardButton((int)offset, state.Value);
                        Logging.Log.Debug($"button:{(int)offset} - {state.Value}");
                        continue;
                    }

                    if (offset == JoystickOffset.PointOfViewControllers0 ||
                        offset == JoystickOffset.PointOfViewControllers1 ||
                        offset == JoystickOffset.PointOfViewControllers2 ||
                        offset == JoystickOffset.PointOfViewControllers3)
                    {
                        Logging.Log.Debug($"Button Press Count: {data.Length}");
                        Logging.Log.Debug($"{state.Offset} - {state.Value}");
                        Logging.Log.Debug($"Offset:{state.Offset}, translated:{TranslatePointOfViewOffset(offset, state.Value)}, RawOffset:{state.RawOffset}, Seq:{state.Sequence}, Val:{state.Value}");

                        HandlePovButton(state.Offset, state.Value);
                        continue;
                    }

                    if (offset == JoystickOffset.X ||
                        offset == JoystickOffset.Y ||
                        offset == JoystickOffset.Z ||
                        offset == JoystickOffset.RotationX ||
                        offset == JoystickOffset.RotationY ||
                        offset == JoystickOffset.RotationZ)
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

            Logging.Log.Info("Dequeue loop started");

            foreach (var job in _actionJobs.GetConsumingEnumerable(_tokenDequeueLoop))
            {
                if (keyUpList.Count > 0)
                {
                    foreach (var keyUp in keyUpList.Where(i => i.Item1 == job.Offset).ToList())
                    {
                        Keyboard.SendKeyPress(keyUp.Item2.ScanCode, keyUp.Item2.Flags);
                        keyUpList.Remove(keyUp);

                        KeystrokeUpSent?.Invoke(this, new KeystrokeSentEventArgs(job.MapId, keyUp.Item1, keyUp.Item2.ScanCode, keyUp.Item2.Flags));
                    }
                }

                if (job.Actions == null)
                {
                    continue;
                }

                foreach (var action in job.Actions)
                {
                    if ((action.Flags & (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP) == (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP)
                    {
                        keyUpList.Add(new Tuple<int, ButtonAction>(job.Offset, action));
                    }
                    else
                    {
                        Keyboard.SendKeyPress(action.ScanCode, action.Flags);
                        KeystrokeDownSent?.Invoke(this, new KeystrokeSentEventArgs(job.MapId, job.Offset, action.ScanCode, action.Flags));
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
                if (!(GetMap(translatedOffset) is HOTASButtonMap map)) return;
                HandleButtonReleased(map, translatedOffset);
                OnButtonRelease(translatedOffset);
            }
            else
            {
                var translatedOffset = TranslatePointOfViewOffset(offset, value);

                _lastPovButton.TryAdd(offset, translatedOffset);
                Logging.Log.Debug($"Pressing POV button: {offset} - {value}");
                
                if (!(GetMap(translatedOffset) is HOTASButtonMap map)) return;

                HandleButtonPressed(map, translatedOffset);
                OnButtonPress(translatedOffset);
            }
        }

        private static readonly ConcurrentDictionary<int, bool> _activeMacros = new ConcurrentDictionary<int, bool>();
        private static readonly ConcurrentDictionary<int, Task> _activeButtons = new ConcurrentDictionary<int, Task>();
        private void HandleStandardButton(int offset, int value)
        {
            var map = GetMap(offset) as HOTASButtonMap;
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

        private void HandleButtonPressed(HOTASButtonMap buttonMap, int offset)
        {
            if (buttonMap == null) return;

            if (buttonMap.IsMacro)
            {
                HandleMacro(buttonMap, offset);
                return;
            }

            if (buttonMap.ShiftModePage > 0)
            {
                HandleShiftMode(buttonMap.ShiftModePage, buttonMap.IsShift);
                if (buttonMap.ActionCatalogItem.Actions.Count == 0) return;
            }

            _actionJobs.Add(new ActionJobItem() { Offset = offset, MapId = buttonMap.MapId, Actions = buttonMap.ActionCatalogItem.Actions }, _tokenDequeueLoop);
        }

        private void HandleMacro(HOTASButtonMap buttonMap, int offset)
        {
            if (_activeMacros.TryGetValue(offset, out _))
            {
                //prevent a given macro from running more than once
                return;
            }

            _activeMacros.TryAdd(offset, true);
            Task.Run(() => PlayMacroOnce(offset, buttonMap.ActionCatalogItem.Actions));
        }

        private void HandleShiftMode(int mode, bool isShift)
        {
            ModeProfileSelected?.Invoke(this, new ModeProfileSelectedEventArgs() { Mode = mode, IsShift = isShift });
        }

        private async Task PlayMacroOnce(int offset, ObservableCollection<ButtonAction> actions)
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

                Keyboard.SendKeyPress(action.ScanCode, action.Flags);

                if ((action.Flags & (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP) == (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP)
                {
                    KeystrokeUpSent?.Invoke(this, new KeystrokeSentEventArgs(offset, offset, action.ScanCode, action.Flags));
                }
                else
                {
                    KeystrokeDownSent?.Invoke(this, new KeystrokeSentEventArgs(offset, offset, action.ScanCode, action.Flags));
                }
            }
            _activeMacros.TryRemove(offset, out _);
        }

        private void HandleButtonReleased(HOTASButtonMap buttonMap, int offset)
        {
            if (buttonMap == null) return;
            
            var mapId = buttonMap.MapId;

            if (buttonMap.IsShift)
            {
                //todo
                //raise new shift event
                Logging.Log.Info($"Raise Shift Event - {buttonMap.MapName} - {buttonMap.IsShift}");
                ShiftReleased?.Invoke(this, new EventArgs());
            }

            _actionJobs.Add(new ActionJobItem() { Offset = offset, MapId = mapId, Actions = null }, _tokenDequeueLoop);
        }

        private void HandleAxis(JoystickUpdate state)
        {
            var offset = (int)state.Offset;

            if (!(GetMap(offset) is HOTASAxisMap axis)) return;

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

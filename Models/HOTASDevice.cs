using Newtonsoft.Json;
using SharpDX.DirectInput;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SierraHOTAS.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HOTASDevice
    {
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;

        [JsonProperty]
        public Guid InstanceId { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        public Capabilities Capabilities { get; set; }

        [JsonProperty]
        public ObservableCollection<HOTASMap> ButtonMap { get; set; }

        private Joystick Joystick { get; set; }
        private IDisposable _disposableSubscription = null;

        public HOTASDevice()
        {
            ButtonMap = new ObservableCollection<HOTASMap>();
        }

        public HOTASDevice(Guid instanceId, string name)
        {
            if (instanceId == Guid.Empty || instanceId == null || string.IsNullOrWhiteSpace(name))
            {
                throw new NullReferenceException("Information about Joystick is unavailable (this should only disable the Save and Load options");
            }

            InstanceId = instanceId;
            Name = name;
            ButtonMap = new ObservableCollection<HOTASMap>();

            Initialize();
        }

        private void Initialize()
        {
            if (MainWindow.IsDebug) return;

            var i = new DirectInput();
            Joystick = new Joystick(i, InstanceId);
            Joystick.Properties.BufferSize = 128;
            Joystick.Acquire();

            LoadCapabilitiesMapping();
        }

        private Task _deviceTask;
        private CancellationTokenSource _tokenSourceListenLoop;
        private CancellationToken _tokenListenLoop;

        private void LoadCapabilitiesMapping()
        {
            Debug.WriteLine($"\nLoading device capabilities for ...{Name}");

            Capabilities = Joystick.Capabilities;

            Debug.WriteLine("AxeCount {0}", Capabilities.AxeCount);
            Debug.WriteLine("ButtonCount {0}", Capabilities.ButtonCount);
            Debug.WriteLine("PovCount {0}", Capabilities.PovCount);
            Debug.WriteLine("Flags {0}", Capabilities.Flags);

            Debug.WriteLine("\nBuilding button maps...");
            //SeedButtonMap(JoystickOffset.X, Capabilities.AxeCount, HOTASMap.ButtonType.Axis);
            if (Capabilities.ButtonCount > 0) SeedButtonMap(JoystickOffset.Buttons0, Capabilities.ButtonCount, HOTASMap.ButtonType.Button);
            if (Capabilities.PovCount > 0) SeedPointOfViewMap(JoystickOffset.PointOfViewControllers0, Capabilities.PovCount, HOTASMap.ButtonType.POV);

        }

        private void SeedPointOfViewMap(JoystickOffset startFrom, int length, HOTASMap.ButtonType type)
        {
            var indexStart = JoystickOffsetValues.GetIndex(startFrom.ToString());
            for (var count = indexStart; count < indexStart + length; count++)
            {
                var offset = JoystickOffsetValues.GetOffset(count);

                //each of the eight POV positions needs a unique offset number so that we don't have to have a compound index to do lookups with later.
                //so POV1 Offset is 0x00000020 and EAST = 0x2328, then assign translated offset of 0x23280020
                //so POV2 Offset is 0x00000024 and SOUTH EAST = 0x34BC then assign translated offset of 0x34BC0024
                uint translatedOffset;
                for (uint position = 0; position < 8; position++)
                {
                    translatedOffset = TranslatePointOfViewOffset(offset, 4500 * position);

                    ButtonMap.Add(new HOTASMap()
                    {
                        Offset = translatedOffset,
                        ButtonId = (int)translatedOffset,
                        Type = type,
                        ButtonName = Enum.GetName(typeof(JoystickOffsetValues.PointOfViewPositionValues), 4500 * position)
                    });
                }
            }
        }

        private uint TranslatePointOfViewOffset(JoystickOffset offset, uint value)
        {
            var translatedOffset = value;
            translatedOffset <<= 8;
            translatedOffset |= (uint)offset;
            return translatedOffset;
        }

        private void SeedButtonMap(JoystickOffset startFrom, int length, HOTASMap.ButtonType type)
        {
            var indexStart = JoystickOffsetValues.GetIndex(startFrom.ToString());
            for (var count = indexStart; count < indexStart + length; count++)
            {
                var offset = JoystickOffsetValues.GetOffset(count);
                ButtonMap.Add(new HOTASMap()
                {
                    Offset = (uint)offset,
                    ButtonId = (int)offset,
                    Type = type,
                    ButtonName = $"{JoystickOffsetValues.GetName(offset)}"
                });
            }
        }

        public HOTASMap GetMap(JoystickOffset buttonOffset)
        {
            
            
            //need to consider POV!!!



            var offset = (int)buttonOffset;
            return ButtonMap.FirstOrDefault(m => m.ButtonId == offset);
        }

        private void OnButtonPress(int buttonId)
        {
            ButtonPressed?.Invoke(this, new ButtonPressedEventArgs() { ButtonId = buttonId, Device = this });
        }

        public void Stop()
        {
            if (MainWindow.IsDebug) return;
            
            _disposableSubscription?.Dispose();
            if (_tokenSourceListenLoop == null) return;

            _tokenSourceListenLoop.Cancel();
            if (!_tokenSourceListenLoop.IsCancellationRequested) return;
            if (_deviceTask.IsCanceled || _deviceTask.IsCompleted)
            {
                _deviceTask.Dispose();
            }

            //Joystick.Unacquire();
            //Joystick.Dispose();
            //Joystick = null;
        }

        public void Listen()
        {
            _tokenSourceListenLoop = new CancellationTokenSource();
            _tokenListenLoop = _tokenSourceListenLoop.Token;
            _deviceTask = Task.Run(ListenLoop, _tokenListenLoop);
        }

        private void ListenLoop()
        {
            Debug.WriteLine("\n\nReading Joystick...!");

            while (!_tokenListenLoop.IsCancellationRequested)
            {
                if (Joystick == null) return;
                Joystick.Poll();
                System.Threading.Thread.Sleep(1);
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
                        var translatedOffset = TranslatePointOfViewOffset(offset, (uint) state.Value);
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

        private async Task HandleMacroAsync(JoystickUpdate state)
        {
            var offset = state.Offset;
            var map = GetMap(offset);
            if (map != null && map.Actions.Count > 0)
            {
                if (IsButtonDown(state.Value))
                {
                    PlayActionOnce(map.Actions, out _repeatKeyDownAction, out _lastKeyUpAction);
                    _lstKeyUpBuffer.Add(offset, _lastKeyUpAction);
                    //_currentRepeatingOffset = offset;
                    //_isStopRepeatRequested = false;
                    //_buttonCancelTask = null;
                    await HandleButtonDownAsync(map);

                }
                else
                {
                    if (offset == _currentRepeatingOffset)
                    {
                        //stop repeating last button pressed
                        _isStopRepeatRequested = true;
                        _currentRepeatingOffset = 0;
                        //_buttonCancelTask.Wait(350);
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


        private Task _buttonCancelTask;
        private static ButtonAction _lastKeyUpAction;
        private static ConcurrentDictionary<JoystickOffset, bool> _activeMacros = new ConcurrentDictionary<JoystickOffset, bool>();
        private void HandleButton(JoystickUpdate state)
        {
            var offset = state.Offset;
            var map = GetMap(offset);

            Debug.WriteLine(offset);
            if (map != null && map.Actions.Count > 0)
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
                        Task.Run(() => PlayMacroOnce(offset, map.Actions));
                        return;
                    }

                    StopRepeatingKey();
                    PlayActionOnce(map.Actions, out _repeatKeyDownAction, out _lastKeyUpAction);
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
        private async Task HandleButtonDownAsync(HOTASMap map)
        {
            Debug.WriteLine($"===> start repeating {map}");
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
            Debug.WriteLine($"===> end repeating {map}");
        }

        private void HandleAxis(JoystickUpdate state)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Newtonsoft.Json;
using SharpDX.DirectInput;

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
        public List<HOTASMap> ButtonMap { get; set; }

        private Joystick Joystick { get; set; }
        private IDisposable _disposableSubscription = null;


        public HOTASDevice(Guid instanceId, string name)
        {
            if (instanceId == Guid.Empty || instanceId == null || string.IsNullOrWhiteSpace(name))
            {
                throw new NullReferenceException("Information about Joystick is unavailable");
            }

            InstanceId = instanceId;
            Name = name;


            if (HOTASCollection.IsDebug)
            {
                TEST_LoadCapabilitiesMapping();
                return;
            }



            var i = new DirectInput();
            Joystick = new Joystick(i, InstanceId);
            Joystick.Properties.BufferSize = 128;
            Joystick.Acquire();

            Capabilities = Joystick.Capabilities;

            LoadCapabilitiesMapping();

            Console.WriteLine("AxeCount {0}", Capabilities.AxeCount);
            Console.WriteLine("ButtonCount {0}", Capabilities.ButtonCount);
            Console.WriteLine("PovCount {0}", Capabilities.PovCount);
            Console.WriteLine("Flags {0}", Capabilities.Flags);
        }


        private void TEST_LoadCapabilitiesMapping()
        {
            ButtonMap = new List<HOTASMap>();

            for (var i = 1; i <= 37; i++)
            {
                ButtonMap.Add(new HOTASMap() { Offset = JoystickOffsetValues.Offsets[i], ButtonId = i, Action = "<EMPTY>", ButtonName = $"Button{i}" });
            }
        }


        public void Listen()
        {
            //TODO: add events so that ProcessButton can work somewhere else
            //TODO: move this to HOTASDevice
            Debug.WriteLine("\n\nReading Joystick...!");
            //
            //
            //track the button that was pressed, and lookup the map based on button id and button pressed(128) or button released(0) value
            //
            //
            _disposableSubscription = Observable.Interval(TimeSpan.FromMilliseconds(1))
                .SelectMany(x =>
                {
                    Joystick.Poll();
                    return Joystick.GetBufferedData();
                })
                .Where(state =>
                {
                    Debug.WriteLine($"Offset:{state.Offset}, RawOffset:{state.RawOffset}");
                    if (state.Value == 128) return false;
                    if (state.Offset < JoystickOffset.Buttons0 || state.Offset > JoystickOffset.Buttons127) return false;
                    Debug.WriteLine($"State:{state.ToString()}");
                    return true;
                })
                .Subscribe(async state =>
                    {
                        OnButtonPress((int)state.Offset);
                        var map = GetMap(state.Offset);
                        if (map != null)
                        {
                            await ProcessButton(map); //event for action trigger
                        }
                    }
                );
        }

        public void Stop()
        {
            if (HOTASCollection.IsDebug) return;
            _disposableSubscription.Dispose();
        }

        private void LoadCapabilitiesMapping()
        {
            ButtonMap = new List<HOTASMap>();

            SeedButtonMap(JoystickOffset.X, Capabilities.AxeCount, HOTASMap.ButtonType.Axis);
            SeedButtonMap(JoystickOffset.PointOfViewControllers0, Capabilities.PovCount, HOTASMap.ButtonType.POV);
            SeedButtonMap(JoystickOffset.Buttons0, Capabilities.ButtonCount, HOTASMap.ButtonType.Button);
        }

        private void SeedButtonMap(JoystickOffset startFrom, int length, HOTASMap.ButtonType type)
        {
            var indexStart = JoystickOffsetValues.GetIndex(startFrom.ToString());
            for (var i = indexStart; i < indexStart + length; i++)
            {
                var x = JoystickOffsetValues.GetOffset(i);
                ButtonMap.Add(new HOTASMap()
                {
                    Offset = x, ButtonId = (int) x, Type = type, Action = $"<{type.ToString().ToUpper()}>", ButtonName = $"{JoystickOffsetValues.GetCleanName(x)}"
                });
            }
        }

        public HOTASMap GetMap(JoystickOffset buttonId)
        {
            var offset = (int)buttonId;
            return ButtonMap.FirstOrDefault(m => m.ButtonId == offset);
        }

        private void OnButtonPress(int buttonId)
        {
            ButtonPressed?.Invoke(this, new ButtonPressedEventArgs() { ButtonId = buttonId, Device = this });
        }

        private async Task ProcessButton(HOTASMap map)
        {
            //foreach (var action in buttonMap.Actions)
            //{
            //    Keyboard.SendKeyPress((Win32Structures.ScanCodeShort)action.ScanCode, action.Flags);
            //    await Task.Delay(action.TimeInMilliseconds + 1);
            //}

            //TODO: temp
            Debug.WriteLine($"Processing map: {map.ButtonName} for {Name}. Sending Keypresses: {map.Action}");

            await Task.CompletedTask;
        }
    }
}

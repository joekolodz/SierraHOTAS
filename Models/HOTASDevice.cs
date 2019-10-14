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
        private HOTAS _hotas;


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

        public void ListenAsync()
        {
            _hotas = new HOTAS();
            _hotas.ButtonPressed += OnButtonPress;
            _hotas.ListenAsync(Joystick, ButtonMap);
            Debug.WriteLine($"\n\nListening for joystick events ({Name})...!");
        }

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
                    translatedOffset = HOTAS.TranslatePointOfViewOffset(offset, 4500 * position);

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

        private void OnButtonPress(object sender, ButtonPressedEventArgs e)
        {
            ButtonPressed?.Invoke(this, new ButtonPressedEventArgs() { ButtonId = e.ButtonId, Device = this });
        }

        public void Stop()
        {
            _hotas.Stop();
        }

 
    }
}

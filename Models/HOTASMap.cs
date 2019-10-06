using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using SharpDX.DirectInput;

namespace SierraHOTAS.Models
{
    public class HOTASMap
    {
        public enum ButtonType
        {
            Axis,
            POV,
            Button
        }

        public JoystickOffset Offset { get; set; }
        public int ButtonId { get; set; }
        public string ButtonName { get; set; }
        public string Action { get; set; }
        public List<ButtonAction> Actions { get; set; }
        public ButtonType Type { get; set; }

        public void Record()
        {
            Actions = new List<ButtonAction>();
            AddKeyboardHandlers();
            Keyboard.Start();
        }

        public void Stop()
        {
            Keyboard.Stop();
            RemoveKeyboardHandlers();
        }

        public void Cancel()
        {
            Keyboard.Stop();
            RemoveKeyboardHandlers();
        }

        private void AddKeyboardHandlers()
        {
            Keyboard.KeyDownEvent += Keyboard_KeyDownEvent;
            Keyboard.KeyUpEvent += Keyboard_KeyUpEvent;
        }

        private void RemoveKeyboardHandlers()
        {
            Keyboard.KeyDownEvent -= Keyboard_KeyDownEvent;
            Keyboard.KeyUpEvent -= Keyboard_KeyUpEvent;
        }

        private void Keyboard_KeyDownEvent(object sender, Keyboard.KeystrokeEventArgs e)
        {
            var name = RecordKeypress(e);
            Action += "[" + name + " DOWN]";

            Debug.WriteLine($"v KeyDown Event - Name:{name}, Code:{e.Code}, Flags:{e.Flags}");
            Debug.WriteLine("");
        }

        private void Keyboard_KeyUpEvent(object sender, Keyboard.KeystrokeEventArgs e)
        {
            var name = RecordKeypress(e);
            Action += "[" + name + " UP]";

            Debug.WriteLine($"^ KeyUp Event - Name:{name}, Code:{e.Code}, Flags:{e.Flags}");
            Debug.WriteLine("");
        }

        private string RecordKeypress(Keyboard.KeystrokeEventArgs e)
        {
            Actions.Add(new ButtonAction() {Flags = e.Flags, ScanCode = e.Code, TimeInMilliseconds = 0});
            var name = Enum.GetName(typeof(Win32Structures.ScanCodeShort), e.Code);
            if ((e.Flags & (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED) ==
                (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED)
            {
                if ((e.Code & (int) Win32Structures.ScanCodeShort.LMENU) == (int) Win32Structures.ScanCodeShort.LMENU)
                {
                    name = "RALT";
                }

                if ((e.Code & (int) Win32Structures.ScanCodeShort.LCONTROL) == (int) Win32Structures.ScanCodeShort.LCONTROL)
                {
                    name = "RCONTROL";
                }
            }

            return name;
        }
    }
}

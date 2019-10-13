using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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

        public uint Offset { get; set; }
        public int ButtonId { get; set; }
        public string ButtonName { get; set; }
        public ObservableCollection<ButtonAction> Actions { get; set; }
        public ButtonType Type { get; set; }

        public bool IsMacro => Actions.Any(a => a.TimeInMilliseconds > 0);

        private bool _isRecording;
        private ObservableCollection<ButtonAction> _actionsHistory;

        public HOTASMap()
        {
            Actions = new ObservableCollection<ButtonAction>();
        }

        private void SetRecordState(bool isRecording)
        {
            _isRecording = isRecording;
            Keyboard.IsKeySuppressionActive = isRecording;
            if (isRecording)
            {
                AddKeyboardHandlers();
            }
            else
            {
                RemoveKeyboardHandlers();
            }
        }

        public void Record()
        {
            _actionsHistory = Actions.ToList().ToObservableCollection();//make an actual copy
            Actions.Clear();
            SetRecordState(true);
        }

        public void Stop()
        {
            SetRecordState(false);
            _actionsHistory = null;
        }

        public void Cancel()
        {
            SetRecordState(false);
            Actions.Clear();
            foreach (var a in _actionsHistory)
            {
                Actions.Add(a);
            }
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

            Debug.WriteLine($"v KeyDown Event - Name:{name}, Code:{e.Code}, Flags:{e.Flags}");
            Debug.WriteLine("");
        }

        private void Keyboard_KeyUpEvent(object sender, Keyboard.KeystrokeEventArgs e)
        {
            if (!_isRecording) return;

            var name = RecordKeypress(e);

            Debug.WriteLine($"^ KeyUp Event - Name:{name}, Code:{e.Code}, Flags:{e.Flags}");
            Debug.WriteLine("");
        }

        private string RecordKeypress(Keyboard.KeystrokeEventArgs e)
        {
            if (!_isRecording) return string.Empty;

            Debug.WriteLine("[Recording]");

            Actions.Add(new ButtonAction() { Flags = e.Flags, ScanCode = e.Code, TimeInMilliseconds = 0 });
            
            //TODO: raise property changed event? or record event?

            //TODO:all this belongs in the view model
            var name = Enum.GetName(typeof(Win32Structures.ScanCodeShort), e.Code);

            //replace Enum.GetName(typeof(Win32Structures.ScanCodeShort), e.Code) with a dictionary for custom names
            if ((e.Flags & (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED) == (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED)
            {
                if ((e.Code & (int)Win32Structures.ScanCodeShort.LMENU) == (int)Win32Structures.ScanCodeShort.LMENU)
                {
                    name = "RALT";
                }

                if ((e.Code & (int)Win32Structures.ScanCodeShort.LCONTROL) == (int)Win32Structures.ScanCodeShort.LCONTROL)
                {
                    name = "RCONTROL";
                }
            }
            return name;
        }

        public override string ToString()
        {
            var o = "";
            foreach (var a in Actions)
            {
                var upDown = "v";
                if ((a.Flags & (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP) ==
                    (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP)
                {
                    upDown = "^";
                }
                o += $"[{Enum.GetName(typeof(Win32Structures.ScanCodeShort), a.ScanCode)}{upDown}]";
            }
            return o;
        }
    }
}

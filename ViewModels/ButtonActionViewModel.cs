using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SierraHOTAS.Annotations;
using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModel
{
    public class ButtonActionViewModel : INotifyPropertyChanged
    {
        public string ScanCode => GetScanCodeDisplay();

        public int TimeInMilliseconds
        {
            get => _action.TimeInMilliseconds;
            set => _action.TimeInMilliseconds = value;
        }

        public bool IsKeyUp => (_action.Flags & (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP) == (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP;

        private ButtonAction _action { get; set; }

        public ButtonActionViewModel()
        {
            _action = new ButtonAction();
        }

        public ButtonActionViewModel(ButtonAction action)
        {
            _action = action;
            //Debug.WriteLine($"BAVM Flags:{_action.Flags}, Up:{IsKeyUp}");
        }

        private string GetScanCodeDisplay()
        {
            return Keyboard.GetKeyDisplayName((Win32Structures.ScanCodeShort)_action.ScanCode, _action.Flags);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

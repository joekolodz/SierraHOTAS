﻿using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModels
{
    public class ButtonActionViewModel
    {
        public string ScanCode => GetScanCodeDisplay();

        public int TimeInMilliseconds
        {
            get => _action.TimeInMilliseconds;
            set => _action.TimeInMilliseconds = value;
        }

        public bool IsKeyUp => _action.IsKeyUp;

        private ButtonAction _action { get; set; }

        public ButtonActionViewModel()
        {
            _action = new ButtonAction();
        }

        public ButtonActionViewModel(ButtonAction action)
        {
            _action = action;
        }

        private string GetScanCodeDisplay()
        {
            return Keyboard.GetKeyDisplayName((Win32Structures.ScanCodeShort)_action.ScanCode, _action.IsExtended);
        }
    }
}

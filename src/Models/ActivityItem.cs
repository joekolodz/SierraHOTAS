using System;

namespace SierraHOTAS.Models
{
    public class ActivityItem
    {
        public int Offset { get; set; }
        public string ButtonName { get; set; }
        public string ActionName { get; set; }
        public int ScanCode { get; set; }
        public int Flags { get; set; }
        public bool IsMacro { get; set; }
        public DateTime Time { get; set; }
        public string Key => GetScanCodeDisplay();
        public bool IsKeyUp => (Flags & (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP) == (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP;

        private string GetScanCodeDisplay()
        {
            return Keyboard.GetKeyDisplayName((Win32Structures.ScanCodeShort)ScanCode, Flags);
        }
    }
}

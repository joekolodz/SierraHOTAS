using System;

namespace SierraHOTAS.Models
{
    public class ModeSelectedEventArgs : EventArgs
    {
        public int Mode { get; set; }
        public bool IsShift { get; set; }
    }
}

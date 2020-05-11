using System;

namespace SierraHOTAS.Models
{
    public class ShiftModeChangedEventArgs : EventArgs
    {
        public int Mode { get; set; }
    }
}

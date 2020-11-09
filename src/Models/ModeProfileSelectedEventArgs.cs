using System;

namespace SierraHOTAS.Models
{
    public class ModeProfileSelectedEventArgs : EventArgs
    {
        public int Mode { get; set; }
        public bool IsShift { get; set; }
    }
}

using System;

namespace SierraHOTAS.Models
{
    public class MacroStartedEventArgs : EventArgs
    {
        public int Offset { get; set; }
        public int Code { get; set; }

        public MacroStartedEventArgs(int offset, int code)
        {
            Offset = offset;
            Code = code;
        }
    }
}

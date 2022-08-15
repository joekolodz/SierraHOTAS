using System;

namespace SierraHOTAS.Models
{
    public class RepeatCancelledEventArgs : EventArgs
    {
        public int Offset { get; set; }
        public int Code { get; set; }

        public RepeatCancelledEventArgs(int offset, int code)
        {
            Offset = offset;
            Code = code;
        }
    }
}

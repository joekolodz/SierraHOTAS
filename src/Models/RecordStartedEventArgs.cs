using System;

namespace SierraHOTAS.Models
{
    public class RepeatStartedEventArgs : EventArgs
    {
        public int Offset { get; set; }
        public int Code { get; set; }

        public RepeatStartedEventArgs(int offset, int code)
        {
            Offset = offset;
            Code = code;
        }
    }
}

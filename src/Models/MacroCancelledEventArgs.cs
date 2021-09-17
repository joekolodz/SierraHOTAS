using System;

namespace SierraHOTAS.Models
{
    public class MacroCancelledEventArgs: EventArgs
    {
        public int Offset { get; set; }
        public int Code { get; set; }

        public MacroCancelledEventArgs(int offset, int code)
        {
            Offset = offset;
            Code = code;
        }
    }
}

using System;

namespace SierraHOTAS.Models
{
    public class KeystrokeSentEventArgs : EventArgs
    {
        public int Offset { get; set; }
        public int Code { get; set; }
        public int Flags { get; set; }

        public KeystrokeSentEventArgs(int offset, int code, int flags)
        {
            Offset = offset;
            Code = code;
            Flags = flags;
        }
    }
}

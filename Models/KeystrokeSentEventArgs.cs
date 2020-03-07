using System;

namespace SierraHOTAS.Models
{
    public class KeystrokeSentEventArgs : EventArgs
    {
        public int MapId { get; set; }
        public int Offset { get; set; }
        public int Code { get; set; }
        public int Flags { get; set; }

        public KeystrokeSentEventArgs(int mapId, int offset, int code, int flags)
        {
            MapId = mapId;
            Offset = offset;
            Code = code;
            Flags = flags;
        }
    }
}

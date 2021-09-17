using System;

namespace SierraHOTAS.Models
{
    public class KeystrokeSentEventArgs : EventArgs
    {
        public int MapId { get; set; }
        public int Offset { get; set; }
        public int ScanCode { get; set; }
        public bool IsKeyUp { get; set; }
        public bool IsExtended { get; set; }

        public KeystrokeSentEventArgs(int mapId, int offset, int scanCode, bool isKeyUp, bool isExtended)
        {
            MapId = mapId;
            Offset = offset;
            ScanCode = scanCode;
            IsKeyUp = isKeyUp;
            IsExtended = isExtended;
        }
    }
}

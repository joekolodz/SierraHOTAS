using System;
using SierraJSON;

namespace SierraHOTAS.Models
{
    [SierraJsonObject(SierraJsonObject.MemberSerialization.OptIn)]
    public class AxisChangedEventArgs : EventArgs
    {
        public int AxisId { get; set; }
        public int Value { get; set; }
        public IHOTASDevice Device { get; set; }
    }
}

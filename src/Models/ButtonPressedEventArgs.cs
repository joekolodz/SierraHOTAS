using System;
using SierraJSON;

namespace SierraHOTAS.Models
{
    [SierraJsonObject(SierraJsonObject.MemberSerialization.OptIn)]
    public class ButtonPressedEventArgs : EventArgs
    {
        public int ButtonId { get; set; }
        public IHOTASDevice Device { get; set; }
    }
}

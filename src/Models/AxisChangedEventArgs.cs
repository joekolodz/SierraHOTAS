using Newtonsoft.Json;
using System;

namespace SierraHOTAS.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AxisChangedEventArgs : EventArgs
    {
        public int AxisId { get; set; }
        public int Value { get; set; }
        public IHOTASDevice Device { get; set; }
    }
}

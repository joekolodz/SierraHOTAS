using System;
using Newtonsoft.Json;

namespace SierraHOTAS.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ButtonPressedEventArgs : EventArgs
    {
        public int ButtonId { get; set; }
        public HOTASDevice Device { get; set; }
    }
}

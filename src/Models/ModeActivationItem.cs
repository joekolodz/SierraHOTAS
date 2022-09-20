using System;
using SierraJSON;

namespace SierraHOTAS.Models
{
    public class ModeActivationItem
    {
        public int Mode { get; set; }
        public int InheritFromMode { get; set; }
        public bool IsShift { get; set; }
        public string ModeName { get; set; }
        public string DeviceName { get; set; }
        public Guid DeviceId { get; set; }
        public string ButtonName { get; set; }
        public int ButtonId { get; set; }
        [SierraJsonIgnore]
        public int TemplateMode { get; set; }
    }
}

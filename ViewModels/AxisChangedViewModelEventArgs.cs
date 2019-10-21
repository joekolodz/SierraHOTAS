using SierraHOTAS.ViewModel;
using System;

namespace SierraHOTAS.ViewModels
{
    public class AxisChangedViewModelEventArgs : EventArgs
    {
        public int AxisId { get; set; }
        public int Value { get; set; }
        public DeviceViewModel Device { get; set; }
    }
}

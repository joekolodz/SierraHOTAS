using System;

namespace SierraHOTAS.ViewModels
{
    public class ButtonPressedViewModelEventArgs : EventArgs
    {
        public int ButtonId { get; set; }
        public DeviceViewModel Device { get; set; }
    }
}

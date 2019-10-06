using System;

namespace SierraHOTAS.ViewModel
{
    public class ButtonPressedViewModelEventArgs : EventArgs
    {
        public int ButtonId { get; set; }
        public DeviceViewModel Device { get; set; }
    }
}

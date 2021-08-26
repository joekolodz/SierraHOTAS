using System;

namespace SierraHOTAS.Models
{
    public class LostConnectionToDeviceEventArgs : EventArgs
    {
        public IHOTASDevice HOTASDevice { get; set; }

        public LostConnectionToDeviceEventArgs(IHOTASDevice device)
        {
            HOTASDevice = device;
        }
    }
}

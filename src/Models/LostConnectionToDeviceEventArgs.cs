using System;

namespace SierraHOTAS.Models
{
    public class LostConnectionToDeviceEventArgs : EventArgs
    {
        public HOTASDevice HOTASDevice { get; set; }

        public LostConnectionToDeviceEventArgs(HOTASDevice device)
        {
            HOTASDevice = device;
        }
    }
}

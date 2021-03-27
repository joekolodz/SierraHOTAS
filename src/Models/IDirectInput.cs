using System;
using SharpDX.DirectInput;
using System.Collections.Generic;

namespace SierraHOTAS.Models
{
    public interface IDirectInput : IDisposable
    {
        DirectInput GetDirectInput();
        IList<DeviceInstance> GetDevices(DeviceClass deviceClass, DeviceEnumerationFlags flags);
    }
}

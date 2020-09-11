using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;

namespace SierraHOTAS.Models
{
    public interface IDirectInput
    {
        DirectInput GetDirectInput();
        IList<DeviceInstance> GetDevices(DeviceClass deviceClass, DeviceEnumerationFlags flags);
    }
}

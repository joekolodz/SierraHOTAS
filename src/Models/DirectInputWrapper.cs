using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;

namespace SierraHOTAS.Models
{
    public class DirectInputWrapper : IDirectInput
    {
        private readonly DirectInput _directInput;

        public DirectInputWrapper(DirectInput directInput)
        {
            _directInput = directInput;
        }

        public DirectInput GetDirectInput()
        {
            return _directInput;
        }

        public IList<DeviceInstance> GetDevices(DeviceClass deviceClass, DeviceEnumerationFlags flags)
        {
            return _directInput.GetDevices(deviceClass, flags);
        }
    }
}

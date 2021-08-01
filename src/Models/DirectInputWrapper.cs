using SharpDX.DirectInput;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SierraHOTAS.Models
{
    [ExcludeFromCodeCoverage]
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

        public void Dispose()
        {
            _directInput.Dispose();
        }
    }
}

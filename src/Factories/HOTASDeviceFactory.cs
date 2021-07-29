using System;
using SierraHOTAS.Models;

namespace SierraHOTAS.Factories
{
    public class HOTASDeviceFactory
    {
        public virtual IHOTASDevice CreateHOTASDevice(IDirectInput directInput, Guid productGuid, Guid deviceId, string name, IHOTASQueue hotasQueue)
        {
            return new HOTASDevice(directInput, productGuid, deviceId, name, hotasQueue);
        }

        public virtual IHOTASDevice CreateHOTASDevice(IDirectInput directInput, JoystickFactory joystickFactory, Guid productGuid, Guid deviceId, string name, IHOTASQueue hotasQueue)
        {
            return new HOTASDevice(directInput, joystickFactory, productGuid, deviceId, name, hotasQueue);
        }

    }
}

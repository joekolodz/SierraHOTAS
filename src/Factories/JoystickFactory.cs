using System;
using SharpDX.DirectInput;
using SierraHOTAS.Models;

namespace SierraHOTAS.Factories
{
    public class JoystickFactory
    {
        public virtual IJoystick CreateJoystick(IDirectInput directInput, Guid deviceId)
        {
            return new JoystickWrapper(new Joystick(directInput.GetDirectInput(), deviceId));
        }
    }
}

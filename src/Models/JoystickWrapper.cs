using System.Diagnostics.CodeAnalysis;
using SharpDX;
using SharpDX.DirectInput;

namespace SierraHOTAS.Models
{
    [ExcludeFromCodeCoverage]
    public class JoystickWrapper : IJoystick
    {
        private readonly Joystick _joystick;

        public int BufferSize
        {
            get => _joystick.Properties.BufferSize;
            set => _joystick.Properties.BufferSize = value;
        }

        public Capabilities Capabilities => _joystick.Capabilities;

        public JoystickWrapper(Joystick joystick)
        {
            _joystick = joystick;
        }
       
        public void Acquire()
        {
            //TODO this does not return after the computer wakes from sleep
            _joystick.Acquire();
        }

        public void GetCurrentState(ref JoystickState joystickState)
        {
            _joystick.GetCurrentState(ref joystickState);
        }

        public void Poll()
        {
            _joystick.Poll();
        }

        public JoystickUpdate[] GetBufferedData()
        {
            return _joystick.GetBufferedData();
        }

        public void Unacquire()
        {
            _joystick.Unacquire();
        }

        public void Dispose()
        {
            _joystick.Dispose();
        }

        /// <summary>
        /// Checks the device for the existence of an axis by the given name since SharpDX only returns the count of axes but not a named list
        /// </summary>
        /// <param name="axisName"></param>
        /// <returns></returns>
        public bool IsAxisPresent(string axisName)
        {
            //a non-null object will always be returned even for properties (axes) that don't exist on the physical device
            var p = _joystick.GetObjectPropertiesByName(axisName);
            try
            {
                var _ = p.DeadZone; //accessing any property on the object will throw if that property does not exist on the device
                return true;
            }
            catch (SharpDXException)
            {
                return false;
            }
        }

    }
}

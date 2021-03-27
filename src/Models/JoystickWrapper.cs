using SharpDX.DirectInput;

namespace SierraHOTAS.Models
{
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

            //TODO z[this does not return after the computer wakes from sleep
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
    }
}

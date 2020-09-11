using SharpDX.DirectInput;

namespace SierraHOTAS.Models
{
    public interface IJoystick
    {
        int BufferSize { get; set; }
        void Acquire();
        Capabilities Capabilities { get; }
        void GetCurrentState(ref JoystickState joystickState);
        void Poll();
        JoystickUpdate[] GetBufferedData();
        void Unacquire();
        void Dispose();
    }
}

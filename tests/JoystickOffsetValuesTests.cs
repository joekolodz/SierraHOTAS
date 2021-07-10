using SierraHOTAS.Models;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class JoystickOffsetValuesTests
    {
        [Fact]
        public void get_index_name()
        {
            Assert.Equal(0, JoystickOffsetValues.GetIndex("X"));
            Assert.Equal(1, JoystickOffsetValues.GetIndex("Y"));
            Assert.Equal(2, JoystickOffsetValues.GetIndex("Z"));
        }

        [Fact]
        public void get_button_index_for_joystick_state()
        {
            Assert.Equal(0, JoystickOffsetValues.GetButtonIndexForJoystickState(47));
            Assert.Equal(0, JoystickOffsetValues.GetButtonIndexForJoystickState(48));
            Assert.Equal(1, JoystickOffsetValues.GetButtonIndexForJoystickState(49));
            Assert.Equal(127, JoystickOffsetValues.GetButtonIndexForJoystickState(176));
        }

        [Fact]
        public void get_offset()
        {
            Assert.Equal(JoystickOffset.Button1, JoystickOffsetValues.GetOffset(12));
            Assert.Equal(JoystickOffset.ForceSliders1, JoystickOffsetValues.GetOffset(163));
            Assert.Equal(JoystickOffset.X, JoystickOffsetValues.GetOffset(-1));
            Assert.Equal(JoystickOffset.X, JoystickOffsetValues.GetOffset(164));
        }

        [Fact]
        public void get_name_enum()
        {
            Assert.Equal("RY", JoystickOffsetValues.GetName(JoystickOffset.RY));
            Assert.Equal("Z", JoystickOffsetValues.GetName(JoystickOffset.Z));
            Assert.Equal("Button1", JoystickOffsetValues.GetName(JoystickOffset.Button1));
            Assert.Equal("AccelerationSliders1", JoystickOffsetValues.GetName(JoystickOffset.AccelerationSliders1));
        }

        [Fact]
        public void get_name_index()
        {
            Assert.Equal("RY", JoystickOffsetValues.GetName(16));
            Assert.Equal("Z", JoystickOffsetValues.GetName(8));
            Assert.Equal("Button1", JoystickOffsetValues.GetName(48));
            Assert.Equal("AccelerationSliders1", JoystickOffsetValues.GetName(236));
        }
    }
}

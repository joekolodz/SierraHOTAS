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
            Assert.Equal(JoystickOffset.ForceSliders1, JoystickOffsetValues.GetOffset(162));
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
            Assert.Equal("AccelerationSliders1", JoystickOffsetValues.GetName(232));
        }

        [Fact]
        public void get_pov_name()
        {
            Assert.Equal("POVNorth", JoystickOffsetValues.GetPOVName((int)JoystickOffsetValues.PointOfViewPositionValues.POVNorth));
            Assert.Equal("POVNorthEast", JoystickOffsetValues.GetPOVName((int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthEast << 8));
            Assert.Equal("POVEast", JoystickOffsetValues.GetPOVName((int)JoystickOffsetValues.PointOfViewPositionValues.POVEast << 8));
            Assert.Equal("POVSouthEast", JoystickOffsetValues.GetPOVName((int)JoystickOffsetValues.PointOfViewPositionValues.POVSouthEast << 8));
            Assert.Equal("POVSouth", JoystickOffsetValues.GetPOVName((int)JoystickOffsetValues.PointOfViewPositionValues.POVSouth << 8));
            Assert.Equal("POVSouthWest", JoystickOffsetValues.GetPOVName((int)JoystickOffsetValues.PointOfViewPositionValues.POVSouthWest << 8));
            Assert.Equal("POVWest", JoystickOffsetValues.GetPOVName((int)JoystickOffsetValues.PointOfViewPositionValues.POVWest << 8));
            Assert.Equal("POVNorthWest", JoystickOffsetValues.GetPOVName((int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthWest << 8));
        }
    }
}

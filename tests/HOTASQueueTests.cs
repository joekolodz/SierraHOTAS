using System;
using System.Collections.ObjectModel;
using NSubstitute;
using SharpDX.DirectInput;
using SierraHOTAS.Models;
using Xunit;
using JoystickOffset = SierraHOTAS.Models.JoystickOffset;

namespace SierraHOTAS.Tests
{
    public class HOTASQueueTests
    {
        private class TestJoystick_BasicQueue : IJoystick
        {
            public TestJoystick_BasicQueue()
            {
                Capabilities = new Capabilities() { AxeCount = 2, ButtonCount = 128, PovCount = 4};
            }

            public int BufferSize { get; set; }
            public void Acquire()
            {

            }

            public Capabilities Capabilities { get; }

            public void GetCurrentState(ref JoystickState joystickState)
            {
                joystickState.Buttons[0] = true;
            }

            public bool IsAxisPresent(string axisName)
            {
                return true;
            }

            public void Poll()
            {

            }

            public JoystickUpdate[] GetBufferedData()
            {
                var data = new JoystickUpdate[1];
                data[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonPressed };
                return data;
            }

            public void Unacquire()
            {

            }

            public void Dispose()
            {

            }
        }

        [Fact]
        public void translate_point_of_view_offset_same_as_button()
        {
            const int expected = -224;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, -1);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_north()
        {
            const int expected = 32;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorth);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_north_east()
        {
            const int expected = 1152032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthEast);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_east()
        {
            const int expected = 2304032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVEast);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_south_east()
        {
            const int expected = 3456032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVSouthEast);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_south()
        {
            const int expected = 4608032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVSouth);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_south_west()
        {
            const int expected = 5760032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVSouthWest);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_west()
        {
            const int expected = 6912032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVWest);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_north_west()
        {
            const int expected = 8064032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthWest);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void verify_translate_works_for_other_pov_buttons()
        {
            var expected = 36;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV2, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorth);
            Assert.Equal(expected, actual);

            expected = 8064036;
            actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV2, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthWest);
            Assert.Equal(expected, actual);

            expected = 40;
            actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV3, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorth);
            Assert.Equal(expected, actual);

            expected = 8064040;
            actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV3, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthWest);
            Assert.Equal(expected, actual);

            
            expected = 44;
            actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV4, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorth);
            Assert.Equal(expected, actual);

            expected = 8064044;
            actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV4, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthWest);
            Assert.Equal(expected, actual);
        }

        //[Fact]
        //public void tests()
        //{
        //    var joystick = new TestJoystick_BasicQueue();

        //    var map = new ObservableCollection<IHotasBaseMap>()
        //    {
        //        new HOTASButton()
        //        {
        //            MapId = (int) JoystickOffset.Button1, MapName = "trigger", Type = HOTASButton.ButtonType.Button,
        //            ActionName = "fire", ActionCatalogItem = new ActionCatalogItem()
        //            {
        //                ActionName = "fire", Actions = new ObservableCollection<ButtonAction>()
        //                {
        //                    new ButtonAction() {ScanCode = 1},
        //                    new ButtonAction() {ScanCode = 1, IsKeyUp = true}
        //                }
        //            }
        //        }
        //    };

        //    var queue = new HOTASQueue();


        //    //Assert.Raises<KeystrokeSentEventArgs>(a => queue.KeystrokeDownSent += a,
        //    //    a => queue.KeystrokeDownSent -= a,
        //    //    () => queue.Listen(joystick, map));
        //}
    }
}

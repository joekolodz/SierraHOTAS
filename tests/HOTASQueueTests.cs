using SierraHOTAS.Models;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class HOTASQueueTests
    {
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
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 0);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_north_east()
        {
            const int expected = 1152032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 4500);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_east()
        {
            const int expected = 2304032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 9000);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_south_east()
        {
            const int expected = 3456032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 13500);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_south()
        {
            const int expected = 4608032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 18000);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_south_west()
        {
            const int expected = 5760032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 22500);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_west()
        {
            const int expected = 6912032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 27000);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void translate_point_of_view_offset_north_west()
        {
            const int expected = 8064032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 31500);
            Assert.Equal(expected,actual);
        }
    }
}

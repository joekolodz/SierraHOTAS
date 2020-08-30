using SierraHOTAS.Models;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class JitterDetectionTests
    {
        [Fact]
        public void push_latest_value_roll_off()
        {
            var list = new int[4];

            JitterDetection.CalculateAveragePosition(list, list.Length, 5);
            JitterDetection.CalculateAveragePosition(list, list.Length, 6);
            JitterDetection.CalculateAveragePosition(list, list.Length, 7);
            JitterDetection.CalculateAveragePosition(list, list.Length, 8);

            Assert.Equal(8, list[0]);
            Assert.Equal(7, list[1]);
            Assert.Equal(6, list[2]);
            Assert.Equal(5, list[3]);

            JitterDetection.CalculateAveragePosition(list, list.Length, 9);
            Assert.Equal(9, list[0]);
            Assert.Equal(8, list[1]);
            Assert.Equal(7, list[2]);
            Assert.Equal(6, list[3]);
        }

        [Fact]
        public void calculate_average_position()
        {
            var list  = new int[4];

            JitterDetection.CalculateAveragePosition(list, list.Length, 6);
            JitterDetection.CalculateAveragePosition(list, list.Length, 7);
            JitterDetection.CalculateAveragePosition(list, list.Length, 8);
            var avg = JitterDetection.CalculateAveragePosition(list, list.Length, 9);
            Assert.Equal(7, avg);
        }

        [Fact]
        public void is_jitter_smooth()
        {
            var test = new JitterDetection();
            test.Threshold = 5;

            for (var i = 0; i < 20; i++)
            {
                test.IsJitter(5);
            }

            test.IsJitter(4);
            test.IsJitter(2);
            test.IsJitter(3);
            test.IsJitter(7);
            test.IsJitter(1);
            test.IsJitter(6);
            test.IsJitter(2);
            test.IsJitter(1);
            test.IsJitter(9);
            test.IsJitter(19);
            test.IsJitter(2);
            test.IsJitter(5);
            test.IsJitter(40);
            Assert.True(test.IsJitter(40));
            Assert.False(test.IsJitter(40));
            Assert.False(test.IsJitter(500));

            for (var i = 0; i < 40; i++)
            {
                test.IsJitter(501);
            }

            Assert.True(test.IsJitter(500));
        }
    }
}

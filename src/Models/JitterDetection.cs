using System;

namespace SierraHOTAS.Models
{
    public class JitterDetection
    {
        private int _threshold = 10;
        private readonly int[] _previousValues;
        private readonly int[] _previousAverages;
        private int _arraySize = 20;
        private int _arraySizeAverage = 20;

        public JitterDetection()
        {
            _previousValues = new int[_arraySize];
            _previousAverages = new int[_arraySizeAverage];
        }

        public bool IsJitter(int value)
        {
            var avg = CalculateAveragePosition(_previousValues, _arraySize, value);

            var movingDelta = CalculateMovingAveragePosition(_previousAverages, _arraySizeAverage, avg);

            return movingDelta <= _threshold;
        }

        private static int CalculateMovingAveragePosition(int[] list, int listSize, int avg)
        {
            PushLatestValue(list, listSize, avg);
            var sum = 0;
            for (var i = 0; i < listSize; i++)
            {
                sum += list[i];
            }

            var movingAvg = sum / listSize;

            var movingDelta = Math.Abs(movingAvg - avg);

            return movingDelta;
        }

        public static int CalculateAveragePosition(int[] list, int listSize, int value)
        {
            PushLatestValue(list, listSize, value);

            var sum = 0;
            for (var i = 0; i < listSize; i++)
            {
                sum += list[i];
            }

            var avg = sum / listSize;
            return avg;
        }

        private static void PushLatestValue(int[] list, int listSize, int value)
        {
            for (var i = listSize - 1; i > 0; i--)
            {
                list[i] = list[i - 1];
            }
            list[0] = value;
        }
    }
}

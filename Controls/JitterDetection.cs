namespace SierraHOTAS.Controls
{
    public class JitterDetection
    {
        private readonly int[] _lastThreeValues;
        private int _arraySize = 3;

        public JitterDetection()
        {
            _lastThreeValues = new int[_arraySize];
        }

        public bool IsJitter(int value)
        {
            if (IsJitterDetected(value))
            {
                return true;
            }
            PushLatestValue(value);
            return false;
        }

        private void PushLatestValue(int value)
        {
            for (var i = _arraySize - 1; i > 0; i--)
            {
                _lastThreeValues[i] = _lastThreeValues[i - 1];
            }
            _lastThreeValues[0] = value;
        }

        private bool IsJitterDetected(int value)
        {
            for (var i = 0; i < _arraySize; i++)
            {
                if (_lastThreeValues[i] == value) return true;
            }
            return false;
        }
    }
}

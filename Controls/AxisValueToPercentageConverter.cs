using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SierraHOTAS.Controls
{
    public class AxisValueToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var axisBoundary = (int)value;
            var percent = axisBoundary / 655;
            return (int)percent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value as string;
            int.TryParse(stringValue, out var axisValue);
            axisValue *= 655;
            return axisValue;
        }
    }
}

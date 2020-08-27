using SierraHOTAS.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SierraHOTAS.Controls
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case null:
                    return Visibility.Collapsed;
                case int count:
                    if (count > 1) return Visibility.Visible;
                    break;
                case AxisMapViewModel axisMapViewModel:
                    {
                        if (axisMapViewModel.SegmentCount > 0) return Visibility.Visible;
                        break;
                    }
                default:
                    {
                        if (!(value is ObservableCollection<ButtonMapViewModel> listButtonMapVm)) return Visibility.Collapsed;
                        if (listButtonMapVm.Count > 0) return Visibility.Visible;
                        break;
                    }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

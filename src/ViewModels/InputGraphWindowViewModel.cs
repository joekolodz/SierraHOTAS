using SierraHOTAS.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SierraHOTAS.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class InputGraphWindowViewModel
    {
        public Dispatcher AppDispatcher { get; set; }
        public ObservableCollection<InputGraphPointNode> Points { get; set; }
        public InputGraphWindowViewModel(Action<EventHandler<AxisChangedEventArgs>> handler)
        {
            //Points = new ObservableCollection<InputGraphPointNode>();
            //handler(AxisChangedHandler);
        }

        private void AxisChangedHandler(object sender, AxisChangedEventArgs e)
        {
            Logging.Log.Debug($"Device:{e.Device.Name}, Axis:{e.AxisId}, Value:{e.Value}");

            AppDispatcher.Invoke(Go);
        }

        private void Go()
        {
            if (Points.Count > 1000) Points.RemoveAt(0);
            var node = new InputGraphPointNode()
            {
                X = 0,
                Y = 0,
                Stroke = new SolidColorBrush(Colors.DarkRed),
                Geometry = new EllipseGeometry()
                {
                    Center = new Point(0, 0), 
                    RadiusX = 5, 
                    RadiusY = 5
                }
            };
            Points.Add(node);
        }
    }
}

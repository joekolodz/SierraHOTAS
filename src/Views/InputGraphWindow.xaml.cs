using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SierraHOTAS.Views
{
    /// <summary>
    /// Interaction logic for InputGraphWindow.xaml
    /// </summary>
    public partial class InputGraphWindow : Window
    {
        private List<IHOTASDevice> _axisDeviceList;
        private int _graphScale;
        private readonly Action<EventHandler<AxisChangedEventArgs>> _callBackRemoveHandler;
        private Point _windowPosition;
        private DispatcherTimer _dispatcherTimer;
        private int _meterStrokeThickness = 2;

        public InputGraphWindow(IHOTASCollection deviceList, Action<EventHandler<AxisChangedEventArgs>> handler, Action<EventHandler<AxisChangedEventArgs>> callBackRemoveHandler)
        {
            InitializeComponent();

            DataContext = this;

            foreach (var d in deviceList.Devices)
            {
                if (d.Capabilities.AxeCount <= 0) continue;
                if (_axisDeviceList == null) _axisDeviceList = new List<IHOTASDevice>();
                _axisDeviceList.Add(d);
            }
            //bind the names to a checkbox control and draw a canvas for each device selected?
            //the devices in _axisDeviceList will have the names of the true axis that are actually on the device


            handler(AxisChangedHandler);
            _callBackRemoveHandler = callBackRemoveHandler;

            SizeChanged += InputGraphWindow_OnSizeChanged;
            CalculateScale((int)LineGraphCanvas.Height);

            _dispatcherTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 25), DispatcherPriority.Render, DrawLoop, Dispatcher.CurrentDispatcher);
        }


        private double _meterPosition = 0d;
        private readonly Line _meterLine = new Line() { Name = "Meter" };
        private void DrawLoop(object sender, EventArgs e)
        {
            LineGraphCanvas.Children.Remove(_meterLine);

            ++_meterPosition;
            if (_meterPosition > LineGraphCanvas.ActualWidth) _meterPosition = 0;

            _meterLine.Stroke = new SolidColorBrush(Colors.Yellow);
            _meterLine.StrokeThickness = _meterStrokeThickness;
            _meterLine.X1 = _meterPosition;
            _meterLine.Y1 = 0;
            _meterLine.X2 = _meterPosition;
            _meterLine.Y2 = LineGraphCanvas.ActualHeight;

            LineGraphCanvas.Children.Add(_meterLine);

            var removeUiElements = new List<UIElement>();

            var meterPositionBoundary = _meterPosition + _meterStrokeThickness + 6;

            foreach (UIElement child in LineGraphCanvas.Children)
            {

                if (child is Ellipse)
                {
                    var x = child as Ellipse;
                    var p = child.PointToScreen(new Point(0, 0));//Ellipse uses the parent canvas X,Y coord. it does not have its own
                    var beforeX = p.X;
                    p.X -= _windowPosition.X;
                    //p.Y -= _windowPosition.Y;


                    //Debug.WriteLine($"Meter: {_meterPosition}, Dot:{beforeX}, Window:{_windowPosition.X}, new Dot:{p.X}");

                    if (Math.Abs(p.X - meterPositionBoundary) < .01)
                    {
                        removeUiElements.Add(x);
                    }
                }
            }

            foreach (var child in removeUiElements)
            {
                LineGraphCanvas.Children.Remove(child);
            }

        }

        private void InputGraphWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateScale((int)LineGraphCanvas.ActualHeight);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            _windowPosition = new Point(Left, Top);
            base.OnLocationChanged(e);
        }

        private void CalculateScale(int height)
        {
            _graphScale = 65535 / (height - 80);
        }

        private void AxisChangedHandler(object sender, AxisChangedEventArgs e)
        {
            TrackAxis(e);
        }

        private void TrackAxis(AxisChangedEventArgs e)
        {
            var c = Colors.White;
            var scaledValue = e.Value / _graphScale;


            if (e.Device.Name.ToLower().Contains("stick"))
            {
                switch (e.AxisId)
                {
                    case 0:
                        c = Colors.Coral;
                        break;
                    case 4:
                        c = Colors.Pink;
                        break;
                    case 8:
                        c = Colors.Fuchsia;
                        //scaledValue = e.Value / 80;
                        break;
                }
            }

            if (e.Device.Name.ToLower().Contains("throttle"))
            {
                switch (e.AxisId)
                {
                    case 0:
                        c = Colors.Red;
                        //scaledValue = e.Value / 80;
                        break;
                    case 4:
                        c = Colors.Yellow;
                        //scaledValue = e.Value / 80;
                        break;
                    case 8:
                        c = Colors.Cyan;
                        //scaledValue = e.Value / 80;
                        break;
                    case 12:
                        c = Colors.MediumPurple;
                        break;
                    case 16:
                        c = Colors.LimeGreen;
                        break;
                    case 20:
                        c = Colors.DeepSkyBlue;
                        break;
                }
            }

            Dispatcher.InvokeAsync(() => Draw1(e.AxisId, scaledValue, c));

        }

        private void Draw1(int axisId, int value, Color color)
        {
            var dot = new Ellipse()
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1,
                Fill = new SolidColorBrush(color),
                Width = 2,
                Height = 2
            };

            Canvas.SetLeft(dot, _meterPosition - _meterStrokeThickness);
            Canvas.SetBottom(dot, value);
            LineGraphCanvas.Children.Add(dot);
        }

        protected override void OnClosed(EventArgs e)
        {
            _dispatcherTimer.Stop();
            _callBackRemoveHandler(AxisChangedHandler);
        }
    }
}

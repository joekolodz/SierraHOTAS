using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class InputGraphWindow : Window, INotifyPropertyChanged
    {
        public Dispatcher AppDispatcher { get; set; }
        
        private int _graphScale;
        private int _deviation;
        private int _min;
        private int _max;
        public int Deviation
        {
            get => _deviation;
            set
            {
                _deviation = value;
                OnPropertyChanged(nameof(Deviation));
            }
        }

        public int Min
        {
            get => _min;
            set
            {
                _min = value;
                OnPropertyChanged(nameof(Min));
            }
        }

        public int Max
        {
            get => _max;
            set
            {
                _max = value;
                OnPropertyChanged(nameof(Max));
            }
        }

        private readonly Dictionary<int, Collection<int>> _pointsDictionary;
        
        private readonly int[] _shiftList;


        private readonly Action<EventHandler<AxisChangedEventArgs>> _callBackRemoveHandler;
        private bool _isCapturing;
        public InputGraphWindow(Action<EventHandler<AxisChangedEventArgs>> handler, Action<EventHandler<AxisChangedEventArgs>> callBackRemoveHandler)
        {
            InitializeComponent();

            DataContext = this;

            _pointsDictionary = new Dictionary<int, Collection<int>>();
            _shiftList = new int[28];//from JoysticOffset.cs

            AppDispatcher = Dispatcher;
            handler(AxisChangedHandler);

            _callBackRemoveHandler = callBackRemoveHandler;

            _graphScale = (int)Width;

            SizeChanged += InputGraphWindow_OnSizeChanged;
        }

        private void InputGraphWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _graphScale = 65535 / (int)ActualHeight;
        }

        private void AxisChangedHandler(object sender, AxisChangedEventArgs e)
        {
            TrackAxis(e);
        }

        private void TrackAxis(AxisChangedEventArgs e)
        {
            //var axisName = JoystickOffsetValues.GetName(e.AxisId);

            if (!_pointsDictionary.ContainsKey(e.AxisId))
            {
                _pointsDictionary.Add(e.AxisId, new Collection<int>());
            }

            var points = _pointsDictionary[e.AxisId];

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
                        scaledValue = e.Value / 80;
                        break;
                }
            }

            if (e.Device.Name.ToLower().Contains("throttle"))
            {
                if (_isCapturing && e.AxisId == 12)
                {
                    if (points.Count > 100)
                        points.RemoveAt(0);

                    points.Add(e.Value);
                    Max = points.Max();
                    Min = points.Min();
                    Deviation = Max - Min;
                }

                switch (e.AxisId)
                {
                    case 0:
                        c = Colors.Red;
                        scaledValue = e.Value / 80;
                        break;
                    case 4:
                        c = Colors.Yellow;
                        scaledValue = e.Value / 80;
                        break;
                    case 8:
                        c = Colors.Cyan;
                        scaledValue = e.Value / 80;
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


            AppDispatcher.Invoke(() => Draw1(e.AxisId, scaledValue, c));
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

            var shift = _shiftList[axisId];

            Canvas.SetLeft(dot, shift++);
            Canvas.SetBottom(dot, value);
            LineGraphCanvas.Children.Add(dot);

            if (shift > ActualWidth)
            {
                shift = 0;
                LineGraphCanvas.Children.Clear();
            }

            _shiftList[axisId] = shift;
        }

        private void StartCapture_OnClick(object sender, RoutedEventArgs e)
        {
            _isCapturing = true;
        }

        private void StopCapture_OnClick(object sender, RoutedEventArgs e)
        {
            _isCapturing = false;
            Deviation = 0;
            Max = 0;
            Min = 0;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape) CloseInternal();
        }

        private void CloseInternal()
        {
            _callBackRemoveHandler(AxisChangedHandler);

            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

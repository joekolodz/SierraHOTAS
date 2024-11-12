using SierraHOTAS.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SierraHOTAS.Controls
{
    public partial class RadialAxisMap : UserControl
    {
        private static int instances = 0;
        private Guid id;
        private AxisMapViewModel _axisVm;
        private Line _arc;
        private Ellipse _circle;
        private readonly List<Line> _segmentLines;
        private double _gaugeDiameter = 40;
        private readonly Color _directionalColor;

        public static DependencyProperty AxisHandProperty = DependencyProperty.Register(nameof(AxisHand), typeof(int), typeof(RadialAxisMap), new FrameworkPropertyMetadata(0, OnPropertyChangedChanged));
        public static DependencyProperty IsMultiActionProperty = DependencyProperty.Register(nameof(IsMultiAction), typeof(int), typeof(RadialAxisMap), new FrameworkPropertyMetadata(0, OnPropertyChangedChanged));
        public static DependencyProperty SegmentCountProperty = DependencyProperty.Register(nameof(SegmentCount), typeof(int), typeof(RadialAxisMap), new FrameworkPropertyMetadata(0, OnPropertyChangedChanged));
        public static DependencyProperty SegmentBoundaryProperty = DependencyProperty.Register(nameof(SegmentBoundary), typeof(int), typeof(RadialAxisMap), new FrameworkPropertyMetadata(0, OnPropertyChangedChanged));
        
        private static void OnPropertyChangedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is RadialAxisMap prop)) return;
            if(e.Property.Name == nameof(AxisHand)) prop.DrawCircle((int)e.NewValue);
            if(e.Property.Name == nameof(IsMultiAction) || e.Property.Name == nameof(SegmentCount)) prop.OnSegmentsChanged();
            if(e.Property.Name == nameof(SegmentBoundary)) prop.ChangeSegmentBoundary();
        }

        public int AxisHand
        {
            get => (int)GetValue(AxisHandProperty);
            set => SetValue(AxisHandProperty, value);
        }

        public int IsMultiAction
        {
            get => (int)GetValue(IsMultiActionProperty);
            set => SetValue(IsMultiActionProperty, value);
        }

        public int SegmentCount
        {
            get => (int)GetValue(SegmentCountProperty);
            set => SetValue(SegmentCountProperty, value);
        }

        public int SegmentBoundary
        {
            get => (int)GetValue(SegmentBoundaryProperty);
            set => SetValue(SegmentBoundaryProperty, value);
        }

        public RadialAxisMap()
        {
            InitializeComponent();
            instances++;

            _segmentLines = new List<Line>();
            _directionalColor = (Color)ColorConverter.ConvertFromString("#80e5ff");

            DataContextChanged += AxisMap_DataContextChanged;

            CreateDial();

            id = Guid.NewGuid();
            Debug.WriteLine($"RadialAxisMap Constructor - instances:{instances} - {id}");
        }

        private bool SegmentFilter(object segment)
        {
            return _axisVm.SegmentFilter(segment);
        }

        private void AxisMap_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _axisVm = DataContext as AxisMapViewModel;
            if (_axisVm == null) return;

            SetSegmentBoundaryFilter();

            OnSegmentsChanged();
        }

        private void ChangeSegmentBoundary()
        {
            Dispatcher.Invoke(() =>
            {
                RemoveAllSegmentLines();
                DrawSegmentBoundaries();
            });
        }

        private void SetSegmentBoundaryFilter()
        {
            lstSegments.ItemsSource = _axisVm.Segments;
            var view = (CollectionView)CollectionViewSource.GetDefaultView(lstSegments.ItemsSource);
            if (view != null)
            {
                view.Filter = SegmentFilter;
            }
        }

        private void OnSegmentsChanged()
        {
            RemoveAllSegmentLines();

            if (_axisVm.SegmentCount < 1) return;

            DrawSegmentBoundaries();

            //refreshes the filter so we don't see the last segment which is always pinned to 100%
            CollectionViewSource.GetDefaultView(lstSegments.ItemsSource).Refresh();
        }

        private void RemoveAllSegmentLines()
        {
            foreach (var line in _segmentLines)
            {
                CanvasPlaceHolder.Children.Remove(line);
            }
            _segmentLines.Clear();
        }

        private void DrawSegmentBoundaries()
        {
            const double canvasLeft = 0;
            const double canvasTop = 0;
            const double angleRatio = ushort.MaxValue / 360.0f; //(joystick axis values range from 0 to 65535)

            foreach (var keyValue in _axisVm.Segments)
            {
                if (keyValue.Value >= ushort.MaxValue - 655) continue;
                var angle = keyValue.Value / angleRatio; //compress into degrees to get the angle on the circle
                var theta = angle;

                var radius = _gaugeDiameter / 2; //canvas_axis_dial.Height / 2;
                var h = canvasLeft + radius;
                var k = canvasTop + radius;

                var radians = (Math.PI / 180) * theta; //turn joystick angle into radians
                var x = h + radius * Math.Sin(radians);
                var y = k - radius * Math.Cos(radians);

                var newH = h + (radius / 1.5) * Math.Sin(radians);
                var newK = k - (radius / 1.5) * Math.Cos(radians);

                var arc = new Line()
                {
                    StrokeThickness = 4,
                    Stroke = Brushes.WhiteSmoke,
                    X1 = newH,
                    Y1 = newK
                };
                arc.X2 = x;
                arc.Y2 = y;

                CanvasPlaceHolder.Children.Add(arc);
                _segmentLines.Add(arc);
            }
        }

        private void CreateDial()
        {
            var h = _gaugeDiameter / 2;
            var k = _gaugeDiameter / 2;

            _arc = new Line()
            {
                StrokeThickness = 4,
                Stroke = Brushes.Gold,
                X1 = h,
                Y1 = k,
                X2 = h,
                Y2 = 0
            };

            var zeroLine = new Line()
            {
                StrokeThickness = 4,
                Stroke = Brushes.Red,
                X1 = h,
                Y1 = k / 2,
                X2 = h,
                Y2 = 0
            };

            _circle = new Ellipse()
            {
                StrokeThickness = 3,
                Width = _gaugeDiameter,
                Height = _gaugeDiameter,
                Fill = new SolidColorBrush(Colors.Transparent),
                Stroke = Brushes.Gold
            };
            Canvas.SetLeft(_circle, 0);
            Canvas.SetTop(_circle, 0);

            CanvasPlaceHolder.Children.Add(_arc);
            CanvasPlaceHolder.Children.Add(zeroLine);
            CanvasPlaceHolder.Children.Add(_circle);
        }
        private void DrawCircle(int angle)
        {
            const double canvasLeft = 0;
            const double canvasTop = 0;
            const double angleRatio = ushort.MaxValue / 360.0f; //(joystick axis values range from 0 to 65535)
            var theta = angle / angleRatio; //compress joystick value into degrees to get the angle on the circle

            var radius = _gaugeDiameter / 2;
            var h = canvasLeft + radius;
            var k = canvasTop + radius;

            if (_arc == null)
            {
                CreateDial();
            }

            var radians = (Math.PI / 180) * theta; //turn joystick angle into radians
            var x = h + radius * Math.Sin(radians);
            var y = k - radius * Math.Cos(radians);

            _arc.X2 = x;
            _arc.Y2 = y;

            SetDirectionalColor();
        }

        private void SetDirectionalColor()
        {
            if (_axisVm.Direction == AxisDirection.Forward)
            {
                _arc.Stroke = Brushes.Gold;
                _circle.Stroke = Brushes.Gold;
            }
            else
            {
                _arc.Stroke = new SolidColorBrush(_directionalColor);
                _circle.Stroke = new SolidColorBrush(_directionalColor);
            }
        }
    }
}

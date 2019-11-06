using System;
using System.Collections.Generic;
using System.Windows;
using SierraHOTAS.ViewModels;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SierraHOTAS.Models
{
    public partial class RadialAxisMap : UserControl
    {
        private AxisMapViewModel _axisVm;
        private Line _arc;
        private Ellipse _circle;
        private readonly List<Line> _segmentLines;
        private double _gaugeDiameter = 40;
        private readonly Color _directionalColor;


        public RadialAxisMap()
        {
            InitializeComponent();

            txtSegments.TextChanged += OnSegmentsTextChanged;
            _segmentLines = new List<Line>();
            _directionalColor = (Color)ColorConverter.ConvertFromString("#80e5ff");

            DataContextChanged += AxisMap_DataContextChanged;
            CreateDial();
        }

        private void _axisVm_OnAxisValueChanged(object sender, int axisValue)
        {
            DrawCircle(axisValue);
        }

        private void AxisMap_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_axisVm != null) _axisVm.OnAxisValueChanged -= _axisVm_OnAxisValueChanged;

            _axisVm = DataContext as AxisMapViewModel;
            if (_axisVm == null) return;

            _axisVm.OnAxisValueChanged += _axisVm_OnAxisValueChanged;
        }

        private void OnSegmentsTextChanged(object sender, TextChangedEventArgs e)
        {
            RemoveAllSegmentLines();

            var success = int.TryParse(txtSegments.Text, out var segments);
            if (!success) return;

            if (segments < 1) return;

            DrawSegmentBoundaries(segments);
        }

        private void RemoveAllSegmentLines()
        {
            foreach (var line in _segmentLines)
            {
                CanvasPlaceHolder.Children.Remove(line);
            }
            _segmentLines.Clear();
        }

        private void DrawSegmentBoundaries(int segments)
        {
            const double canvasLeft = 0;
            const double canvasTop = 0;
            const double angleRatio = ushort.MaxValue / 360.0f; //(joystick axis values range from 0 to 65535)

            var segmentDegrees = 360 / segments;

            for (var s = 1; s < segments; s++)
            {
                var angle = segmentDegrees * s;
                var theta = angle / angleRatio; //compress into degrees to get the angle on the circle
                theta = angle;

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

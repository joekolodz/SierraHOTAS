using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SierraHOTAS.Controls
{
    /// <summary>
    /// Interaction logic for RadialAxisMap.xaml
    /// </summary>
    public partial class RadialAxisMap : UserControl
    {
        private Line _arc;
        private Ellipse _circle;
        private List<Line> _segmentLines;
        private Dictionary<int, int> _segmentRanges;
        private int _currentSegment;

        public double GaugeDiameter { get; set; } = 40;

        private readonly Color _directionalColor;

        private int _lastValue;
        private AxisDirection _direction;
        private readonly JitterDetection _jitter;

        private bool _isDirectional;

        private MediaPlayer _mediaPlayer;

        public RadialAxisMap()
        {
            InitializeComponent();

            _jitter = new JitterDetection();
            _segmentLines = new List<Line>();
            _segmentRanges = new Dictionary<int, int>();

            _directionalColor = (Color)ColorConverter.ConvertFromString("#80e5ff");

            chkIsDirectional.Checked += ChkIsDirectional_Checked;
            chkIsDirectional.Unchecked += ChkIsDirectional_Checked;
            txtSegments.TextChanged += TxtSegments_TextChanged;

            _mediaPlayer = new MediaPlayer { Volume = 0f };
            _mediaPlayer.Open(new Uri(@"Sounds\click04.mp3", UriKind.Relative));
        }

        private void TxtSegments_TextChanged(object sender, TextChangedEventArgs e)
        {
            ResetSegments();

            var success = int.TryParse(txtSegments.Text, out var segments);
            if (!success) return;

            if (segments < 1) return;

            if (segments == 1)
            {
                _currentSegment = 1;
                return;
            }

            CalculateSegmentRange(segments);
            DrawSegmentBoundaries(segments);
        }

        private void CalculateSegmentRange(int segments)
        {
            var segmentRangeBoundary = ushort.MaxValue / (segments);
            _segmentRanges.Clear();
            for (var i = 1; i < segments; i++)
            {
                _segmentRanges.Add(i, (segmentRangeBoundary * i));
            }

            _segmentRanges.Add(segments, ushort.MaxValue);
        }

        private void ResetSegments()
        {
            _segmentRanges.Clear();
            RemoveAllSegmentLines();
        }

        private void RemoveAllSegmentLines()
        {
            foreach (var line in _segmentLines)
            {
                canvas_axis_dial.Children.Remove(line);
            }
        }

        private void DrawSegmentBoundaries(int segments)
        {
            //var segmentWidth = (int)(GaugeWidth / segments);

            //for (var s = 1; s < segments; s++)
            //{
            //    var line = new Line();
            //    line.X1 = segmentWidth * s;
            //    line.Y1 = 0;

            //    line.X2 = line.X1;
            //    line.Y2 = 18;

            //    line.Stroke = new SolidColorBrush(Colors.WhiteSmoke);
            //    line.StrokeThickness = 2;

            //    Canvas.SetLeft(line, 0);
            //    Canvas.SetBottom(line, 1);

            //    canvas_axis_bar.Children.Add(line);
            //    _segmentLines.Add(line);
            //}

            const double canvasLeft = 0;
            const double canvasTop = 0;
            const double angleRatio = ushort.MaxValue / 360.0f; //(joystick axis values range from 0 to 65535)

            var segmentDegrees = 360 / segments;

            for (var s = 1; s < segments; s++)
            {
                var angle = segmentDegrees * s;
                var theta = angle / angleRatio; //compress into degrees to get the angle on the circle
                theta = angle;

                var radius = GaugeDiameter / 2; //canvas_axis_dial.Height / 2;
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

                canvas_axis_dial.Children.Add(arc);
                _segmentLines.Add(arc);
            }

        }

        private void ChkIsDirectional_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            _isDirectional = chkIsDirectional.IsChecked ?? false;
            if (!_isDirectional) _direction = AxisDirection.Forward;
        }

        public void SetAxis(int value)
        {
            if (_jitter.IsJitter(value)) return;

            SetDirection(value);
            DetectSelectedSegment(value);
            DrawCircle(value);
        }

        private void SetDirection(int value)
        {
            if (_isDirectional)
            {
                _direction = value < _lastValue ? AxisDirection.Backward : AxisDirection.Forward;
            }
            _lastValue = value;
        }

        private void DetectSelectedSegment(int value)
        {
            if (_segmentRanges.Count <= 1) return;
            var segmentRange = _segmentRanges.FirstOrDefault(r => value <= r.Value);
            var newSegment = segmentRange.Key;

            if (newSegment != _currentSegment)
            {
                _currentSegment = newSegment;
                _mediaPlayer.Volume = 1f;
                _mediaPlayer.Play();
                _mediaPlayer.Position = TimeSpan.Zero;
                Logging.Log.Info($"New Segment: {_currentSegment}");
            }
        }

        private void DrawCircle(int angle)
        {
            const double canvasLeft = 0;
            const double canvasTop = 0;
            const double angleRatio = ushort.MaxValue / 360.0f; //(joystick axis values range from 0 to 65535)
            var theta = angle / angleRatio; //compress joystick value into degrees to get the angle on the circle

            var radius = GaugeDiameter / 2; //canvas_axis_dial.Height / 2;
            var ellipseWidth = radius * 2;
            var ellipseHeight = radius * 2;
            var h = canvasLeft + radius;
            var k = canvasTop + radius;


            if (_arc == null)
            {
                _arc = new Line()
                {
                    StrokeThickness = 4,
                    Stroke = Brushes.Gold,
                    X1 = h,
                    Y1 = k
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
                    Width = ellipseWidth,
                    Height = ellipseHeight,
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = Brushes.Gold
                };
                Canvas.SetLeft(_circle, 0);
                Canvas.SetTop(_circle, 0);

                canvas_axis_dial.Children.Add(_arc);
                canvas_axis_dial.Children.Add(zeroLine);
                canvas_axis_dial.Children.Add(_circle);
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
            if (_direction == AxisDirection.Forward)
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

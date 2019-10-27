using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SierraHOTAS.Controls
{
    public enum AxisDirection
    {
        Forward,
        Backward
    }

    public partial class LinearAxisMap : UserControl
    {
        private Rectangle _rectLinearGauge;
        private Rectangle _rectLimitBorder;
        private List<Line> _segmentLines;
        private Dictionary<int, int> _segmentRanges;
        private int _currentSegment;

        public double GaugeWidth { get; set; } = 120;
        public double GaugeHeight { get; set; } = 20;

        private readonly Color _directionalColor;

        private int _lastValue;
        private AxisDirection _direction = AxisDirection.Forward;
        private readonly JitterDetection _jitter;

        private bool _isDirectional;
        private readonly MediaPlayer _mediaPlayer;

        public LinearAxisMap()
        {
            InitializeComponent();

            _jitter = new JitterDetection();
            _segmentLines = new List<Line>();
            _segmentRanges = new Dictionary<int, int>();

            _directionalColor = (Color)ColorConverter.ConvertFromString("#80e5ff");

            if (GaugeHeight > Height) GaugeHeight = Height;
            if (GaugeWidth > Width) GaugeWidth = Width;

            chkIsDirectional.Checked += ChkIsDirectional_Checked;
            chkIsDirectional.Unchecked += ChkIsDirectional_Checked;
            txtSegments.TextChanged += TxtSegments_TextChanged;

            _mediaPlayer = new MediaPlayer {Volume = 0f};
            _mediaPlayer.Open(new Uri(@"Sounds\click05.mp3", UriKind.Relative));
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
                canvas_axis_bar.Children.Remove(line);
            }
        }

        private void DrawSegmentBoundaries(int segments)
        {
            var segmentWidth = (int)(GaugeWidth / segments);

            for (var s = 1; s < segments; s++)
            {
                var line = new Line();
                line.X1 = segmentWidth * s;
                line.Y1 = 0;

                line.X2 = line.X1;
                line.Y2 = 18;

                line.Stroke = new SolidColorBrush(Colors.WhiteSmoke);
                line.StrokeThickness = 2;

                Canvas.SetLeft(line, 0);
                Canvas.SetBottom(line, 1);

                canvas_axis_bar.Children.Add(line);
                _segmentLines.Add(line);
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
            DrawRectangle(value);
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

            var newSegment = GetSegmentFromRawValue(value);

            if (newSegment != _currentSegment)
            {
                _currentSegment = newSegment;
                _mediaPlayer.Volume = 1f;
                _mediaPlayer.Play();
                _mediaPlayer.Position = TimeSpan.Zero;
            }
        }

        private int GetSegmentFromRawValue(int value)
        {
            var segmentRange = _segmentRanges.FirstOrDefault(r => value <= r.Value);
            return segmentRange.Key;
        }

        private void DrawRectangle(int width)
        {
            if (_rectLinearGauge == null)
            {
                CreateAxisBar();
            }

            var ratio = 65535 / GaugeWidth;
            double w = width;
            w /= ratio;
            var final = Math.Round(w);

            _rectLinearGauge.Width = final;

            SetDirectionalColor();
        }

        private void CreateAxisBar()
        {
            _rectLinearGauge = new Rectangle()
            {
                Stroke = new SolidColorBrush(Colors.Gold),
                Fill = new SolidColorBrush(Colors.Gold),
                Width = 1,
                Height = GaugeHeight
            };
            _rectLimitBorder = new Rectangle()
            {
                Stroke = new SolidColorBrush(Colors.Gold),
                Fill = new SolidColorBrush(Colors.Transparent),
                Width = GaugeWidth,
                Height = GaugeHeight
            };
            Canvas.SetLeft(_rectLinearGauge, 0);
            Canvas.SetBottom(_rectLinearGauge, 0);

            Canvas.SetLeft(_rectLimitBorder, 0);
            Canvas.SetBottom(_rectLimitBorder, 0);

            canvas_axis_bar.Children.Add(_rectLimitBorder);
            canvas_axis_bar.Children.Add(_rectLinearGauge);
        }

        private void SetDirectionalColor()
        {
            if (_direction == AxisDirection.Forward)
            {
                _rectLinearGauge.Fill = new SolidColorBrush(Colors.Gold);
                _rectLinearGauge.Stroke = new SolidColorBrush(Colors.Gold);
                _rectLimitBorder.Stroke = new SolidColorBrush(Colors.Gold);
            }
            else
            {
                _rectLinearGauge.Fill = new SolidColorBrush(_directionalColor);
                _rectLinearGauge.Stroke = new SolidColorBrush(_directionalColor);
                _rectLimitBorder.Stroke = new SolidColorBrush(_directionalColor);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using SierraHOTAS.ViewModels;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SierraHOTAS.Controls
{
    public partial class LinearAxisMap : UserControl
    {
        private Rectangle _rectLinearGauge;
        private Rectangle _rectLimitBorder;
        private readonly List<Line> _segmentLines;
        private readonly Color _directionalColor;
        private LinearAxisMapViewModel _axisVm;

        private double _gaugeWidth = 120;
        private double _gaugeHeight = 20;


        public LinearAxisMap()
        {
            InitializeComponent();

            txtSegments.TextChanged += OnSegmentsTextChanged;
            _segmentLines = new List<Line>();
            _directionalColor = (Color)ColorConverter.ConvertFromString("#80e5ff");

            DataContextChanged += LinearAxisMap_DataContextChanged;

            CreateAxisBar();
        }

        private void _axisVm_OnAxisValueChanged(object sender, int axisValue)
        {
            DrawRectangle(axisValue);
        }

        private void LinearAxisMap_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(_axisVm!=null)_axisVm.OnAxisValueChanged -= _axisVm_OnAxisValueChanged;

            _axisVm = DataContext as LinearAxisMapViewModel;
            if (_axisVm == null) return;

            _axisVm.OnAxisValueChanged += _axisVm_OnAxisValueChanged;
        }

        private void OnSegmentsTextChanged(object sender, TextChangedEventArgs e)
        {
            RemoveAllSegmentLines();

            var success = int.TryParse(txtSegments.Text, out var segments);
            if (!success) return;

            if (segments < 1) return;

            _axisVm.SegmentsCountChanged(segments);
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
            var segmentWidth = (int)(_gaugeWidth / segments);

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

                CanvasPlaceHolder.Children.Add(line);
                _segmentLines.Add(line);
            }
        }

        private void DrawRectangle(int width)
        {
            if (_rectLinearGauge == null)
            {
                CreateAxisBar();
            }

            var ratio = 65535 / _gaugeWidth;
            double w = width;
            w /= ratio;
            var final = Math.Round(w);

            _rectLinearGauge.Width = final;

            SetDirectionalColor();
        }

        private void CreateAxisBar()
        {
            if (CanvasPlaceHolder.Children.Contains(_rectLimitBorder))
            {
                CanvasPlaceHolder.Children.Remove(_rectLimitBorder);
            }

            if (CanvasPlaceHolder.Children.Contains(_rectLimitBorder))
            {
                CanvasPlaceHolder.Children.Remove(_rectLimitBorder);
            }

            _rectLinearGauge = new Rectangle()
            {
                Stroke = new SolidColorBrush(Colors.Gold),
                Fill = new SolidColorBrush(Colors.Gold),
                Width = 1,
                Height = _gaugeHeight
            };
            _rectLimitBorder = new Rectangle()
            {
                Stroke = new SolidColorBrush(Colors.Gold),
                Fill = new SolidColorBrush(Colors.Transparent),
                Width = _gaugeWidth,
                Height = _gaugeHeight
            };
            Canvas.SetLeft(_rectLinearGauge, 0);
            Canvas.SetBottom(_rectLinearGauge, 0);

            Canvas.SetLeft(_rectLimitBorder, 0);
            Canvas.SetBottom(_rectLimitBorder, 0);

            CanvasPlaceHolder.Children.Add(_rectLimitBorder);
            CanvasPlaceHolder.Children.Add(_rectLinearGauge);
        }

        private void SetDirectionalColor()
        {
            if (_axisVm.Direction == AxisDirection.Forward)
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

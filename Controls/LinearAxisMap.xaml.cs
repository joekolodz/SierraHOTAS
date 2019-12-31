using SierraHOTAS.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        private AxisMapViewModel _axisVm;

        private double _gaugeWidth = 120;
        private double _gaugeHeight = 20;


        public LinearAxisMap()
        {
            InitializeComponent();

            _segmentLines = new List<Line>();
            _directionalColor = (Color)ColorConverter.ConvertFromString("#80e5ff");

            DataContextChanged += AxisMap_DataContextChanged;

            CreateAxisBar();
        }

        private bool SegmentFilter(object segment)
        {
            return _axisVm.SegmentFilter(segment);
        }

        private void _axisVm_OnAxisValueChanged(object sender, int axisValue)
        {
            DrawRectangle(axisValue);
        }

        private void AxisMap_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_axisVm != null)
            {
                _axisVm.OnAxisValueChanged -= _axisVm_OnAxisValueChanged;
                _axisVm.PropertyChanged -= _axisVm_PropertyChanged;
                _axisVm.SegmentBoundaryChanged -= _axisVm_SegmentBoundaryChanged;
            }

            _axisVm = DataContext as AxisMapViewModel;
            if (_axisVm == null) return;

            SetSegmentBoundaryFilter();

            _axisVm.OnAxisValueChanged += _axisVm_OnAxisValueChanged;
            _axisVm.PropertyChanged += _axisVm_PropertyChanged;
            _axisVm.SegmentBoundaryChanged += _axisVm_SegmentBoundaryChanged;

            OnSegmentsChanged();
        }

        private void _axisVm_SegmentBoundaryChanged(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
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

        private void _axisVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_axisVm.SegmentCount))
            {
                OnSegmentsChanged();
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
            const float ratio = 546.125f;//based on bar width of 120px ie 65535/120 - promote this to public const
            foreach (var keyValue in _axisVm.Segments)
            {
                if (keyValue.Value >= ushort.MaxValue - 655) continue;
                var point = keyValue.Value / ratio;

                var line = new Line();
                line.X1 = point;
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
            if (final < 0) return;
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

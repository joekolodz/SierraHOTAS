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

        public static DependencyProperty AxisHandProperty = DependencyProperty.Register(nameof(AxisHand), typeof(int), typeof(LinearAxisMap), new FrameworkPropertyMetadata(0, OnPropertyChangedChanged));
        public static DependencyProperty IsMultiActionProperty = DependencyProperty.Register(nameof(IsMultiAction), typeof(int), typeof(LinearAxisMap), new FrameworkPropertyMetadata(0, OnPropertyChangedChanged));
        public static DependencyProperty SegmentCountProperty = DependencyProperty.Register(nameof(SegmentCount), typeof(int), typeof(LinearAxisMap), new FrameworkPropertyMetadata(0, OnPropertyChangedChanged));
        public static DependencyProperty SegmentBoundaryProperty = DependencyProperty.Register(nameof(SegmentBoundary), typeof(int), typeof(LinearAxisMap), new FrameworkPropertyMetadata(0, OnPropertyChangedChanged));

        private static void OnPropertyChangedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is LinearAxisMap prop)) return;
            if (e.Property.Name == nameof(AxisHand)) prop.DrawRectangle((int)e.NewValue);
            if (e.Property.Name == nameof(IsMultiAction) || e.Property.Name == nameof(SegmentCount)) prop.OnSegmentsChanged();
            if (e.Property.Name == nameof(SegmentBoundary)) prop.ChangeSegmentBoundary();
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

        private void AxisMap_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _axisVm = DataContext as AxisMapViewModel;
            if (_axisVm == null) return;

            SetSegmentBoundaryFilter();

            OnSegmentsChanged();
        }

        private void ChangeSegmentBoundary()
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

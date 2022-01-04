using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SierraHOTAS.Views
{
    /// <summary>
    /// Interaction logic for InputGraphWindow.xaml
    /// </summary>
    public partial class InputGraphWindow : Window
    {
        private static InputGraphWindow _thisWindow;
        private List<IHOTASDevice> _axisDeviceList;
        private int _graphScale;
        private readonly Action<EventHandler<AxisChangedEventArgs>> _callBackRemoveHandler;
        private DispatcherTimer _dispatcherTimer;
        private int _meterStrokeThickness = 1;
        private Dictionary<int, Tuple<int, Color>> _deviceLastPoint = new Dictionary<int, Tuple<int, Color>>();
        private int _meterPosition = 0;
        private static WriteableBitmap _writeableBitmap;

        public static void CreateWindow(Window parent, IHOTASCollection deviceList, Action<EventHandler<AxisChangedEventArgs>> handler, Action<EventHandler<AxisChangedEventArgs>> callBackRemoveHandler)
        {
            if (_thisWindow != null)
            {
                _thisWindow.Show();
                return;
            }

            _thisWindow = new InputGraphWindow(deviceList, handler, callBackRemoveHandler) {Owner = parent};
            _thisWindow.Show();
        }

        private InputGraphWindow(IHOTASCollection deviceList, Action<EventHandler<AxisChangedEventArgs>> handler, Action<EventHandler<AxisChangedEventArgs>> callBackRemoveHandler)
        {
            InitializeComponent();

            DataContext = this;

            foreach (var d in deviceList.Devices.Where(x=>x.IsDeviceLoaded))
            {
                if (d.Capabilities.AxeCount <= 0) continue;
                if (_axisDeviceList == null) _axisDeviceList = new List<IHOTASDevice>();
                _axisDeviceList.Add(d);
                //todo: grab capabilities from device so we know what axes are available
            }
            //todo: bind the names to a checkbox control and draw a canvas for each device selected?
            //the devices in _axisDeviceList will have the names of the true axis that are actually on the device

            handler(AxisChangedHandler);
            _callBackRemoveHandler = callBackRemoveHandler;

            Loaded += InputGraphWindow_Loaded;
            SizeChanged += InputGraphWindow_OnSizeChanged;

            CalculateScale((int)LineGraphCanvas.Height);

            _dispatcherTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 25), DispatcherPriority.Send, DrawLoop, Dispatcher.CurrentDispatcher);

        }

        private void InputGraphWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _writeableBitmap = new WriteableBitmap(
                (int)ActualWidth,
                (int)ActualHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            RenderOptions.SetBitmapScalingMode(LineGraphCanvasImage, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(LineGraphCanvasImage, EdgeMode.Aliased);

            LineGraphCanvasImage.Source = _writeableBitmap;
            LineGraphCanvasImage.Stretch = Stretch.None;
            LineGraphCanvasImage.HorizontalAlignment = HorizontalAlignment.Left;
            LineGraphCanvasImage.VerticalAlignment = VerticalAlignment.Top;
        }

        void DrawPixel(int column, int row, Color color)
        {
            var c = color.R << 16;
            c |= color.G << 8;
            c |= color.B;

            try
            {
                _writeableBitmap.Lock();

                unsafe
                {
                    var pBackBuffer = _writeableBitmap.BackBuffer;
                    var square = pBackBuffer;

                    square += row * _writeableBitmap.BackBufferStride;
                    square += column * 4;

                    *(int*)square = c;
                    for (var i = 0; i < _meterStrokeThickness - 1; i++)
                    {
                        square += 4;
                        *(int*)square = c;
                    }
                }

                var strokeWidth = _meterStrokeThickness;
                if (column + _meterStrokeThickness > ActualWidth)
                {
                    strokeWidth = (int)ActualWidth - _meterStrokeThickness;
                }

                _writeableBitmap.AddDirtyRect(new Int32Rect(column, row, strokeWidth, 1));
            }
            finally
            {
                _writeableBitmap.Unlock();
            }
        }

        private void DrawMeterLine(int column)
        {
            var height = LineGraphCanvas.ActualHeight;
            height = ActualHeight;
            for (var row = 0; row < height; row++)
            {
                DrawPixel(column, row, Colors.Yellow);
            }
        }

        private void EraseMeterLine(int column)
        {
            var width = _meterStrokeThickness;
            var height = (int)ActualHeight;

            var stride = (width * _writeableBitmap.Format.BitsPerPixel + 7) / 8;
            var bufferSize = height * stride;

            var rect = new Int32Rect(column, 0, width, height);
            var bitmapData = new int[bufferSize]; //initializes to zero which is just black
            _writeableBitmap.WritePixels(rect, bitmapData, stride, column, 0);
        }

        private void DrawLoop(object sender, EventArgs e)
        {
            EraseMeterLine(_meterPosition);

            ++_meterPosition;
            if (_meterPosition > LineGraphCanvas.ActualWidth) _meterPosition = 0;

            DrawMeterLine(_meterPosition);

            var ids = new int[_deviceLastPoint.Keys.Count];
            _deviceLastPoint.Keys.CopyTo(ids, 0);

            for (var i = _deviceLastPoint.Count - 1; i >= 0; i--)
            {
                var key = ids[i];
                var axis = _deviceLastPoint[key];
                Draw1(key, axis.Item1, axis.Item2);
            }
        }

        private void InputGraphWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateScale((int)LineGraphCanvas.ActualHeight);
        }

        private void CalculateScale(int height)
        {
            _graphScale = 65535 / (height); //34 = title bar plus inner window border
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
                        break;
                }
            }

            if (e.Device.Name.ToLower().Contains("throttle"))
            {
                switch (e.AxisId)
                {
                    case 0:
                        c = Colors.Red;
                        break;
                    case 4:
                        c = Colors.Yellow;
                        break;
                    case 8:
                        c = Colors.Cyan;
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
            var x = _meterPosition - 1;
            if (_meterPosition == 0) x = 0;

            DrawPixel(x, value, color);

            if (_deviceLastPoint.Keys.Contains(axisId))
            {
                _deviceLastPoint[axisId] = new Tuple<int, Color>(value, color);
            }
            else
            {
                _deviceLastPoint.Add(axisId, new Tuple<int, Color>(value, color));
            }

        }

        protected override void OnClosed(EventArgs e)
        {
            _dispatcherTimer.Stop();
            _callBackRemoveHandler(AxisChangedHandler);
            _thisWindow = null;
        }
    }
}

using SierraHOTAS.ViewModel;
using SierraHOTAS.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SierraHOTAS.ViewModels;
using Math = System.Math;

//https://www.pinvoke.net/default.aspx/user32/SendInput.html
//https://cboard.cprogramming.com/windows-programming/170043-how-use-sendmessage-wm_keyup.html
//free icon: https://www.axialis.com/free/icons/

namespace SierraHOTAS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool IsDebug { get; set; }

        public HOTASCollectionViewModel HotasCollectionViewModel { get; }

        public MainWindow()
        {
            //IsDebug = true;

            InitializeComponent();

            Logging.Log.Info("Startup");

            HotasCollectionViewModel = new HOTASCollectionViewModel();
            HotasCollectionViewModel.ButtonPressed += CollectionViewModelButtonPressed;
            HotasCollectionViewModel.AxisChanged += CollectionViewModelAxisChanged;
            HotasCollectionViewModel.FileOpened += HotasCollectionViewModel_FileOpened;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void CollectionViewModelAxisChanged(object sender, ViewModels.AxisChangedViewModelEventArgs e)
        {
            //0 TO 65535
            //32767 HALF WAY

            Dispatcher.Invoke(() => DrawRectangle(e.Value));
            Dispatcher.Invoke(() => DrawCircle(e.Value));
        }

        private System.Windows.Shapes.Rectangle _rect;
        private void DrawRectangle(int width)
        {
            float w = width;
            w = w / 327.68f;
            var final = Math.Round(w);

            if (_rect == null)
            {
                _rect = new System.Windows.Shapes.Rectangle()
                {
                    Stroke = new SolidColorBrush(Colors.AliceBlue),
                    Fill = new SolidColorBrush(Colors.DarkRed),
                    Width = 1,
                    Height = 60
                };
                Canvas.SetLeft(_rect, 0);
                Canvas.SetBottom(_rect, 0);
                canvas_x_axis_bar.Children.Add(_rect);
            }

            _rect.Width = final;
        }

        private System.Windows.Shapes.Line _arc;
        private void DrawCircle(int angle)
        {
            const double canvasLeft = 0;
            const double canvasTop = 0;
            const double angleRatio = ushort.MaxValue / 360.0f; //(joystick axis values range from 0 to 65535)

            var radius = canvas_x_axis_circle.Height / 2;
            var ellipseWidth = radius * 2;
            var ellipseHeight = radius * 2;
            var h = canvasLeft + radius;
            var k = canvasTop + radius;

            var theta = angle / angleRatio; //compress joystick value into degrees to get the angle on the circle

            if (_arc == null)
            {
                _arc = new System.Windows.Shapes.Line()
                {
                    StrokeThickness = 4,
                    Stroke = Brushes.Red,
                    X1 = h,
                    Y1 = k
                };

                var circle = new System.Windows.Shapes.Ellipse()
                {
                    Width = ellipseWidth,
                    Height = ellipseHeight,
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = Brushes.AliceBlue
                };
                Canvas.SetLeft(circle, 0);
                Canvas.SetTop(circle, 0);

                canvas_x_axis_circle.Children.Add(_arc);
                canvas_x_axis_circle.Children.Add(circle);
            }

            var radians = (Math.PI / 180) * theta; //turn joystick angle into radians
            var x = h + radius * Math.Sin(radians);
            var y = k - radius * Math.Cos(radians);

            _arc.X2 = x;
            _arc.Y2 = y;

            //Logging.Log.Info($"{canvasLeft}, {canvasTop} / {theta} - {radians} - {x},{y}");
        }

        private void HotasCollectionViewModel_FileOpened(object sender, EventArgs e)
        {
            txtLastFile.Content = FileSystem.LastSavedFileName;
            Logging.Log.Info($"Loaded a device set...");
            DataContext = HotasCollectionViewModel;
            lstDevices.ItemsSource = HotasCollectionViewModel.Devices;
            lstDevices.SelectedIndex = 0;
            foreach (var d in HotasCollectionViewModel.Devices)
            {
                Logging.Log.Info($"{d.InstanceId}, {d.Name}");
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowsProcedure.Initialize(this);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HotasCollectionViewModel.Initialize();
            DataContext = HotasCollectionViewModel;
            Keyboard.Start();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            HotasCollectionViewModel.Dispose();
            Keyboard.Stop();
        }

        private void CollectionViewModelButtonPressed(object sender, ButtonPressedViewModelEventArgs e)
        {
            Dispatcher?.Invoke(() => lstDevices.SelectedItem = e.Device);

            foreach (var map in e.Device.ButtonMap)
            {
                if (map.ButtonId == e.ButtonId)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        gridMap.SelectedItem = map;
                        gridMap.ScrollIntoView(map);
                    });
                    break;
                }
            }
        }

        private void LstDevices_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;
            if (!(e.AddedItems[0] is DeviceViewModel device)) return;

            gridMap.ItemsSource = device.ButtonMap;

            if (HotasCollectionViewModel.SelectionChangedCommand.CanExecute(null))
            {
                HotasCollectionViewModel.SelectionChangedCommand.Execute(device);
            }
        }

        private void ActionList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ComboBox box) || !(box.DataContext is MapViewModel mapContext)) return;
            if (e.AddedItems.Count <= 0) return;
            if (!(e.AddedItems[0] is ActionCatalogItem selectedAction)) return;

            Logging.Log.Info($"map selected to be changed: {mapContext.ButtonName}");

            HotasCollectionViewModel.ActionComboBoxSelectionChangeCommand(mapContext, selectedAction);
        }
    }
}

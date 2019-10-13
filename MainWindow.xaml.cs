using SierraHOTAS.ViewModel;
using SierraHOTAS.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

//https://www.pinvoke.net/default.aspx/user32/SendInput.html
//https://cboard.cprogramming.com/windows-programming/170043-how-use-sendmessage-wm_keyup.html
//free icon: https://www.axialis.com/free/icons/

//TODO:
//check todos!
// when in TEST mode, suppress keyboard handling and force output to the TEST window

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

            HotasCollectionViewModel = new HOTASCollectionViewModel();
            HotasCollectionViewModel.ButtonPressed += CollectionViewModelButtonPressed;
            HotasCollectionViewModel.FileOpened += HotasCollectionViewModel_FileOpened;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void HotasCollectionViewModel_FileOpened(object sender, EventArgs e)
        {
            txtLastFile.Content = FileSystem.LastSavedFileName;
            Debug.WriteLine($"Loaded a device set...");
            DataContext = HotasCollectionViewModel;
            lstDevices.ItemsSource = HotasCollectionViewModel.Devices;
            lstDevices.SelectedIndex = 0;
            foreach (var d in HotasCollectionViewModel.Devices)
            {
                Debug.WriteLine($"{d.InstanceId}, {d.Name}");
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
    }
}

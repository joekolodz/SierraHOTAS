using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SierraHOTAS;
using SierraHOTAS.ViewModel;

//https://www.pinvoke.net/default.aspx/user32/SendInput.html
//https://cboard.cprogramming.com/windows-programming/170043-how-use-sendmessage-wm_keyup.html


namespace SierraHOTAS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public HOTASCollectionViewModel HotasCollectionViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();

            HotasCollectionViewModel = new HOTASCollectionViewModel();
            DataContext = HotasCollectionViewModel;
            HotasCollectionViewModel.ButtonPressed += CollectionViewModelButtonPressed;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HotasCollectionViewModel.Initialize();
            lstDevices.ItemsSource = HotasCollectionViewModel.Devices;
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
                    Dispatcher?.Invoke(() => gridMap.SelectedItem = map);
                    break;
                }
            }
        }

        private async Task TestKeyCombination()
        {
            var flags1 = Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_ALTDOWN;
            var flags2 = Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED;
            var flags3 = flags1 | flags2;

            Keyboard.SendKeyPress(Win32Structures.ScanCodeShort.LMENU, (int)flags3);
        }

        private void LstDevices_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.AddedItems[0] is DeviceViewModel device)) return;
            gridMap.ItemsSource = device.ButtonMap;

            if (HotasCollectionViewModel.SelectionChangedCommand.CanExecute(null))
            {
                HotasCollectionViewModel.SelectionChangedCommand.Execute(e.AddedItems);
            }
        }

    }
}

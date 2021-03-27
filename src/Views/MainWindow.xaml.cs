using Hardcodet.Wpf.TaskbarNotification;
using SierraHOTAS.Models;
using SierraHOTAS.ModeProfileWindow.ViewModels;
using SierraHOTAS.ViewModels;
using SierraHOTAS.Win32;
using System;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

//https://www.pinvoke.net/default.aspx/user32/SendInput.html
//https://cboard.cprogramming.com/windows-programming/170043-how-use-sendmessage-wm_keyup.html
//free icon: https://www.axialis.com/free/icons/

namespace SierraHOTAS.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string WQL_EVENT_QUERY = "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2";

        private readonly TaskbarIcon _taskbarIcon;

        public HOTASCollectionViewModel HotasCollectionViewModel { get; }



        //inject HOTASCollectionViewModel to this consctructor
        //the ctor for HOTASCollectionViewModel should take parameters for HOTASCollection, ActionCatalogViewModel, and whatever else
        //figure out how to inject for all the ViewModels
        //register the VMs in the container in the App Onstart

        public MainWindow(HOTASCollectionViewModel hotasCollectionViewModel)
        {
            InitializeComponent();

            Logging.Log.Info("Starting up");

            _taskbarIcon = (TaskbarIcon)FindResource("NotifyIcon");
            _taskbarIcon.DataContext = hotasCollectionViewModel.QuickProfilePanelViewModel;

            HotasCollectionViewModel = hotasCollectionViewModel;
            HotasCollectionViewModel.ButtonPressed += CollectionViewModelButtonPressed;
            HotasCollectionViewModel.AxisChanged += CollectionViewModelAxisChanged;
            HotasCollectionViewModel.FileOpened += HotasCollectionViewModel_FileOpened;
            HotasCollectionViewModel.ModeProfileChanged += HotasCollectionViewModel_ModeProfileChanged;
            HotasCollectionViewModel.ShowMainWindow += HotasCollectionViewModel_ShowMainWindow;
            HotasCollectionViewModel.Close += HotasCollectionViewModel_Close;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
            StateChanged += MainWindow_StateChanged;
            KeyDown += Window_KeyDown;
        }

        private void HotasCollectionViewModel_Close(object sender, EventArgs e)
        {
            Close();
        }

        private void HotasCollectionViewModel_ShowMainWindow(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Focus();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Visibility = Visibility.Hidden;
            }
        }

        private void HotasCollectionViewModel_ModeProfileChanged(object sender, ModeProfileChangedEventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                var selectedProfile = HotasCollectionViewModel.ModeActivationItems.FirstOrDefault<ModeActivationItem>(x => x.Mode == e.Mode);
                if (selectedProfile != null)
                {
                    ModeActivationGrid.SelectedItem = selectedProfile;
                }
            });
        }

        private DeviceViewModel _currentlySelectedDeviceVm;
        private void CollectionViewModelAxisChanged(object sender, AxisChangedViewModelEventArgs e)
        {
            if (e.Device == null) return;
            if (_currentlySelectedDeviceVm != e.Device) return;

            foreach (var map in e.Device.ButtonMap)
            {
                if (map.ButtonId == e.AxisId)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        if (map.Type == HOTASButtonMap.ButtonType.Button ||
                            map.Type == HOTASButtonMap.ButtonType.POV)
                        {
                            gridMap.SelectedItem = map;
                            gridMap.ScrollIntoView(map);
                        }
                        else
                        {
                            Dispatcher?.Invoke(() => e.Device.SetAxis(map.ButtonId, e.Value));
                        }
                    });
                    break;
                }
            }
        }

        private void HotasCollectionViewModel_FileOpened(object sender, EventArgs e)
        {
            DataContext = HotasCollectionViewModel;
            lstDevices.ItemsSource = HotasCollectionViewModel.Devices;
            lstDevices.SelectedIndex = 0;
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

            SetupUsbDeviceWatcher();

            Keyboard.Start();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _watcher.EventArrived -= Watcher_EventArrived;
            _watcher.Stop();
            _watcher.Dispose();
            HotasCollectionViewModel.Dispose();
            Keyboard.Stop();
            _taskbarIcon.Dispose();
        }

        private ManagementEventWatcher _watcher;
        private void SetupUsbDeviceWatcher()
        {
            _watcher = new ManagementEventWatcher();

            var query = new WqlEventQuery(WQL_EVENT_QUERY);
            _watcher.EventArrived += Watcher_EventArrived;
            _watcher.Query = query;
            _watcher.Start();
        }

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            //TODO remove log
            Logging.Log.Info($"Watcher_EventArrived {e}");
            Dispatcher.Invoke(() => HotasCollectionViewModel.RefreshDeviceListCommand.Execute(null));
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        private new static bool IsActive(Window wnd)
        {
            // workaround for minimization bug
            // Managed .IsActive may return wrong value
            if (wnd == null) return false;
            return GetForegroundWindow() == new WindowInteropHelper(wnd).Handle;
        }

        public static bool IsApplicationActive()
        {
            var wnd = Application.Current.MainWindow;
            return IsActive(wnd);
        }

        private void CollectionViewModelButtonPressed(object sender, ButtonPressedViewModelEventArgs e)
        {
            if (e.Device == null) return;

            Dispatcher?.Invoke(() =>
            {
                if (!txtTestBox.IsFocused)
                {
                    var isActive = IsApplicationActive();
                    if (isActive)
                    {
                        txtTestBox.Clear();
                        txtTestBox.Focus();
                    }
                }
                lstDevices.SelectedItem = e.Device;
            });

            var map = e.Device.ButtonMap.FirstOrDefault(m => m.ButtonId == e.ButtonId);
            if (map == null) return;

            Dispatcher?.Invoke(() =>
            {
                if (HotasCollectionViewModel.SnapToButton.GetValueOrDefault())
                {
                    gridMap.SelectedItem = map;
                    gridMap.ScrollIntoView(map);
                }
            });
        }

        private void LstDevices_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;
            if (!(e.AddedItems[0] is DeviceViewModel device)) return;

            SelectDevice(device);
        }

        private void SelectDevice(DeviceViewModel device)
        {
            _currentlySelectedDeviceVm = device;

            gridMap.ItemsSource = device.ButtonMap;

            if (HotasCollectionViewModel.SelectionChangedCommand.CanExecute(null))
            {
                HotasCollectionViewModel.SelectionChangedCommand.Execute(device);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //if there are no devices connected to trigger button events, use debug mode on and send a DOWN event for Button0 with left shift and button UP event for Button0 with left control
            if (!App.IsDebug) return;
            if (e.Key == Key.LeftShift)
            {
                _currentlySelectedDeviceVm.ForceButtonPress(SharpDX.DirectInput.JoystickOffset.Buttons0, true);
            }
            if (e.Key == Key.LeftCtrl)
            {
                _currentlySelectedDeviceVm.ForceButtonPress(SharpDX.DirectInput.JoystickOffset.Buttons0, false);
            }
        }

        private void ModeActivationGrid_Selected(object sender, RoutedEventArgs e)
        {
            if (!(ModeActivationGrid.CurrentItem is ModeActivationItem item)) return;

            foreach (var device in HotasCollectionViewModel.Devices)
            {
                if (device.InstanceId != item.DeviceId) continue;
                SelectDevice(device);
                HotasCollectionViewModel.SetMode(item.Mode);
                break;
            }
        }
    }
}

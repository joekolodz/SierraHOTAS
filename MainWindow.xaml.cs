﻿using SierraHOTAS.ViewModel;
using SierraHOTAS.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using SharpDX.DirectInput;
using SierraHOTAS.Models;
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

        private void CollectionViewModelAxisChanged(object sender, AxisChangedViewModelEventArgs e)
        {
            if (e.AxisId == (int)JoystickOffset.RotationX) Dispatcher?.Invoke(() => RadialAxisMapRX.SetAxis(e.Value));
            if (e.AxisId == (int)JoystickOffset.RotationY) Dispatcher?.Invoke(() => RadialAxisMapRY.SetAxis(e.Value));
            if (e.AxisId == (int)JoystickOffset.RotationZ) Dispatcher?.Invoke(() => RadialAxisMapRZ.SetAxis(e.Value));

            if (e.AxisId == (int)JoystickOffset.X) Dispatcher?.Invoke(() => LinearAxisMapX.SetAxis(e.Value));
            if (e.AxisId == (int)JoystickOffset.Y) Dispatcher?.Invoke(() => LinearAxisMapY.SetAxis(e.Value));
            if (e.AxisId == (int)JoystickOffset.Z) Dispatcher?.Invoke(() => LinearAxisMapZ.SetAxis(e.Value));
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




        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        private static bool IsActive(Window wnd)
        {
            // workaround for minimization bug
            // Managed .IsActive may return wrong value
            if (wnd == null) return false;
            return GetForegroundWindow() == new WindowInteropHelper(wnd).Handle;
        }

        public static bool IsApplicationActive()
        {
            var wnd = Application.Current.MainWindow;
            if (IsActive(wnd)) return true;
            return false;
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

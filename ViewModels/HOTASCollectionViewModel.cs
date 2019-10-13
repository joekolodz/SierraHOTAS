using SierraHOTAS.Models;
using SierraHOTAS.ViewModel.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SierraHOTAS.Annotations;

namespace SierraHOTAS.ViewModel
{
    public class HOTASCollectionViewModel : IDisposable, INotifyPropertyChanged
    {
        public ObservableCollection<DeviceViewModel> Devices { get; set; }

        public string LastFileSaved => FileSystem.LastSavedFileName;

        public event EventHandler<ButtonPressedViewModelEventArgs> ButtonPressed;

        public event EventHandler<EventArgs> FileOpened;

        private HOTASCollection _deviceList;

        public DeviceViewModel SelectedDevice { get; set; }

        private ICommand _fileSaveCommand;

        public ICommand SaveFileCommand => _fileSaveCommand ?? (_fileSaveCommand = new CommandHandler(FileSave, () => CanExecute));

        private ICommand _fileSaveAsCommand;

        public ICommand SaveFileAsCommand => _fileSaveAsCommand ?? (_fileSaveAsCommand = new CommandHandler(FileSaveAs, () => CanExecute));

        private ICommand _fileOpenCommand;

        public ICommand OpenFileCommand => _fileOpenCommand ?? (_fileOpenCommand = new CommandHandler(FileOpen, () => CanExecute));

        private ICommand _selectionChangedCommand;

        public ICommand SelectionChangedCommand => _selectionChangedCommand ?? (_selectionChangedCommand = new RelayCommandWithParameter(lstActionList_OnSelectionChanged));

        private ICommand _exitApplicationCommand;

        public ICommand ExitApplicationCommand => _exitApplicationCommand ?? (_exitApplicationCommand = new CommandHandler(ExitApplication, () => CanExecute));

        public bool CanExecute => true;

        public HOTASCollectionViewModel()
        {
            _deviceList = new HOTASCollection();
        }

        public void Initialize()
        {
            if (MainWindow.IsDebug)
            {
                _deviceList.Devices = DataProvider.GetDeviceList();
            }
            else
            {
                _deviceList.ButtonPressed += DeviceList_ButtonPressed;
                _deviceList.Start();
            }

            BuildDevicesViewModel();
        }

        public void Dispose()
        {
            _deviceList.Stop();
        }

        private void BuildDevicesViewModelFromLoadedDevices(HOTASCollection loadedDevices)
        {

            foreach (var ld in loadedDevices.Devices)
            {
                DeviceViewModel deviceVm = null;
                foreach (var vm in Devices)
                {
                    if (vm.InstanceId != ld.InstanceId) continue;
                    deviceVm = vm;
                    break;
                }

                if (deviceVm == null)
                {
                    Debug.WriteLine($"Loaded mappings for {ld.Name}, but could not find the device attached!");
                    Debug.WriteLine($"Mappings will be displayed, but they will not function");
                    Devices.Add(new DeviceViewModel(ld));
                    continue;
                }
                else
                {
                    Devices.Remove(deviceVm);
                }
                //deviceVm = new DeviceViewModel(ld);

                //Devices.Add(deviceVm);

                var d = _deviceList.GetDevice(ld.InstanceId);
                if (d == null) continue;
                d.ButtonMap = ld.ButtonMap.ToObservableCollection();
                deviceVm.RebuildMap();
            }
        }

        private void BuildDevicesViewModel()
        {
            Devices = _deviceList.Devices.Select(device => new DeviceViewModel(device)).ToObservableCollection();
        }

        private void DeviceList_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            var device = Devices.First(d => d.InstanceId == e.Device.InstanceId);
            ButtonPressed?.Invoke(sender, new ButtonPressedViewModelEventArgs() { ButtonId = e.ButtonId, Device = device });
        }

        private static void ExitApplication()
        {
            Application.Current.Shutdown(0);
        }

        private void FileSave()
        {
            FileSystem.FileSave(_deviceList);
        }

        private void FileSaveAs()
        {
            FileSystem.FileSaveAs(_deviceList);
        }

        private void FileOpen()
        {
            _deviceList.Stop();
            var loadedDeviceList = FileSystem.FileOpen();
            BuildDevicesViewModelFromLoadedDevices(loadedDeviceList);
            FileOpened?.Invoke(this, new EventArgs());
            _deviceList.ListenToAllDevices();
        }

        public void lstActionList_OnSelectionChanged(object device)
        {
            SelectedDevice = device as DeviceViewModel;
            if (SelectedDevice != null) Debug.WriteLine($"Device Selected:{SelectedDevice.Name}");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModel.Commands;
using SierraHOTAS.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace SierraHOTAS.ViewModel
{
    public class HOTASCollectionViewModel : IDisposable, INotifyPropertyChanged
    {
        public bool IsDirectional { get; set; }
        public ObservableCollection<DeviceViewModel> Devices { get; set; }
        public ActionCatalogViewModel ActionCatalog { get; set; }

        public string LastFileSaved => FileSystem.LastSavedFileName;

        public event EventHandler<ButtonPressedViewModelEventArgs> ButtonPressed;
        public event EventHandler<AxisChangedViewModelEventArgs> AxisChanged;

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

        public ICommand SelectionChangedCommand => _selectionChangedCommand ?? (_selectionChangedCommand = new RelayCommandWithParameter(lstDevices_OnSelectionChanged));

        private ICommand _exitApplicationCommand;

        public ICommand ExitApplicationCommand => _exitApplicationCommand ?? (_exitApplicationCommand = new CommandHandler(ExitApplication, () => CanExecute));

        public bool CanExecute => true;

        public HOTASCollectionViewModel()
        {
            _deviceList = new HOTASCollection();
            ActionCatalog = new ActionCatalogViewModel();
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
                _deviceList.AxisChanged += DeviceList_AxisChanged;
                _deviceList.Start();
            }

            BuildDevicesViewModel();
            AddHandlers();
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
                    Logging.Log.Warn($"Loaded mappings for {ld.Name}, but could not find the device attached!");
                    Logging.Log.Warn($"Mappings will be displayed, but they will not function");
                    Devices.Add(new DeviceViewModel(ld));
                    continue;
                }

                var d = _deviceList.GetDevice(ld.InstanceId);
                if (d == null) continue;
                d.ButtonMap = ld.ButtonMap.ToObservableCollection();
                deviceVm.RebuildMap();
            }
        }

        private void BuildDevicesViewModel()
        {
            RemoveAllHandlers();
            Devices = _deviceList.Devices.Select(device => new DeviceViewModel(device)).ToObservableCollection();
        }

        private void DeviceList_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            var device = Devices?.First(d => d.InstanceId == e.Device.InstanceId);
            ButtonPressed?.Invoke(sender, new ButtonPressedViewModelEventArgs() { ButtonId = e.ButtonId, Device = device });
        }

        private void DeviceList_AxisChanged(object sender, AxisChangedEventArgs e)
        {
            if (Devices == null) return;
            var device = Devices.First(d => d.InstanceId == e.Device.InstanceId);
            AxisChanged?.Invoke(sender, new AxisChangedViewModelEventArgs() { AxisId = e.AxisId, Value = e.Value, Device = device });
        }

        private static void ExitApplication()
        {
            Application.Current.Shutdown(0);
        }

        private void FileSave()
        {
            foreach (var d in _deviceList.Devices)
            {
                foreach (var m in d.ButtonMap)
                {
                    if (m.ActionName == "<No Action>")
                    {
                        m.ActionName = string.Empty;
                        m.Actions.Clear();
                    }
                }
            }
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
            if (loadedDeviceList == null) return;

            BuildDevicesViewModelFromLoadedDevices(loadedDeviceList);
            AddHandlers();
            BuildActionCatalogFromLoadedDevices();

            FileOpened?.Invoke(this, new EventArgs());
            _deviceList.ListenToAllDevices();
        }

        private void BuildActionCatalogFromLoadedDevices()
        {
            ActionCatalog.Clear();

            foreach (var device in Devices)
            {
                foreach (var map in device.ButtonMap)
                {
                    if (string.IsNullOrWhiteSpace(map.ActionName)) continue;
                    if (ActionCatalog.Contains(map.ActionName)) continue;

                    var item = new ActionCatalogItem()
                    {
                        ActionName = map.ActionName,
                        Actions = map.GetHotasActions()
                    };
                    ActionCatalog.Add(item);
                }
            }
        }

        public void lstDevices_OnSelectionChanged(object device)
        {
            SelectedDevice = device as DeviceViewModel;
            if (SelectedDevice != null) Debug.WriteLine($"Device Selected:{SelectedDevice.Name}");
        }

        private void AddHandlers()
        {
            foreach (var deviceVm in Devices)
            {
                deviceVm.RecordingStopped += Device_RecordingStopped;
            }
        }

        private void Device_RecordingStopped(object sender, EventArgs e)
        {
            if (!(sender is MapViewModel mapVm)) return;
            ActionCatalog.Add(mapVm);
        }

        public void ActionComboBoxSelectionChangeCommand(MapViewModel mapContext, ActionCatalogItem selectedAction)
        {
            mapContext.AssignActions(selectedAction);
        }

        private void RemoveAllHandlers()
        {
            if (Devices == null) return;
            foreach (var deviceVm in Devices)
            {
                deviceVm.RecordingStopped -= Device_RecordingStopped;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

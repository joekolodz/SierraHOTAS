using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using SierraHOTAS.ModeProfileWindow;
using SierraHOTAS.ModeProfileWindow.ViewModels;
using SierraHOTAS.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace SierraHOTAS.ViewModels
{
    public class HOTASCollectionViewModel : IDisposable, INotifyPropertyChanged
    {
        public Dispatcher AppDispatcher { get; set; }
        public ObservableCollection<DeviceViewModel> Devices { get; set; }
        public ActionCatalogViewModel ActionCatalog { get; set; }
        public ObservableCollection<ActivityItem> Activity { get; set; }
        public ObservableCollection<ModeActivationItem> ModeActivationItems => _deviceList.ModeProfileActivationButtons.Values.ToObservableCollection();

        private bool? _snapToButton = true;

        public bool? SnapToButton
        {
            get => _snapToButton;
            set
            {
                if (value == _snapToButton) return;
                _snapToButton = value ?? false;
                OnPropertyChanged();
            }
        }

        private string _profileSetFileName;

        public string ProfileSetFileName
        {
            get => _profileSetFileName;
            set
            {
                if (value == _profileSetFileName) return;
                _profileSetFileName = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler<ButtonPressedViewModelEventArgs> ButtonPressed;
        public event EventHandler<AxisChangedViewModelEventArgs> AxisChanged;
        public event EventHandler<ModeProfileChangedEventArgs> ModeProfileChanged;
        public event EventHandler<EventArgs> FileOpened;

        private HOTASCollection _deviceList;

        public DeviceViewModel SelectedDevice { get; set; }

        private ICommand _fileSaveCommand;

        public ICommand SaveFileCommand => _fileSaveCommand ?? (_fileSaveCommand = new CommandHandler(FileSave));

        private ICommand _fileSaveAsCommand;

        public ICommand SaveFileAsCommand => _fileSaveAsCommand ?? (_fileSaveAsCommand = new CommandHandler(FileSaveAs));

        private ICommand _fileOpenCommand;

        public ICommand OpenFileCommand => _fileOpenCommand ?? (_fileOpenCommand = new CommandHandler(FileOpenDialog));

        private ICommand _selectionChangedCommand;

        public ICommand SelectionChangedCommand => _selectionChangedCommand ?? (_selectionChangedCommand = new CommandHandlerWithParameter<DeviceViewModel>(lstDevices_OnSelectionChanged));

        private ICommand _clearActiveProfileSetCommand;

        public ICommand ClearActiveProfileSetCommand => _clearActiveProfileSetCommand ?? (_clearActiveProfileSetCommand = new CommandHandler(ClearActiveProfileSet));

        private ICommand _refreshDeviceListCommand;

        public ICommand RefreshDeviceListCommand => _refreshDeviceListCommand ?? (_refreshDeviceListCommand = new CommandHandler(RefreshDeviceList));

        private ICommand _clearActivityListCommand;

        public ICommand ClearActivityListCommand => _clearActivityListCommand ?? (_clearActivityListCommand = new CommandHandler(ClearActivityList));

        private ICommand _createNewModeProfileCommand;

        public ICommand CreateNewModeProfileCommand => _createNewModeProfileCommand ?? (_createNewModeProfileCommand = new CommandHandler(CreateNewModeProfile));

        private ICommand _editModeProfileCommand;

        public ICommand EditModeProfileCommand => _editModeProfileCommand ?? (_editModeProfileCommand = new CommandHandlerWithParameter<ModeActivationItem>(EditModeProfile));

        private ICommand _deleteModeProfileCommand;

        public ICommand DeleteModeProfileCommand => _deleteModeProfileCommand ?? (_deleteModeProfileCommand = new CommandHandlerWithParameter<ModeActivationItem>(DeleteModeProfile));

        private void CreateNewModeProfile()
        {
            const int defaultMode = 1;

            if (_deviceList.ModeProfileActivationButtons.Count == 0)
            {
                var modeMessageWindow = new ModeProfileMessageWindow
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                modeMessageWindow.ShowDialog();
                AssignActivationButton(defaultMode);
            }

            var mode = _deviceList.SetupNewModeProfile();
            _deviceList.SetMode(mode);
            AssignActivationButton(mode);
            OnModeProfileChanged(this, new ModeProfileChangedEventArgs() { Mode = _deviceList.Mode });
        }

        private void EditModeProfile(ModeActivationItem item)
        {
            Logging.Log.Debug("EDIT!");
            var exists = _deviceList.ModeProfileActivationButtons.ContainsKey(item.Mode);
            OnPropertyChanged(nameof(ModeActivationItems));
        }

        private void DeleteModeProfile(ModeActivationItem item)
        {
            if (_deviceList.RemoveModeProfile(item))
            {
                OnPropertyChanged(nameof(ModeActivationItems));
            }
        }

        public void SetMode(int mode)
        {
            _deviceList.SetMode(mode);
            OnModeProfileChanged(this, new ModeProfileChangedEventArgs() { Mode = _deviceList.Mode });
        }

        private void AssignActivationButton(int mode)
        {
            var modeWindow = new ModeProfileConfigWindow(mode, _deviceList.ModeProfileActivationButtons);
            modeWindow.Owner = Application.Current.MainWindow;
            modeWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _deviceList.ButtonPressed += modeWindow.ModeProfileConfigViewModel.DeviceList_ButtonPressed;
            modeWindow.ShowDialog();
            _deviceList.ButtonPressed -= modeWindow.ModeProfileConfigViewModel.DeviceList_ButtonPressed;

            Logging.Log.Info($"Profile name: {modeWindow.ProfileName}, Device: {modeWindow.ModeProfileConfigViewModel.DeviceName}, Button: {modeWindow.ModeProfileConfigViewModel.ActivationButtonId}");

            _deviceList.ApplyActivationButtonToAllProfiles();
            OnPropertyChanged(nameof(ModeActivationItems));
        }

        public HOTASCollectionViewModel()
        {
            _deviceList = new HOTASCollection();
            ActionCatalog = new ActionCatalogViewModel();
            Activity = new ObservableCollection<ActivityItem>();
            EventAggregator.Subscribe<QuickProfileSelectedEvent>(QuickLoadProfile);
        }

        private void QuickLoadProfile(QuickProfileSelectedEvent profileInfo)
        {
            var hotas = FileSystem.FileOpen(profileInfo.Path);
            LoadHotas(hotas);
        }

        public void Initialize()
        {
            if (MainWindow.IsDebug)
            {
                _deviceList.Devices = DataProvider.GetDeviceList();
            }
            else
            {
                _deviceList.AxisChanged += DeviceList_AxisChanged;
            }

            _deviceList.ButtonPressed += DeviceList_ButtonPressed;
            _deviceList.KeystrokeDownSent += DeviceList_KeystrokeDownSent;
            _deviceList.KeystrokeUpSent += DeviceList_KeystrokeUpSent;
            _deviceList.ModeProfileChanged += DeviceList_ModeProfileChanged;

            _deviceList.Start();

            BuildDevicesViewModel();
            AddHandlers();
        }

        private void DeviceList_ModeProfileChanged(object sender, ModeProfileChangedEventArgs e)
        {
            OnModeProfileChanged(sender, e);
        }

        private void OnModeProfileChanged(object sender, ModeProfileChangedEventArgs e)
        {
            AppDispatcher.Invoke(RebuildAllButtonMaps); //crossing thread boundaries from HOTASQueue thread to UI thread
            ModeProfileChanged?.Invoke(sender, e);
        }

        private void DeviceList_KeystrokeUpSent(object sender, KeystrokeSentEventArgs e)
        {
            AddActivity(sender as HOTASQueue, e);
        }

        private void DeviceList_KeystrokeDownSent(object sender, KeystrokeSentEventArgs e)
        {
            AddActivity(sender as HOTASQueue, e);
        }

        private void AddActivity(HOTASQueue queue, KeystrokeSentEventArgs e)
        {
            var map = queue.GetMap(e.Offset);
            string actionName;

            if (map.Type == HOTASButtonMap.ButtonType.Button || map.Type == HOTASButtonMap.ButtonType.POV)
            {
                if (!(map is HOTASButtonMap buttonMap)) return;
                actionName = buttonMap.ActionName;
            }
            else
            {
                if (!(map is HOTASAxisMap axisMap)) return;

                //only virtual axis buttons will have a map id > 0, otherwise map id is equal to offset
                if (e.MapId == 0) e.MapId = e.Offset;

                if (axisMap.Direction == AxisDirection.Forward)
                {
                    actionName = axisMap.ButtonMap.FirstOrDefault(m => m.MapId == e.MapId)?.ActionName;
                }
                else
                {
                    actionName = axisMap.ReverseButtonMap.FirstOrDefault(m => m.MapId == e.MapId)?.ActionName;
                }
            }

            var activity = new ActivityItem() { Offset = e.Offset, ButtonName = map.MapName, ScanCode = e.Code, Flags = e.Flags, ActionName = actionName, Time = DateTime.Now };

            AppDispatcher?.Invoke(() =>
            {
                Activity.Insert(0, activity);
            });
        }

        public void Dispose()
        {
            _deviceList.Stop();
        }

        private void RebuildAllButtonMaps()
        {
            foreach (var deviceVm in Devices)
            {
                var d = _deviceList.GetDevice(deviceVm.InstanceId);
                if (d == null) continue;
                deviceVm.RebuildMap();
            }
        }

        private void BuildDevicesViewModelFromLoadedDevices(HOTASCollection loadedDevices)
        {
            foreach (var ld in loadedDevices.Devices)
            {
                var deviceVm = Devices.FirstOrDefault(vm => vm.InstanceId == ld.DeviceId);

                if (deviceVm == null)
                {
                    Logging.Log.Warn($"Loaded mappings for {ld.Name}, but could not find the device attached!");
                    Logging.Log.Warn($"Mappings will be displayed, but they will not function");
                    Devices.Add(new DeviceViewModel(ld));
                    continue;
                }

                var d = _deviceList.GetDevice(ld.DeviceId);
                if (d == null) continue;

                d.SetModeProfile(ld.ModeProfiles);

                deviceVm.RebuildMap(d.ButtonMap);
            }
        }

        private void BuildDevicesViewModel()
        {
            RemoveAllHandlers();
            Devices = _deviceList.Devices.Select(device => new DeviceViewModel(device)).ToObservableCollection();
        }

        private void DeviceList_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            var device = Devices?.First(d => d.InstanceId == e.Device.DeviceId);
            ButtonPressed?.Invoke(sender, new ButtonPressedViewModelEventArgs() { ButtonId = e.ButtonId, Device = device });
        }

        private void DeviceList_AxisChanged(object sender, AxisChangedEventArgs e)
        {
            if (Devices == null) return;
            var device = Devices.First(d => d.InstanceId == e.Device.DeviceId);
            AxisChanged?.Invoke(sender, new AxisChangedViewModelEventArgs() { AxisId = e.AxisId, Value = e.Value, Device = device });
        }

        private void RefreshDeviceList()
        {
            var newDevices = _deviceList.RescanDevices();

            //if the device has a mapping already loaded, then assign this device to that mapping
            foreach (var deviceViewModel in Devices)
            {
                var newDevice = newDevices.FirstOrDefault(n => n.DeviceId == deviceViewModel.InstanceId);
                if (newDevice == null) continue;

                newDevices.Remove(newDevice);
                deviceViewModel.ReInitializeDevice(newDevice);
                _deviceList.Devices.Add(newDevice);
                _deviceList.ListenToDevice(newDevice);
            }

            //remaining devices here do not have a mapping loaded, so assign a default mapping
            foreach (var n in newDevices)
            {
                var vm = new DeviceViewModel(n);
                Devices.Add(vm);
                _deviceList.Devices.Add(n);
                _deviceList.ListenToDevice(n);
            }
        }

        private void ClearActivityList()
        {
            Activity.Clear();
        }

        private void ClearActiveProfileSet()
        {
            _deviceList.ClearButtonMap();

            foreach (var deviceVm in Devices)
            {
                deviceVm.ClearButtonMap();
                deviceVm.RebuildMap();
            }

            _deviceList.ModeProfileActivationButtons.Clear();
            OnPropertyChanged(nameof(ModeActivationItems));

            FileSystem.LastSavedFileName = "";
            ProfileSetFileName = FileSystem.LastSavedFileName;
        }

        private void FileSave()
        {
            _deviceList.ClearUnassignedActions();
            FileSystem.FileSave(_deviceList);
            ProfileSetFileName = FileSystem.LastSavedFileName;
        }

        private void FileSaveAs()
        {
            FileSystem.FileSaveAs(_deviceList);
            ProfileSetFileName = FileSystem.LastSavedFileName;
        }

        private void FileOpenDialog()
        {
            var loadedDeviceList = FileSystem.FileOpenDialog();
            if (loadedDeviceList == null) return;
            LoadHotas(loadedDeviceList);
        }

        private void LoadHotas(HOTASCollection loadedDeviceList)
        {
            _deviceList.Stop();

            BuildDevicesViewModelFromLoadedDevices(loadedDeviceList);
            BuildModeProfileActivationListFromLoadedDevices(loadedDeviceList);
            ReBuildActionCatalog();
            AddHandlers();
            ProfileSetFileName = FileSystem.LastSavedFileName;

            _deviceList.AutoSetMode();
            _deviceList.ListenToAllDevices();
            FileOpened?.Invoke(this, new EventArgs());

            Logging.Log.Info($"Loaded a device set...");
            foreach (var d in Devices)
            {
                Logging.Log.Info($"{d.InstanceId}, {d.Name}");
            }
        }

        private void BuildModeProfileActivationListFromLoadedDevices(HOTASCollection loadedDevices)
        {
            foreach (var item in loadedDevices.ModeProfileActivationButtons)
            {
                _deviceList.ModeProfileActivationButtons.Add(item.Key, item.Value);
            }
            OnPropertyChanged(nameof(ModeActivationItems));
        }

        private void ReBuildActionCatalog()
        {
            ActionCatalog.Clear();

            foreach (var device in Devices)
            {
                foreach (var mode in device.ModeProfiles)
                {
                    foreach (var m in mode.Value)
                    {
                        if (string.IsNullOrWhiteSpace(m.MapName)) continue;
                        switch (m)
                        {
                            case HOTASAxisMap axisMap:
                                {
                                    AddButtonListToCatalog(axisMap.ButtonMap);
                                    AddButtonListToCatalog(axisMap.ReverseButtonMap);
                                    break;
                                }
                            case HOTASButtonMap buttonMap:
                                {
                                    AddButtonToCatalog(buttonMap.ActionName, buttonMap.ActionCatalogItem.Actions, buttonMap.MapName);
                                    break;
                                }
                        }
                    }
                }
            }
        }

        private void AddButtonListToCatalog(ObservableCollection<HOTASButtonMap> mapList)
        {
            foreach (var baseMap in mapList)
            {
                AddButtonToCatalog(baseMap.ActionName, baseMap.ActionCatalogItem.Actions, baseMap.MapName);
            }
        }

        private void AddButtonToCatalog(string actionName, ObservableCollection<ButtonAction> actions, string buttonName)
        {
            var item = new ActionCatalogItem()
            {
                ActionName = actionName,
                Actions = actions
            };
            if (item.Actions.Count <= 0) return;
            if (string.IsNullOrWhiteSpace(item.ActionName)) item.ActionName = $"<Un-named> - {buttonName}";
            ActionCatalog.Add(item);
        }

        public void lstDevices_OnSelectionChanged(DeviceViewModel device)
        {
            SelectedDevice = device;
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
            if (!(sender is ButtonMapViewModel mapVm)) return;
            ActionCatalog.Add(mapVm);
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

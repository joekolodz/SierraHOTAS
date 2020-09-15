using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using SierraHOTAS.ModeProfileWindow;
using SierraHOTAS.ModeProfileWindow.ViewModels;
using SierraHOTAS.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private readonly Dispatcher _appDispatcher;
        private readonly IFileSystem _fileSystem;
        private bool? _snapToButton = true;

        public QuickProfilePanelViewModel QuickProfilePanelViewModel { get; set; }
        public ActionCatalogViewModel ActionCatalog { get; set; }
        public ObservableCollection<ActivityItem> Activity { get; set; }
        public ObservableCollection<DeviceViewModel> Devices { get; set; }
        public ObservableCollection<ModeActivationItem> ModeActivationItems => _deviceList.ModeProfileActivationButtons.Values.ToObservableCollection();


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

        private IHOTASCollection _deviceList;

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

        public HOTASCollectionViewModel(Dispatcher dispatcher, IFileSystem fileSystem, IHOTASCollection hotasCollection, ActionCatalogViewModel actionCatalogViewModel)
        {
            _fileSystem = fileSystem;
            _appDispatcher = dispatcher;
            _deviceList = hotasCollection;
            ActionCatalog = actionCatalogViewModel;
            Activity = new ObservableCollection<ActivityItem>();
            QuickProfilePanelViewModel = new QuickProfilePanelViewModel(fileSystem);
            EventAggregator.Subscribe<QuickProfileSelectedEvent>(QuickLoadProfile);
        }

        private void CreateNewModeProfile()
        {
            const int defaultMode = 1;

            if (_deviceList.ModeProfileActivationButtons.Count == 0)
            {
                var message = "Before creating a new profile, you must first assign an activation button to the existing profile.";
                var modeMessageWindow = new ModeProfileMessageWindow(message)
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

        private void QuickLoadProfile(QuickProfileSelectedEvent profileInfo)
        {
            if (string.IsNullOrWhiteSpace(profileInfo.Path)) return;

            var hotas = _fileSystem.FileOpen(profileInfo.Path);
            if (hotas == null)
            {
                ProfileSetFileName = $"Could not load {profileInfo.Path}!!! Is this a SierraHOTAS compatible JSON file?";
                return;
            }

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
            _deviceList.ModeProfileChanged += OnModeProfileChanged;
            _deviceList.LostConnectionToDevice += DeviceList_LostConnectionToDevice;

            _deviceList.Start();

            BuildDevicesViewModel();
            AddHandlers();
        }

        private void DeviceList_LostConnectionToDevice(object sender, LostConnectionToDeviceEventArgs e)
        {
            var deviceVm = Devices.FirstOrDefault(h => h.InstanceId == e.HOTASDevice.DeviceId);
            if (deviceVm == null) return;
            //_appDispatcher.Invoke(() => Devices.Remove(deviceVm));
        }

        private void OnModeProfileChanged(object sender, ModeProfileChangedEventArgs e)
        {
            _appDispatcher.Invoke(RebuildAllButtonMaps); //crossing thread boundaries from HOTASQueue thread to UI thread
            ModeProfileChanged?.Invoke(sender, e);
        }

        private void DeviceList_KeystrokeUpSent(object sender, KeystrokeSentEventArgs e)
        {
            AddActivity((sender as HOTASQueue)?.GetMap(e.Offset), e);
        }

        private void DeviceList_KeystrokeDownSent(object sender, KeystrokeSentEventArgs e)
        {
            AddActivity((sender as HOTASQueue)?.GetMap(e.Offset), e);
        }

        private void AddActivity(IHotasBaseMap map, KeystrokeSentEventArgs e)
        {
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

            _appDispatcher?.Invoke(() =>
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

        private void BuildDevicesViewModelFromLoadedDevices(IHOTASCollection loadedDevices)
        {
            foreach (var ld in loadedDevices.Devices)
            {
                HOTASDevice d;
                var deviceVm = Devices.FirstOrDefault(vm => vm.InstanceId == ld.DeviceId);

                if (deviceVm == null)
                {
                    Logging.Log.Warn($"Loaded mappings for {ld.Name}, but could not find the device attached!");
                    Logging.Log.Warn($"Mappings will be displayed, but they will not function");
                    deviceVm = new DeviceViewModel(ld);
                    Devices.Add(deviceVm);
                    _deviceList.AddDevice(ld);
                    d = ld;
                }
                else
                {
                    d = _deviceList.GetDevice(ld.DeviceId);
                    if (d == null) continue;
                }
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
                deviceViewModel.ReplaceDevice(newDevice);
                
                _deviceList.ReplaceDevice(newDevice);
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

            _fileSystem.LastSavedFileName = "";
            ProfileSetFileName = _fileSystem.LastSavedFileName;
        }

        private void FileSave()
        {
            _deviceList.ClearUnassignedActions();
            _fileSystem.FileSave(_deviceList);
            ProfileSetFileName = _fileSystem.LastSavedFileName;
        }

        private void FileSaveAs()
        {
            _fileSystem.FileSaveAs(_deviceList);
            ProfileSetFileName = _fileSystem.LastSavedFileName;
        }

        private void FileOpenDialog()
        {
            var loadedDeviceList = _fileSystem.FileOpenDialog();
            if (loadedDeviceList == null) return;
            LoadHotas(loadedDeviceList);
        }

        private void LoadHotas(IHOTASCollection loadedDeviceList)
        {
            _deviceList.Stop();

            BuildDevicesViewModelFromLoadedDevices(loadedDeviceList);
            BuildModeProfileActivationListFromLoadedDevices(loadedDeviceList);
            ReBuildActionCatalog();
            AddHandlers();
            ProfileSetFileName = _fileSystem.LastSavedFileName;

            _deviceList.AutoSetMode();
            _deviceList.ListenToAllDevices();

            FileOpened?.Invoke(this, new EventArgs());

            Logging.Log.Info($"Loaded a device set...");
            foreach (var d in Devices)
            {
                Logging.Log.Info($"{d.InstanceId}, {d.Name}");
            }
        }

        private void BuildModeProfileActivationListFromLoadedDevices(IHOTASCollection loadedDevices)
        {
            _deviceList.ModeProfileActivationButtons.Clear();
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

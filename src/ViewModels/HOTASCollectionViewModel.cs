using SierraHOTAS.Annotations;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SierraHOTAS.ViewModels
{
    public class HOTASCollectionViewModel : IDisposable, INotifyPropertyChanged
    {
        private const string ASSIGN_FIRST_PROFILE_MESSAGE = "Before creating a new profile, you must first assign an activation button to the existing profile.";

        private readonly IDispatcher _appDispatcher;
        private readonly IFileSystem _fileSystem;
        private readonly MediaPlayerFactory _mediaPlayerFactory;
        private readonly DeviceViewModelFactory _deviceViewModelFactory;
        private bool? _snapToButton = true;
        private readonly IEventAggregator _eventAggregator;

        public event EventHandler<EventArgs> ShowMainWindow;
        public event EventHandler<EventArgs> Close;
        public event EventHandler<ButtonPressedViewModelEventArgs> ButtonPressed;
        public event EventHandler<AxisChangedViewModelEventArgs> AxisChanged;
        public event EventHandler<ModeChangedEventArgs> ModeChanged;
        public event EventHandler<EventArgs> FileOpened;

        public QuickProfilePanelViewModel QuickProfilePanelViewModel { get; set; }

        public ActionCatalog ActionCatalog => _deviceList.ActionCatalog;
        public ObservableCollection<ActivityItem> Activity { get; set; }
        public ObservableCollection<DeviceViewModel> Devices { get; set; }
        public ObservableCollection<ModeActivationItem> ModeActivationItems => _deviceList.ModeActivationButtons.Values.ToObservableCollection();

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

        private readonly IHOTASCollection _deviceList;

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

        private ICommand _createNewModeCommand;

        public ICommand CreateNewModeCommand => _createNewModeCommand ?? (_createNewModeCommand = new CommandHandler(CreateNewMode));

        private ICommand _editModeCommand;

        public ICommand EditModeCommand => _editModeCommand ?? (_editModeCommand = new CommandHandlerWithParameter<ModeActivationItem>(EditMode));

        private ICommand _deleteModeCommand;

        public ICommand DeleteModeCommand => _deleteModeCommand ?? (_deleteModeCommand = new CommandHandlerWithParameter<ModeActivationItem>(DeleteMode));

        private ICommand _showInputGraphWindowCommand;

        public ICommand ShowInputGraphWindowCommand => _showInputGraphWindowCommand ?? (_showInputGraphWindowCommand = new CommandHandler(ShowInputGraphWindow));
        public HOTASCollectionViewModel(DispatcherFactory dispatcherFactory, IEventAggregator eventAggregator, IFileSystem fileSystem, MediaPlayerFactory mediaPlayerFactory, IHOTASCollection hotasCollection, QuickProfilePanelViewModel quickProfilePanelViewModel, DeviceViewModelFactory deviceViewModelFactory)
        {
            _fileSystem = fileSystem;
            _mediaPlayerFactory = mediaPlayerFactory;
            _deviceViewModelFactory = deviceViewModelFactory;
            _appDispatcher = dispatcherFactory.CreateDispatcher();
            _deviceList = hotasCollection;
            Activity = new ObservableCollection<ActivityItem>();
            QuickProfilePanelViewModel = quickProfilePanelViewModel;

            QuickProfilePanelViewModel.ShowMainWindow += QuickProfilePanelViewModel_ShowMainWindow;
            QuickProfilePanelViewModel.Close += QuickProfilePanelViewModel_Close;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe<QuickProfileSelectedEvent>(QuickLoadProfile);
            _eventAggregator.Subscribe<DeleteModeEvent>(DeleteMode);
        }

        private void QuickProfilePanelViewModel_Close(object sender, EventArgs e)
        {
            Close?.Invoke(this, e);
        }

        private void QuickProfilePanelViewModel_ShowMainWindow(object sender, EventArgs e)
        {
            ShowMainWindow?.Invoke(this, e);
        }

        private void CreateNewMode()
        {
            const int defaultMode = 1;
            if (_deviceList.ModeActivationButtons.Count == 0)
            {
                _eventAggregator.Publish(new ShowMessageWindowEvent() { Message = ASSIGN_FIRST_PROFILE_MESSAGE });

                var isAssigned = AssignActivationButton(defaultMode);
                if (!isAssigned) return;
            }

            var mode = _deviceList.SetupNewMode();
            _deviceList.SetMode(mode);
            AssignActivationButton(mode);
            OnModeChanged(this, new ModeChangedEventArgs() { Mode = _deviceList.Mode });
        }

        private void ShowInputGraphWindow()
        {
            _eventAggregator.Publish(new ShowInputGraphWindowEvent(_deviceList, h => _deviceList.AxisChanged += h, h => _deviceList.AxisChanged -= h));
        }

        private void EditMode(ModeActivationItem item)
        {
            var exists = _deviceList.ModeActivationButtons.TryGetValue(item.Mode, out item);
            if (!exists) return;

            var isCancelled = false;
            var args = new ShowModeConfigWindowEvent(item.Mode, _deviceList.ModeActivationButtons, h => _deviceList.ButtonPressed += h, h => _deviceList.ButtonPressed -= h, () => isCancelled = true);
            _eventAggregator.Publish(args);

            if (isCancelled) return;

            _deviceList.ApplyActivationButtonToAllProfiles();

            OnPropertyChanged(nameof(ModeActivationItems));
        }

        private void DeleteMode(DeleteModeEvent item)
        {
            DeleteMode(item.ActivationItem);
        }

        private void DeleteMode(ModeActivationItem item)
        {
            if (_deviceList.RemoveMode(item))
            {
                OnPropertyChanged(nameof(ModeActivationItems));
            }
        }

        public void SetMode(int mode)
        {
            if (_deviceList.Mode == mode) return;

            _deviceList.SetMode(mode);
            OnModeChanged(this, new ModeChangedEventArgs() { Mode = _deviceList.Mode });
        }

        private bool AssignActivationButton(int mode)
        {
            var isCancelled = false;
            var args = new ShowModeConfigWindowEvent(mode, _deviceList.ModeActivationButtons, h => _deviceList.ButtonPressed += h, h => _deviceList.ButtonPressed -= h, () => isCancelled = true);
            _eventAggregator.Publish(args);

            if (isCancelled) return false;

            if (_deviceList.ModeActivationButtons.Count > 1)
            {
                //need to populate the buttons from the template before assigning the activation button
                _deviceList.ModeActivationButtons.TryGetValue(mode, out var newActivationItem);
                if (newActivationItem != null && newActivationItem.TemplateMode > 0)
                {
                    _deviceList.CopyModeFromTemplate(newActivationItem.TemplateMode, mode);
                }
            }

            _deviceList.ApplyActivationButtonToAllProfiles();
            OnPropertyChanged(nameof(ModeActivationItems));
            return true;
        }

        private void AutoLoadProfile()
        {
            var path = QuickProfilePanelViewModel.GetAutoLoadPath();
            LoadProfile(path);
        }

        private void QuickLoadProfile(QuickProfileSelectedEvent profileInfo)
        {
            if (string.IsNullOrWhiteSpace(profileInfo.Path)) return;

            LoadProfile(profileInfo.Path);
        }

        private void LoadProfile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            var hotas = _fileSystem.FileOpen(path);
            if (hotas == null)
            {
                ProfileSetFileName = $"Could not load {path}!!! Is this a SierraHOTAS compatible JSON file?";
                return;
            }

            if (hotas.Devices.Any(d => d.DeviceId == Guid.Empty))
            {
                ProfileSetFileName = "Invalid device found in profile. Check logs";

                var messageHeader = "Device with an invalid device id found:";
                Logging.Log.Warn(messageHeader);
                Console.WriteLine(messageHeader);

                foreach (var d in hotas.Devices)
                {
                    var messageDetail = $"Invalid device:{d.Name}";
                    Logging.Log.Warn(messageDetail);
                    Console.WriteLine(messageDetail);
                }
                return;
            }

            LoadHotas(hotas);
        }

        public void Initialize()
        {
            if (App.IsDebug)
            {
                _deviceList.Devices = DataProvider.GetDeviceList();
            }
            else
            {
                _deviceList.AxisChanged += DeviceList_AxisChanged;
            }

            _deviceList.ButtonPressed += DeviceList_ButtonPressed;
            _deviceList.KeystrokeDownSent += DeviceList_KeystrokeDownSent;
            _deviceList.MacroStarted += DeviceList_MacroStarted;
            _deviceList.MacroCancelled += DeviceList_MacroCancelled;
            _deviceList.KeystrokeUpSent += DeviceList_KeystrokeUpSent;
            _deviceList.ModeChanged += OnModeChanged;
            _deviceList.LostConnectionToDevice += DeviceList_LostConnectionToDevice;

            _deviceList.Start();

            BuildDevicesViewModel();
            AddHandlers();//TODO remove? addhandlers gets called in LoadHotas

            AutoLoadProfile();
        }

        private void DeviceList_LostConnectionToDevice(object sender, LostConnectionToDeviceEventArgs e)
        {
            //DeviceViewModel is already handling this behavior. Don't really need to do anything here.

            //var deviceVm = Devices.FirstOrDefault(h => h.InstanceId == e.HOTASDevice.DeviceId);
            //if (deviceVm == null) return;
            //_appDispatcher.Invoke(() => Devices.Remove(deviceVm));
        }

        private void OnModeChanged(object sender, ModeChangedEventArgs e)
        {
            _appDispatcher.Invoke(RebuildAllButtonMaps); //crossing thread boundaries from HOTASQueue thread to UI thread
            ModeChanged?.Invoke(sender, e);
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

            if (map.Type == HOTASButton.ButtonType.Button || map.Type == HOTASButton.ButtonType.POV)
            {
                if (!(map is HOTASButton buttonMap)) return;
                actionName = buttonMap.ActionName;
            }
            else
            {
                if (!(map is HOTASAxis axisMap)) return;

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

            var activity = new ActivityItem() { Offset = e.Offset, ButtonName = map.MapName, ScanCode = e.ScanCode, IsKeyUp = e.IsKeyUp, IsExtended = e.IsExtended, ActionName = actionName, Time = DateTime.Now };

            _appDispatcher?.Invoke(() =>
            {
                Activity.Insert(0, activity);
                if (Activity.Count >= 200)
                {
                    Activity.RemoveAt(Activity.Count - 1);
                }
            });
        }

        private void DeviceList_MacroStarted(object sender, MacroStartedEventArgs e)
        {
            AddMacroActivity(sender, e.Offset, e.Code, false, "Macro Started");
        }

        private void DeviceList_MacroCancelled(object sender, MacroCancelledEventArgs e)
        {
            AddMacroActivity(sender, e.Offset, e.Code, true, "Macro Cancelled");
        }

        private void AddMacroActivity(object sender, int offset, int scanCode, bool isKeyUp, string message)
        {
            var map = (sender as HOTASQueue)?.GetMap(offset);
            if (map == null) return;
            var activity = new ActivityItem()
            {
                Offset = offset,
                ButtonName = map.MapName,
                ScanCode = scanCode,
                IsKeyUp = isKeyUp,
                IsExtended = true,
                ActionName = message,
                Time = DateTime.Now
            };

            _appDispatcher?.Invoke(() => { Activity.Insert(0, activity); });
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
                IHOTASDevice d;
                var deviceVm = Devices.FirstOrDefault(vm => vm.InstanceId == ld.DeviceId && ld.DeviceId != Guid.Empty);

                if (deviceVm == null)
                {
                    Logging.Log.Warn($"Loaded mappings for {ld.Name}, but could not find the device attached!");
                    Logging.Log.Warn($"Mappings will be displayed, but they will not function");
                    deviceVm = _deviceViewModelFactory.CreateDeviceViewModel(_appDispatcher, _fileSystem, _mediaPlayerFactory, ld);
                    Devices.Add(deviceVm);
                    _deviceList.AddDevice(ld);
                    d = ld;
                }
                else
                {
                    d = _deviceList.GetDevice(ld.DeviceId);
                    if (d == null) continue;
                }
                d.SetMode(ld.Modes);
                ReBuildActionsFromCatalog(deviceVm);
                deviceVm.RebuildMap(d.ButtonMap);
            }
        }

        private void BuildDevicesViewModel()
        {
            RemoveAllHandlers();
            Devices = _deviceList.Devices.Select(device => _deviceViewModelFactory.CreateDeviceViewModel(_appDispatcher, _fileSystem, _mediaPlayerFactory, device)).ToObservableCollection();
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
            var newDevices = _deviceList.RefreshMissingDevices();

            //if the device has a mapping already loaded, then assign this device to that mapping
            foreach (var deviceViewModel in Devices)
            {
                var newDevice = newDevices.FirstOrDefault(n => n.DeviceId == deviceViewModel.InstanceId);
                if (newDevice == null) continue;

                newDevices.Remove(newDevice);

                _deviceList.ReplaceDevice(newDevice);
                _deviceList.ListenToDevice(newDevice);

                deviceViewModel.ReplaceDevice(newDevice);
                deviceViewModel.RebuildMap();
            }

            //remaining devices here do not have a mapping loaded, so assign a default mapping
            foreach (var n in newDevices)
            {
                //var vm = new DeviceViewModel(_appDispatcher, _fileSystem, _mediaPlayerFactory, n);
                var vm = _deviceViewModelFactory.CreateDeviceViewModel(_appDispatcher, _fileSystem, _mediaPlayerFactory, n);
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
            _deviceList.ActionCatalog.Clear();

            foreach (var deviceVm in Devices)
            {
                deviceVm.ClearButtonMap();
                deviceVm.RebuildMap();
            }

            _deviceList.ModeActivationButtons.Clear();
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

            RemoveUnconnectedDevices();

            _deviceList.SetCatalog(loadedDeviceList.ActionCatalog);

            BuildDevicesViewModelFromLoadedDevices(loadedDeviceList);
            BuildModeActivationListFromLoadedDevices(loadedDeviceList);
            AddHandlers();
            ProfileSetFileName = _fileSystem.LastSavedFileName;

            //since the JSON file may have less button maps than are on the device, we need to overlay them individually instead of replacing the whole collection
            _deviceList.ApplyButtonMapToAllProfiles();
            _deviceList.AutoSetMode();
            _deviceList.ListenToAllDevices();

            FileOpened?.Invoke(this, new EventArgs());

            Logging.Log.Info($"Loaded a device set...");
            foreach (var d in Devices)
            {
                Logging.Log.Info($"{d.InstanceId}, {d.Name}");
            }
        }

        private void RemoveUnconnectedDevices()
        {
            //remove devices from the profile that are not actually connected at the moment. since these may only exist for that profile, when loading a new profile we don't want to show them
            for (var i = Devices.Count - 1; i >= 0; i--)
            {
                if (Devices[i].IsDeviceLoaded) continue;
                Devices.RemoveAt(i);
            }
        }

        private void BuildModeActivationListFromLoadedDevices(IHOTASCollection loadedDevices)
        {
            _deviceList.ModeActivationButtons.Clear();
            foreach (var item in loadedDevices.ModeActivationButtons)
            {
                _deviceList.ModeActivationButtons.Add(item.Key, item.Value);
            }
            OnPropertyChanged(nameof(ModeActivationItems));
        }

        private void ReBuildActionsFromCatalog(DeviceViewModel deviceVm)
        {
            foreach (var mode in deviceVm.Modes)
            {
                foreach (var m in mode.Value)
                {
                    if (string.IsNullOrWhiteSpace(m.MapName)) continue;
                    switch (m)
                    {
                        case HOTASAxis axisMap:
                            {
                                MapButtonListToCatalog(axisMap.ButtonMap);
                                MapButtonListToCatalog(axisMap.ReverseButtonMap);
                                break;
                            }
                        case HOTASButton buttonMap:
                            {
                                MapButtonToCatalog(buttonMap);
                                break;
                            }
                    }
                }
            }
        }

        private void MapButtonListToCatalog(ObservableCollection<HOTASButton> mapList)
        {
            foreach (var buttonMap in mapList)
            {
                MapButtonToCatalog(buttonMap);
            }
        }

        private void MapButtonToCatalog(HOTASButton buttonMap)
        {
            var item = ActionCatalog.Get(buttonMap.ActionId);
            if (item == null) return;
            buttonMap.ActionCatalogItem = item;
        }

        /// <summary>
        /// bound to lstDevices in MainWindow.xaml
        /// </summary>
        /// <param name="device"></param>
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
            ActionCatalog.Add(mapVm.ActionItem, mapVm.ButtonName);
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

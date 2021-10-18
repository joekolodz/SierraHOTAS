using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using SierraHOTAS.Factories;

namespace SierraHOTAS.ViewModels
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        private IHOTASDevice _hotasDevice = null;
        private readonly IFileSystem _fileSystem;
        private readonly MediaPlayerFactory _mediaPlayerFactory;
        private readonly IDispatcher _appDispatcher;

        public virtual event EventHandler RecordingStopped;

        public Guid InstanceId { get; set; }

        public string Name { get; set; }

        public string PID => GetPID(_hotasDevice.ProductId);

        public static string GetPID(Guid productGuid)
        {
            return productGuid.ToString().Substring(0, 4).ToUpper();
        }

        public string VID => GetVID(_hotasDevice.ProductId);

        public static string GetVID(Guid productGuid)
        {
            return productGuid.ToString().Substring(4, 4).ToUpper();
        }

        public string NameWithIds => GetnNameWithIds(VID, PID, Name);

        public static string GetnNameWithIds(string vid, string pid, string deviceName)
        {
            return string.Format($"{deviceName}  -  (VID:{vid} PID:{pid})");
        }


        public ObservableCollection<IBaseMapViewModel> ButtonMap { get; set; }
        public Dictionary<int, ObservableCollection<IHotasBaseMap>> ModeProfiles => _hotasDevice.ModeProfiles;

        public bool IsDeviceLoaded => _hotasDevice?.Capabilities != null;

        public DeviceViewModel()
        {
        }

        public DeviceViewModel(IDispatcher dispatcher, IFileSystem fileSystem, MediaPlayerFactory mediaPlayerFactory, IHOTASDevice device)
        {
            _appDispatcher = dispatcher;
            _fileSystem = fileSystem;
            _mediaPlayerFactory = mediaPlayerFactory;
            _hotasDevice = device;
            InstanceId = _hotasDevice.DeviceId;
            Name = _hotasDevice.Name;
            _hotasDevice.LostConnectionToDevice += _hotasDevice_LostConnectionToDevice;
            _hotasDevice.AxisChanged += _hotasDevice_AxisChanged;

            ButtonMap = new ObservableCollection<IBaseMapViewModel>();
            RebuildMap();
        }

        private void _hotasDevice_AxisChanged(object sender, AxisChangedEventArgs e)
        {
            IBaseMapViewModel map;
            try
            {
                map = ButtonMap.FirstOrDefault(axis => axis.ButtonId == e.AxisId);
            }
            catch (Exception exception)
            {
                Logging.Log.Fatal(exception);
                return;
            }
            map?.SetAxis(e.Value);
        }

        private void _hotasDevice_LostConnectionToDevice(object sender, LostConnectionToDeviceEventArgs e)
        {
            OnPropertyChanged(nameof(IsDeviceLoaded));
        }

        public void ReplaceDevice(IHOTASDevice newDevice)
        {
            _hotasDevice.LostConnectionToDevice -= _hotasDevice_LostConnectionToDevice;
            _hotasDevice = newDevice;
            InstanceId = _hotasDevice.DeviceId;
            Name = _hotasDevice.Name;
            _hotasDevice.LostConnectionToDevice += _hotasDevice_LostConnectionToDevice;
            OnPropertyChanged(nameof(IsDeviceLoaded));
        }

        public void ClearButtonMap()
        {
            RemoveAllButtonMapHandlers();
            _hotasDevice.ClearUnassignedActions();
        }

        public void RebuildMap()
        {
            RebuildMap(_hotasDevice.ButtonMap);
        }

        public void RebuildMap(ObservableCollection<IHotasBaseMap> map)
        {
            RemoveAllButtonMapHandlers();

            ButtonMap.Clear();

            foreach (var baseMap in map)
            {
                switch (baseMap.Type)
                {
                    case HOTASButton.ButtonType.AxisLinear:
                        var linearMap = baseMap as HOTASAxis;
                        var lmVm = new AxisMapViewModel(_appDispatcher, _mediaPlayerFactory, _fileSystem, linearMap);
                        AddAxisMapHandlers(lmVm);
                        ButtonMap.Add(lmVm);
                        break;

                    case HOTASButton.ButtonType.AxisRadial:
                        var radialMap = baseMap as HOTASAxis;
                        var rmVm = new AxisMapViewModel(_appDispatcher, _mediaPlayerFactory, _fileSystem, radialMap);
                        AddAxisMapHandlers(rmVm);
                        ButtonMap.Add(rmVm);
                        break;

                    default:
                        var buttonMap = baseMap as HOTASButton;
                        var bmVm = new ButtonMapViewModel(buttonMap);
                        AddButtonMapHandlers(bmVm);
                        ButtonMap.Add(bmVm);
                        break;
                }
            }
        }

        public void ForceButtonPress(JoystickOffset offset, bool isDown)
        {
            _hotasDevice.ForceButtonPress(offset, isDown);
        }

        private void AddButtonMapHandlers(ButtonMapViewModel mapViewModel)
        {
            mapViewModel.RecordingStarted += MapViewModel_RecordingStarted;
            mapViewModel.RecordingStopped += MapViewModel_RecordingStopped;
            mapViewModel.RecordingCancelled += MapViewModel_RecordingCancelled;
        }

        private void AddAxisMapHandlers(AxisMapViewModel mapViewModel)
        {
            mapViewModel.RecordingStopped += MapViewModel_RecordingStopped;
        }

        private void RemoveAllButtonMapHandlers()
        {
            foreach (var mapViewModel in ButtonMap)
            {
                if (mapViewModel is ButtonMapViewModel button)
                {
                    button.RecordingStarted -= MapViewModel_RecordingStarted;
                    button.RecordingStopped -= MapViewModel_RecordingStopped;
                    button.RecordingCancelled -= MapViewModel_RecordingCancelled;
                }
                else
                {
                    ((AxisMapViewModel)mapViewModel).RecordingStopped -= MapViewModel_RecordingStopped;
                }
            }
        }

        private void ForceDisableAllOtherMaps(object sender, bool isDisabled)
        {
            foreach (var map in ButtonMap)
            {
                if (sender == map) continue;
                map.IsDisabledForced = isDisabled;
            }
        }

        private void MapViewModel_RecordingStarted(object sender, EventArgs e)
        {
            ForceDisableAllOtherMaps(sender, true);
        }

        private void MapViewModel_RecordingStopped(object sender, EventArgs e)
        {
            ForceDisableAllOtherMaps(sender, false);
            RecordingStopped?.Invoke(sender, e);
        }

        private void MapViewModel_RecordingCancelled(object sender, EventArgs e)
        {
            ForceDisableAllOtherMaps(sender, false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

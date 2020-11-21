using SharpDX.DirectInput;
using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using SierraHOTAS.Factories;

namespace SierraHOTAS.ViewModels
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        private HOTASDevice _hotasDevice = null;
        private readonly IFileSystem _fileSystem;
        private readonly MediaPlayerFactory _mediaPlayerFactory;

        public event EventHandler RecordingStopped;

        public Guid InstanceId { get; set; }

        public string Name { get; set; }

        public ObservableCollection<IBaseMapViewModel> ButtonMap { get; set; }
        public Dictionary<int, ObservableCollection<IHotasBaseMap>> ModeProfiles => _hotasDevice.ModeProfiles;

        public bool IsDeviceLoaded => _hotasDevice?.Capabilities != null;

        public DeviceViewModel()
        {
        }

        public DeviceViewModel(IFileSystem fileSystem, MediaPlayerFactory mediaPlayerFactory, HOTASDevice device)
        {
            _fileSystem = fileSystem;
            _mediaPlayerFactory = mediaPlayerFactory;
            _hotasDevice = device;
            InstanceId = _hotasDevice.DeviceId;
            Name = _hotasDevice.Name;
            _hotasDevice.LostConnectionToDevice += _hotasDevice_LostConnectionToDevice;
            ButtonMap = new ObservableCollection<IBaseMapViewModel>();
            RebuildMap();
        }

        private void _hotasDevice_LostConnectionToDevice(object sender, LostConnectionToDeviceEventArgs e)
        {
            OnPropertyChanged(nameof(IsDeviceLoaded));
        }

        public void ReplaceDevice(HOTASDevice newDevice)
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
                    case HOTASButtonMap.ButtonType.AxisLinear:
                        var linearMap = baseMap as HOTASAxisMap;
                        var lmVm = new AxisMapViewModel(_mediaPlayerFactory, _fileSystem, linearMap);
                        AddAxisMapHandlers(lmVm);
                        ButtonMap.Add(lmVm);
                        break;

                    case HOTASButtonMap.ButtonType.AxisRadial:
                        var radialMap = baseMap as HOTASAxisMap;
                        var rmVm = new AxisMapViewModel(_mediaPlayerFactory, _fileSystem, radialMap);
                        AddAxisMapHandlers(rmVm);
                        ButtonMap.Add(rmVm);
                        break;

                    default:
                        var buttonMap = baseMap as HOTASButtonMap;
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

        public void SetAxis(int buttonId, int value)
        {
            var map = ButtonMap.FirstOrDefault(axis => axis.ButtonId == buttonId);
            map?.SetAxis(value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

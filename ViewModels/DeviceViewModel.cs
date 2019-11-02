using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SierraHOTAS.ViewModels
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        private HOTASDevice _hotasDevice = null;

        public event EventHandler RecordingStopped;

        public Guid InstanceId { get; set; }

        public string Name { get; set; }

        public ObservableCollection<IBaseMapViewModel> ButtonMap { get; set; }

        public DeviceViewModel()
        {
        }

        public DeviceViewModel(HOTASDevice device)
        {
            _hotasDevice = device;
            InstanceId = _hotasDevice.InstanceId;
            Name = _hotasDevice.Name;

            BuildMap();
        }

        public void RebuildMap()
        {
            RemoveAllHandlers();

            ButtonMap.Clear();

            foreach (var baseMap in _hotasDevice.ButtonMap)
            {
                switch (baseMap.Type)
                {
                    case HOTASButtonMap.ButtonType.AxisLinear:
                        var linearMap = baseMap as HOTASAxisMap;
                        ButtonMap.Add(new LinearAxisMapViewModel(linearMap));
                        break;

                    case HOTASButtonMap.ButtonType.AxisRadial:
                        var radialMap = baseMap as HOTASAxisMap;
                        ButtonMap.Add(new RadialAxisMapViewModel(radialMap));
                        break;

                    default:
                        var buttonMap = baseMap as HOTASButtonMap;
                        var mapViewModel = new ButtonMapViewModel(buttonMap);
                        AddHandlers(mapViewModel);
                        ButtonMap.Add(mapViewModel);
                        break;
                }
            }
        }

        private void AddHandlers(ButtonMapViewModel mapViewModel)
        {
            mapViewModel.RecordingStarted += MapViewModel_RecordingStarted;
            mapViewModel.RecordingStopped += MapViewModel_RecordingStopped;
            mapViewModel.RecordingCancelled += MapViewModel_RecordingCancelled;
        }

        private void RemoveAllHandlers()
        {
            foreach (var mapViewModel in ButtonMap)
            {
                if (mapViewModel is ButtonMapViewModel)
                {
                    ((ButtonMapViewModel)mapViewModel).RecordingStarted -= MapViewModel_RecordingStarted;
                    ((ButtonMapViewModel)mapViewModel).RecordingStopped -= MapViewModel_RecordingStopped;
                    ((ButtonMapViewModel)mapViewModel).RecordingCancelled -= MapViewModel_RecordingCancelled;
                }
            }
        }

        private void BuildMap()
        {
            ButtonMap = new ObservableCollection<IBaseMapViewModel>();
            RebuildMap();
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

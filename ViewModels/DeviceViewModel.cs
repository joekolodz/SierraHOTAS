using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SierraHOTAS.ViewModel
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        private HOTASDevice _hotasDevice = null;

        public event EventHandler RecordingStopped;

        public Guid InstanceId { get; set; }

        public string Name { get; set; }

        public ObservableCollection<MapViewModel> ButtonMap { get; set; }

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
            foreach (var m in _hotasDevice.ButtonMap)
            {
                var mapViewModel = new MapViewModel(m);
                AddHandlers(mapViewModel);
                ButtonMap.Add(mapViewModel);
            }
        }

        private void AddHandlers(MapViewModel mapViewModel)
        {
            mapViewModel.RecordingStarted += MapViewModel_RecordingStarted;
            mapViewModel.RecordingStopped += MapViewModel_RecordingStopped;
            mapViewModel.RecordingCancelled += MapViewModel_RecordingCancelled;
        }

        private void RemoveAllHandlers()
        {
            foreach (var mapViewModel in ButtonMap)
            {
                mapViewModel.RecordingStarted -= MapViewModel_RecordingStarted;
                mapViewModel.RecordingStopped -= MapViewModel_RecordingStopped;
                mapViewModel.RecordingCancelled -= MapViewModel_RecordingCancelled;
            }
        }

        private void BuildMap()
        {
            ButtonMap = new ObservableCollection<MapViewModel>();
            RebuildMap();
        }

        
        private void ForceDisableAllOtherMaps(object sender, bool isDisabled)
        {

            //foreach (var map in WhatMap)
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

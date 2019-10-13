using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using SierraHOTAS.Annotations;
using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModel
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        private HOTASDevice _hotasDevice = null;

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
            foreach (var m in _hotasDevice.ButtonMap)
            {
                var mapViewModel = new MapViewModel(m);
                mapViewModel.RecordingStarted += MapViewModel_RecordingStarted;
                mapViewModel.RecordingStopped += MapViewModel_RecordingStopped;
                mapViewModel.RecordingCancelled += MapViewModel_RecordingCancelled;
                ButtonMap.Add(mapViewModel);
            }
        }

        private void BuildMap()
        {
            ButtonMap = new ObservableCollection<MapViewModel>();
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

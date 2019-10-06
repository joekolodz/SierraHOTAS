using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModel
{
    public class DeviceViewModel
    {
        private HOTASDevice _hotasDevice = null;

        public Guid InstanceId { get; set; }

        public string Name { get; set; }

        public List<MapViewModel> ButtonMap { get; set; }

        public DeviceViewModel(HOTASDevice device)
        {
            _hotasDevice = device;
            InstanceId = _hotasDevice.InstanceId;
            Name = _hotasDevice.Name;
            BuildMap();
        }

        private void BuildMap()
        {
            ButtonMap = new List<MapViewModel>();
            foreach (var m in _hotasDevice.ButtonMap)
            {
                var mapViewModel = new MapViewModel(m);
                mapViewModel.RecordingStarted += MapViewModel_RecordingStarted;
                mapViewModel.RecordingStopped += MapViewModel_RecordingStopped;
                mapViewModel.RecordingCancelled += MapViewModel_RecordingCancelled;
                ButtonMap.Add(mapViewModel);
            }
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
    }
}

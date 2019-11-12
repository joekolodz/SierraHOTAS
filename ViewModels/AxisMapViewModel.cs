﻿using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SierraHOTAS.ViewModels
{
    public enum AxisDirection
    {
        Forward,
        Backward
    }

    public class AxisMapViewModel : IBaseMapViewModel, INotifyPropertyChanged
    {
        private readonly HOTASAxisMap _hotasAxisMap;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<int> OnAxisValueChanged;
        public event EventHandler RecordingStopped;

        public bool IsDisabledForced { get; set; }
        public bool IsRecording { get; set; }

        public int ButtonId
        {
            get => _hotasAxisMap.MapId;
            set => _hotasAxisMap.MapId = value;
        }

        public string ButtonName
        {
            get => _hotasAxisMap.MapName;
            set
            {
                if (_hotasAxisMap.MapName == value) return;
                _hotasAxisMap.MapName = value;
                OnPropertyChanged(nameof(ButtonName));
            }
        }

        public HOTASButtonMap.ButtonType Type
        {
            get => _hotasAxisMap.Type;
            set => _hotasAxisMap.Type = value;
        }

        private int _segmentCount;
        public int SegmentCount
        {
            get => _hotasAxisMap.Segments.Count;
            set
            {
                if (_segmentCount == value) return;
                _segmentCount = value;
                SegmentsCountChanged();
                OnPropertyChanged(nameof(SegmentCount));
            }
        }

        public ObservableCollection<Segment> Segments
        {
            get => _hotasAxisMap.Segments;
            set => _hotasAxisMap.Segments = value;
        }

        public bool IsMultiAction
        {
            get => _hotasAxisMap.IsMultiAction;
            set
            {
                if (_hotasAxisMap.IsMultiAction == value) return;
                _hotasAxisMap.IsMultiAction = value;
                SegmentsCountChanged();
            }
        }

        public bool IsDirectional
        {
            get => _hotasAxisMap.IsDirectional;
            set
            {
                if (_hotasAxisMap.IsDirectional == value) return;
                _hotasAxisMap.IsDirectional = value;
                if (!_hotasAxisMap.IsDirectional) Direction = AxisDirection.Forward;
                SegmentsCountChanged();
            }
        }

        public AxisDirection Direction { get; set; } = AxisDirection.Forward;

        public ObservableCollection<ButtonMapViewModel> ButtonMap { get; set; }
        public ObservableCollection<ButtonMapViewModel> ReverseButtonMap { get; set; }

        private readonly MediaPlayer _mediaPlayer;

        public AxisMapViewModel(HOTASAxisMap map)
        {
            ButtonMap = new ObservableCollection<ButtonMapViewModel>();
            ReverseButtonMap = new ObservableCollection<ButtonMapViewModel>();
            _hotasAxisMap = map;
            _segmentCount = _hotasAxisMap.Segments.Count;

            _hotasAxisMap.OnAxisDirectionChanged += OnAxisDirectionChanged;
            _hotasAxisMap.OnAxisSegmentChanged += OnAxisSegmentChanged;

            _mediaPlayer = new MediaPlayer { Volume = 0f };
            _mediaPlayer.Open(new Uri(@"Sounds\click05.mp3", UriKind.Relative));
            RebuildButtonMapViewModels();
        }

        public bool SegmentFilter(object item)
        {
            return _hotasAxisMap.SegmentFilter((Segment) item);
        }

        private void SegmentsCountChanged()
        {
            ResetSegments();

            _hotasAxisMap.CalculateSegmentRange(_segmentCount);
            RemoveAllHandlers();
            ButtonMap.Clear();
            ReverseButtonMap.Clear();

            if (_segmentCount == 1) return;

            RebuildButtonMapViewModels();
        }

        private void RebuildButtonMapViewModels()
        {
            foreach (var map in _hotasAxisMap.ButtonMap)
            {
                var vm = new ButtonMapViewModel(map);
                AddHandlers(vm);
                ButtonMap.Add(vm);
            }

            foreach (var map in _hotasAxisMap.ReverseButtonMap)
            {
                var vm = new ButtonMapViewModel(map);
                AddHandlers(vm);
                ReverseButtonMap.Add(vm);
            }
        }

        public void ResetSegments()
        {
            _hotasAxisMap.Clear();
        }

        public void SetAxis(int value)
        {
            OnAxisValueChanged?.Invoke(this, value);
        }

        private void OnAxisDirectionChanged(object sender, AxisDirectionChangedEventArgs e)
        {
            Direction = e.NewDirection;
        }

        private void OnAxisSegmentChanged(object sender, AxisSegmentChangedEventArgs e)
        {
            _mediaPlayer.Dispatcher?.Invoke(() =>
            {
                _mediaPlayer.Volume = 1f;
                _mediaPlayer.Play();
                _mediaPlayer.Position = TimeSpan.Zero;
            });
            Logging.Log.Info($"Segment changed: {e.NewSegment}");
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

        private void ForceDisableAllOtherMaps(object sender, bool isDisabled)
        {
            foreach (var map in ButtonMap)
            {
                if (sender == map) continue;
                map.IsDisabledForced = isDisabled;
            }

            foreach (var map in ReverseButtonMap)
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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

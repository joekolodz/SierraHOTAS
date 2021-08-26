using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SierraHOTAS.Factories;

namespace SierraHOTAS.ViewModels
{
    public enum AxisDirection
    {
        Forward,
        Backward
    }

    public class AxisMapViewModel : IBaseMapViewModel, INotifyPropertyChanged
    {
        private readonly HOTASAxis _hotasAxis;
        private readonly IFileSystem _fileSystem;
        private IMediaPlayer _mediaPlayer;
        private readonly IDispatcher _appDispatcher;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<AxisChangedViewModelEventArgs> OnAxisValueChanged;
        public event EventHandler RecordingStopped;
        public event EventHandler<EventArgs> SegmentBoundaryChanged;

        public bool IsDisabledForced { get; set; }
        public bool IsRecording { get; set; }

        public int ButtonId
        {
            get => _hotasAxis.MapId;
            set => _hotasAxis.MapId = value;
        }

        public string ButtonName
        {
            get => _hotasAxis.MapName;
            set
            {
                if (_hotasAxis.MapName == value) return;
                _hotasAxis.MapName = value;
                OnPropertyChanged();
            }
        }

        private ICommand _fileOpenCommand;
        public ICommand OpenFileCommand => _fileOpenCommand ?? (_fileOpenCommand = new CommandHandler(LoadNewSound, () => CanExecute));

        private ICommand _removeSoundCommand;
        public ICommand RemoveSoundCommand => _removeSoundCommand ?? (_removeSoundCommand = new CommandHandler(RemoveSound, () => CanExecute));

        public bool CanExecute => true;

        public HOTASButton.ButtonType Type
        {
            get => _hotasAxis.Type;
            set => _hotasAxis.Type = value;
        }

        private int _segmentCount;
        public int SegmentCount
        {
            get => _hotasAxis.Segments.Count;
            set
            {
                if (_segmentCount == value) return;
                _segmentCount = value;
                SegmentsCountChanged();
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Segment> Segments
        {
            get => _hotasAxis.Segments;
            set => _hotasAxis.Segments = value;
        }

        public bool IsMultiAction
        {
            get => _hotasAxis.IsMultiAction;
            set
            {
                if (_hotasAxis.IsMultiAction == value) return;
                _hotasAxis.IsMultiAction = value;
                MultiActionChanged();
                OnPropertyChanged();
            }
        }

        public bool IsDirectional
        {
            get => _hotasAxis.IsDirectional;
            set
            {
                if (_hotasAxis.IsDirectional == value) return;
                _hotasAxis.IsDirectional = value;
                if (!_hotasAxis.IsDirectional) Direction = AxisDirection.Forward;
                DirectionChanged();
                OnPropertyChanged();
            }
        }

        public string SoundFileName
        {
            get => _hotasAxis.SoundFileName;
            set
            {
                if (_hotasAxis.SoundFileName == value) return;
                _hotasAxis.SoundFileName = value;
                OnPropertyChanged();
            }
        }

        public double SoundVolume
        {
            get => _hotasAxis.SoundVolume;
            set
            {
                if (Math.Abs(_hotasAxis.SoundVolume - value) < 0.05d) return;
                _hotasAxis.SoundVolume = value;
                OnPropertyChanged();
            }
        }

        public AxisDirection Direction { get; set; } = AxisDirection.Forward;

        private ObservableCollection<ButtonMapViewModel> _buttonMap;
        public ObservableCollection<ButtonMapViewModel> ButtonMap
        {
            get => _buttonMap;
            set
            {
                if (_buttonMap == value) return;
                RemoveHandlersToButtonMapCollection();
                _buttonMap = value;
                AddHandlersToButtonMapCollection();
            }
        }

        private ObservableCollection<ButtonMapViewModel> _reverseButtonMap;
        public ObservableCollection<ButtonMapViewModel> ReverseButtonMap
        {
            get => _reverseButtonMap;
            set
            {
                if (_reverseButtonMap == value) return;
                RemoveHandlersToReverseButtonMapCollection();
                _reverseButtonMap = value;
                AddHandlersToReverseButtonMapCollection();
            }
        }

        public AxisMapViewModel(IDispatcher dispatcher, MediaPlayerFactory mediaPlayerFactory, IFileSystem fileSystem, HOTASAxis map)
        {
            _appDispatcher = dispatcher;
            _fileSystem = fileSystem;
            ButtonMap = new ObservableCollection<ButtonMapViewModel>();
            AddHandlersToButtonMapCollection();

            ReverseButtonMap = new ObservableCollection<ButtonMapViewModel>();
            AddHandlersToReverseButtonMapCollection();

            _hotasAxis = map;
            _segmentCount = _hotasAxis.Segments.Count;

            _hotasAxis.OnAxisDirectionChanged += OnAxisDirectionChanged;
            _hotasAxis.OnAxisSegmentChanged += OnAxisSegmentChanged;

            _mediaPlayer = mediaPlayerFactory.CreateMediaPlayer();
            _mediaPlayer.Volume = 0f;

            if (string.IsNullOrWhiteSpace(SoundFileName))
            {
                _mediaPlayer.IsMuted = true;
            }
            else
            {
                _mediaPlayer.Open(new Uri(map.SoundFileName, UriKind.Relative));
            }
            RebuildAllButtonMapViewModels();
        }

        private void AddHandlersToButtonMapCollection()
        {
            ButtonMap.CollectionChanged += ButtonMap_CollectionChanged;
        }

        private void RemoveHandlersToButtonMapCollection()
        {
            if (ButtonMap == null) return;
            ButtonMap.CollectionChanged -= ButtonMap_CollectionChanged;
        }

        private void ButtonMap_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ButtonMap));
        }

        private void AddHandlersToReverseButtonMapCollection()
        {
            ReverseButtonMap.CollectionChanged += ReverseButtonMap_CollectionChanged;
        }

        private void RemoveHandlersToReverseButtonMapCollection()
        {
            if (ReverseButtonMap == null) return;
            ReverseButtonMap.CollectionChanged -= ReverseButtonMap_CollectionChanged;
        }

        private void ReverseButtonMap_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ReverseButtonMap));
        }

        public bool SegmentFilter(object item)
        {
            return _hotasAxis.SegmentFilter((Segment)item);
        }

        private void SegmentsCountChanged()
        {
            ResetSegments();
            _hotasAxis.CalculateSegmentRange(_segmentCount);
            AddSegmentHandlers();

            RemoveAllButtonMapHandlers();
            ButtonMap.Clear();
            ReverseButtonMap.Clear();

            if (_segmentCount == 1) return;

            RebuildAllButtonMapViewModels();
        }

        private void DirectionChanged()
        {
            RemoveReverseButtonMapHandlers();
            ReverseButtonMap.Clear();
            _hotasAxis.ReverseButtonMap.Clear();
            _hotasAxis.CreateActionMapList();

            if (_segmentCount == 1) return;

            RebuildReverseButtonMapViewModels();
        }

        private void MultiActionChanged()
        {
            if (_segmentCount == 1) return;

            RemoveAllButtonMapHandlers();
            ButtonMap.Clear();
            ReverseButtonMap.Clear();

            _hotasAxis.CreateActionMapList();

            RebuildAllButtonMapViewModels();
        }

        private void RebuildAllButtonMapViewModels()
        {
            RebuildForwardButtonMapViewModels();
            RebuildReverseButtonMapViewModels();
        }

        private void RebuildForwardButtonMapViewModels()
        {
            foreach (var map in _hotasAxis.ButtonMap)
            {
                var vm = new ButtonMapViewModel(map);
                AddButtonMapHandlers(vm);
                ButtonMap.Add(vm);
            }
        }

        private void RebuildReverseButtonMapViewModels()
        {
            foreach (var map in _hotasAxis.ReverseButtonMap)
            {
                var vm = new ButtonMapViewModel(map);
                AddButtonMapHandlers(vm);
                ReverseButtonMap.Add(vm);
            }
        }

        public void ResetSegments()
        {
            RemoveSegmentHandlers();
            _hotasAxis.ClearSegments();
        }

        private void AddSegmentHandlers()
        {
            foreach (var item in Segments)
            {
                item.PropertyChanged += Segment_PropertyChanged;
            }
        }

        private void RemoveSegmentHandlers()
        {
            foreach (var item in Segments)
            {
                item.PropertyChanged -= Segment_PropertyChanged;
            }
        }

        private void Segment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SegmentBoundaryChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// The deviceVM was notified by the device that its axis has changed. This method is used to update the UI
        /// </summary>
        /// <param name="value"></param>
        public void SetAxis(int value)
        {
            _appDispatcher?.Invoke(() =>
            {
                OnAxisValueChanged?.Invoke(this, new AxisChangedViewModelEventArgs() { Value = value });
            });
        }

        private void OnAxisDirectionChanged(object sender, AxisDirectionChangedEventArgs e)
        {
            Direction = e.NewDirection;
        }

        private void OnAxisSegmentChanged(object sender, AxisSegmentChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SoundFileName)) return;
            if (sender is HOTASAxis axisMap)
            {
                _appDispatcher?.Invoke(() =>
                {
                    _mediaPlayer.Volume = axisMap.SoundVolume;
                    _mediaPlayer.Play();
                    _mediaPlayer.Position = TimeSpan.Zero;
                });
            }
        }

        private void AddButtonMapHandlers(ButtonMapViewModel mapViewModel)
        {
            mapViewModel.RecordingStarted += MapViewModel_RecordingStarted;
            mapViewModel.RecordingStopped += MapViewModel_RecordingStopped;
            mapViewModel.RecordingCancelled += MapViewModel_RecordingCancelled;
        }

        private void RemoveAllButtonMapHandlers()
        {
            RemoveForwardButtonMapHandlers();
            RemoveReverseButtonMapHandlers();
        }

        private void RemoveForwardButtonMapHandlers()
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
        private void RemoveReverseButtonMapHandlers()
        {
            foreach (var mapViewModel in ReverseButtonMap)
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

        private void LoadNewSound()
        {
            var soundFileName = _fileSystem.GetSoundFileName();
            if (string.IsNullOrWhiteSpace(soundFileName)) return;
            SoundFileName = soundFileName;
            _mediaPlayer.Close();
            _mediaPlayer.Open(new Uri(SoundFileName, UriKind.Relative));
            _mediaPlayer.IsMuted = false;
        }

        private void RemoveSound()
        {
            SoundFileName = string.Empty;
            _mediaPlayer.IsMuted = true;
        }
    }
}

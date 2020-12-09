using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
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
        private readonly HOTASAxisMap _hotasAxisMap;
        private readonly IFileSystem _fileSystem;
        private readonly MediaPlayerFactory _mediaPlayerFactory;
        private IMediaPlayer _mediaPlayer;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<int> OnAxisValueChanged;
        public event EventHandler RecordingStopped;
        public event EventHandler SegmentBoundaryChanged;

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
                OnPropertyChanged();
            }
        }

        private ICommand _fileOpenCommand;
        public ICommand OpenFileCommand => _fileOpenCommand ?? (_fileOpenCommand = new CommandHandler(LoadNewSound, () => CanExecute));

        private ICommand _removeSoundCommand;
        public ICommand RemoveSoundCommand => _removeSoundCommand ?? (_removeSoundCommand = new CommandHandler(RemoveSound, () => CanExecute));

        public bool CanExecute => true;

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
                OnPropertyChanged();
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
                MultiActionChanged();
                OnPropertyChanged();
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
                DirectionChanged();
                OnPropertyChanged();
            }
        }

        public string SoundFileName
        {
            get => _hotasAxisMap.SoundFileName;
            set
            {
                if (_hotasAxisMap.SoundFileName == value) return;
                _hotasAxisMap.SoundFileName = value;
                OnPropertyChanged();
            }
        }

        public double SoundVolume
        {
            get => _hotasAxisMap.SoundVolume;
            set
            {
                if (Math.Abs(_hotasAxisMap.SoundVolume - value) < 1) return;
                _hotasAxisMap.SoundVolume = value;
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

        public AxisMapViewModel(MediaPlayerFactory mediaPlayerFactory, IFileSystem fileSystem, HOTASAxisMap map)
        {
            _mediaPlayerFactory = mediaPlayerFactory;
            _fileSystem = fileSystem;
            ButtonMap = new ObservableCollection<ButtonMapViewModel>();
            AddHandlersToButtonMapCollection();

            ReverseButtonMap = new ObservableCollection<ButtonMapViewModel>();
            AddHandlersToReverseButtonMapCollection();

            _hotasAxisMap = map;
            _segmentCount = _hotasAxisMap.Segments.Count;

            _hotasAxisMap.OnAxisDirectionChanged += OnAxisDirectionChanged;
            _hotasAxisMap.OnAxisSegmentChanged += OnAxisSegmentChanged;

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
            return _hotasAxisMap.SegmentFilter((Segment)item);
        }

        private void SegmentsCountChanged()
        {
            ResetSegments();
            _hotasAxisMap.CalculateSegmentRange(_segmentCount);
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
            _hotasAxisMap.ReverseButtonMap.Clear();
            _hotasAxisMap.CreateActionMapList();

            if (_segmentCount == 1) return;

            RebuildReverseButtonMapViewModels();
        }

        private void MultiActionChanged()
        {
            if (_segmentCount == 1) return;

            RemoveAllButtonMapHandlers();
            ButtonMap.Clear();
            ReverseButtonMap.Clear();

            _hotasAxisMap.CreateActionMapList();

            RebuildAllButtonMapViewModels();
        }

        private void RebuildAllButtonMapViewModels()
        {
            RebuildForwardButtonMapViewModels();
            RebuildReverseButtonMapViewModels();
        }

        private void RebuildForwardButtonMapViewModels()
        {
            foreach (var map in _hotasAxisMap.ButtonMap)
            {
                var vm = new ButtonMapViewModel(map);
                AddButtonMapHandlers(vm);
                ButtonMap.Add(vm);
            }
        }

        private void RebuildReverseButtonMapViewModels()
        {
            foreach (var map in _hotasAxisMap.ReverseButtonMap)
            {
                var vm = new ButtonMapViewModel(map);
                AddButtonMapHandlers(vm);
                ReverseButtonMap.Add(vm);
            }
        }

        public void ResetSegments()
        {
            RemoveSegmentHandlers();
            _hotasAxisMap.ClearSegments();
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
            if (string.IsNullOrWhiteSpace(SoundFileName)) return;
            if (sender is HOTASAxisMap axisMap)
            {
                _mediaPlayer.Dispatcher?.Invoke(() =>
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

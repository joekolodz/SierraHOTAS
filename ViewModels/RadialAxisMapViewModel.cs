using SierraHOTAS.Annotations;
using SierraHOTAS.Controls;
using SierraHOTAS.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;

namespace SierraHOTAS.ViewModels
{
    public class RadialAxisMapViewModel : IBaseMapViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<int> OnAxisValueChanged;

        private readonly HOTASAxisMap _hotasAxisMap;

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

        private int _segments;
        public int Segments
        {
            get => _hotasAxisMap.Segments.Count;
            set
            {
                if (_segments == value) return;
                _segments = value;
                SegmentsCountChanged();
                OnPropertyChanged(nameof(_segments));
            }
        }


        public bool? IsDirectional
        {
            get => _hotasAxisMap.IsDirectional;
            set
            {
                if (_hotasAxisMap.IsDirectional == value) return;
                _hotasAxisMap.IsDirectional = value ?? false;
                if (!_hotasAxisMap.IsDirectional) Direction = AxisDirection.Forward;
            }
        }

        public AxisDirection Direction { get; set; } = AxisDirection.Forward;

        private int _currentSegment;
        private readonly MediaPlayer _mediaPlayer;
        private int _lastValue;
        private readonly JitterDetection _jitter;


        public Canvas CanvasAxis;

        public RadialAxisMapViewModel(HOTASAxisMap map)
        {
            _hotasAxisMap = map;
            CanvasAxis = new Canvas();

            _jitter = new JitterDetection();

            _mediaPlayer = new MediaPlayer { Volume = 0f };
            _mediaPlayer.Open(new Uri(@"Sounds\click04.mp3", UriKind.Relative));
        }

        private void ResetSegments()
        {
            _hotasAxisMap.Clear();
        }

        public void SegmentsCountChanged()
        {
            _hotasAxisMap.Clear();

            if (_segments == 1)
            {
                _currentSegment = 1;
                return;
            }

            _hotasAxisMap.CalculateSegmentRange(_segments);
        }

        public void SetAxis(int value)
        {
            if (_jitter.IsJitter(value)) return;

            SetDirection(value);
            DetectSelectedSegment(value);
            OnAxisValueChanged?.Invoke(this, value);
        }

        private void SetDirection(int value)
        {
            if (_hotasAxisMap.IsDirectional)
            {
                Direction = value < _lastValue ? AxisDirection.Backward : AxisDirection.Forward;
            }
            _lastValue = value;
        }

        private void DetectSelectedSegment(int value)
        {
            if (_hotasAxisMap.Segments.Count <= 1) return;

            var newSegment = _hotasAxisMap.GetSegmentFromRawValue(value);

            if (newSegment == _currentSegment) return;

            _currentSegment = newSegment;
            _mediaPlayer.Volume = 1f;
            _mediaPlayer.Play();
            _mediaPlayer.Position = TimeSpan.Zero;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

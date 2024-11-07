using SierraHOTAS.ViewModels;
using SierraJSON;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SierraHOTAS.Models
{
    public class HOTASAxis : IHotasBaseMap
    {
        public event EventHandler<AxisDirectionChangedEventArgs> OnAxisDirectionChanged;
        public event EventHandler<AxisSegmentChangedEventArgs> OnAxisSegmentChanged;

        public int MapId { get; set; }
        public string MapName { get; set; }
        public HOTASButton.ButtonType Type { get; set; }
        public ObservableCollection<HOTASButton> ButtonMap { get; set; }
        public ObservableCollection<HOTASButton> ReverseButtonMap { get; set; }

        public bool IsDirectional { get; set; } = true;
        public bool IsMultiAction { get; set; } = false;
        public string SoundFileName { get; set; }
        public float SoundVolume { get; set; } = 1.0f;
        public ObservableCollection<Segment> Segments { get; set; }

        [SierraJsonIgnore]
        public bool IsSegmentChanged { get; set; }

        private int _currentSegment;
        private int _arraySize = 10;
        private int _lastAvg;
        private readonly int[] _previousValues;

        [SierraJsonIgnore]
        public AxisDirection Direction { get; private set; }

        public HOTASAxis()
        {
            Segments = new ObservableCollection<Segment>();
            ButtonMap = new ObservableCollection<HOTASButton>();
            ReverseButtonMap = new ObservableCollection<HOTASButton>();
            _previousValues = new int[_arraySize];
        }

        /// <summary>
        /// All properties are copied except for ActionCatalogItem because ActionCatalogItem is a reference to an entry in a global dictionary that we don't want to duplicate
        /// New Segment property changed handlers are added, not re-referenced.
        /// </summary>
        /// <returns></returns>
        public IHotasBaseMap Clone()
        {
            var cloneButtonMap = new ObservableCollection<HOTASButton>();
            foreach (var b in ButtonMap)
            {
                cloneButtonMap.Add((HOTASButton)b.Clone());
            }

            var cloneReverseButtonMap = new ObservableCollection<HOTASButton>();
            foreach (var b in ReverseButtonMap)
            {
                cloneButtonMap.Add((HOTASButton)b.Clone());
            }

            var cloneSegments = new ObservableCollection<Segment>();
            foreach (var s in Segments)
            {
                cloneSegments.Add(s.Clone());
            }

            var clone = new HOTASAxis()
            {
                MapId = MapId,
                MapName = MapName,
                Type = Type,
                ButtonMap = cloneButtonMap,
                ReverseButtonMap = cloneReverseButtonMap,
                IsDirectional = IsDirectional,
                IsMultiAction = IsMultiAction,
                SoundFileName = SoundFileName,
                SoundVolume = SoundVolume,
                Segments = cloneSegments,
                Direction = Direction
            };

            clone.AddSegmentBoundaryHandlers();

            return clone;
        }

        public void SetAxis(int value)
        {
            SetDirection(value);
            DetectSelectedSegment(value);
        }

        private void SetDirection(int value)
        {
            if (IsDirectional)
            {
                var avg = JitterDetection.CalculateAveragePosition(_previousValues, _arraySize, value);
                Direction = avg < _lastAvg ? AxisDirection.Backward : AxisDirection.Forward;
                _lastAvg = avg;
            }
            else
            {
                Direction = AxisDirection.Forward;
            }

            OnAxisDirectionChanged?.Invoke(this, new AxisDirectionChangedEventArgs() { NewDirection = Direction });
        }

        private void DetectSelectedSegment(int value)
        {
            IsSegmentChanged = false;

            if (Segments.Count <= 1) return;

            var newSegment = GetSegmentFromRawValue(value);

            if (newSegment == _currentSegment) return;

            _currentSegment = newSegment;
            IsSegmentChanged = true;
            OnAxisSegmentChanged?.Invoke(this, new AxisSegmentChangedEventArgs() { NewSegment = _currentSegment });
        }

        public void CalculateSegmentRange(int segments)
        {
            RemoveSegmentBoundaryHandlers();

            Segments.Clear();
            if (segments == 0)
            {
                ButtonMap.Clear();
                ReverseButtonMap.Clear();
                return;
            }

            if (!IsDirectional)
            {
                ReverseButtonMap.Clear();
            }

            CreateSegments(segments);

            CreateActionMapList();

            AddSegmentBoundaryHandlers();
        }

        public void CreateActionMapList()
        {
            if (IsMultiAction)
            {
                CreateMultiActionButtonMapList(Segments.Count);
            }
            else
            {
                CreateSingleActionButtonMapList();
            }
        }

        public void AddSegmentBoundaryHandlers()
        {
            foreach (var item in Segments)
            {
                item.PropertyChanged += Segment_PropertyChanged;
            }
        }

        private void Segment_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //validate segments don't cross each other
            var previous = Segments[0].Value;
            for (var i = 1; i < Segments.Count; i++)
            {
                Segments[i].PropertyChanged -= Segment_PropertyChanged;

                if (Segments[i].Value > ushort.MaxValue)
                {
                    Segments[i].Value = ushort.MaxValue - 655;
                }
                if (Segments[i].Value < previous)
                {
                    Segments[i].Value = previous + 655;
                }
                previous = Segments[i].Value;

                Segments[i].PropertyChanged += Segment_PropertyChanged;
            }
        }

        private void RemoveSegmentBoundaryHandlers()
        {
            foreach (var item in Segments)
            {
                item.PropertyChanged -= Segment_PropertyChanged;
            }
        }

        private void CreateSingleActionButtonMapList()
        {
            for (var i = ButtonMap.Count; i > 1; i--)
            {
                ButtonMap.RemoveAt(i - 1);
            }

            for (var i = ReverseButtonMap.Count; i > 1; i--)
            {
                ReverseButtonMap.RemoveAt(i - 1);
            }

            if (ButtonMap.Count == 0)
            {
                ButtonMap.Add(new HOTASButton() { MapId = 1, MapName = $"Axis Button 1", Type = HOTASButton.ButtonType.Button });
            }

            if (IsDirectional)
            {
                if (ReverseButtonMap.Count == 0)
                {
                    ReverseButtonMap.Add(new HOTASButton() { MapId = 1, MapName = $"Reverse Axis Button 1", Type = HOTASButton.ButtonType.Button });
                }
            }

        }

        private void CreateMultiActionButtonMapList(int segments)
        {
            //try not to lose any macros already established. if more segments, then don't lose any macros. if less segments only trim from bottom of list
            if (segments < ButtonMap.Count)
            {
                for (var i = ButtonMap.Count; i > segments; i--)
                {
                    ButtonMap.RemoveAt(i - 1);
                }
            }
            else
            {
                for (var i = ButtonMap.Count + 1; i <= segments; i++)
                {
                    ButtonMap.Add(new HOTASButton() { MapId = i, MapName = $"Axis Button {i}", Type = HOTASButton.ButtonType.Button });
                }
            }

            if (IsDirectional)
            {
                if (segments < ReverseButtonMap.Count)
                {
                    for (var i = ReverseButtonMap.Count; i > segments; i--)
                    {
                        ReverseButtonMap.RemoveAt(i - 1);
                    }
                }
                else
                {
                    for (var i = ReverseButtonMap.Count + 1; i <= segments; i++)
                    {
                        ReverseButtonMap.Add(new HOTASButton() { MapId = i, MapName = $"Reverse Axis Button {i}", Type = HOTASButton.ButtonType.Button });
                    }
                }
            }
        }

        private void CreateSegments(int segments)
        {
            var segmentRangeBoundary = ushort.MaxValue / (segments);
            for (var i = 1; i < segments; i++)
            {
                Segments.Add(new Segment(i, (segmentRangeBoundary * i)));
            }

            Segments.Add(new Segment(segments, ushort.MaxValue));
        }

        public void ClearSegments()
        {
            Segments.Clear();
        }

        public int GetSegmentFromRawValue(int value)
        {
            var segmentRange = Segments.FirstOrDefault(r => value <= r.Value);
            if (segmentRange == null) return 0;
            return segmentRange.Id;
        }

        public bool SegmentFilter(Segment item)
        {
            return item.Value != ushort.MaxValue;
        }

        public HOTASButton GetButtonMapFromRawValue(int value)
        {
            HOTASButton map;

            var segment = GetSegmentFromRawValue(value);

            if (IsMultiAction)
            {
                map = GetMultiActionMap(segment);
            }
            else
            {
                map = GetSingleActionMap();
            }

            return map;
        }

        private HOTASButton GetSingleActionMap()
        {
            HOTASButton map;
            if (IsDirectional)
            {
                if (Direction == AxisDirection.Forward)
                {
                    map = ButtonMap.FirstOrDefault(m => m.MapId == 1);
                }
                else
                {
                    map = ReverseButtonMap.FirstOrDefault(m => m.MapId == 1);
                }
            }
            else
            {
                map = ButtonMap.FirstOrDefault(m => m.MapId == 1);
            }
            return map;
        }

        private HOTASButton GetMultiActionMap(int segment)
        {
            HOTASButton map;
            if (IsDirectional)
            {
                if (Direction == AxisDirection.Forward)
                {
                    map = ButtonMap.FirstOrDefault(m => m.MapId == segment);
                }
                else
                {
                    map = ReverseButtonMap.FirstOrDefault(m => m.MapId == segment);
                }
            }
            else
            {
                map = ButtonMap.FirstOrDefault(m => m.MapId == segment);
            }
            return map;
        }

        public void ClearUnassignedActions()
        {
            ClearUnassignedActions(ButtonMap);
            ClearUnassignedActions(ReverseButtonMap);
        }

        private void ClearUnassignedActions(ObservableCollection<HOTASButton> map)
        {
            foreach (var b in map)
            {
                if (b.ActionName != ActionCatalogItem.NO_ACTION_TEXT) continue;
                b.ActionName = string.Empty;
                b.ActionCatalogItem.Actions.Clear();
            }
        }
    }
}

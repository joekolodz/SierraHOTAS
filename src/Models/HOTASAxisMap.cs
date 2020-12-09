using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using SierraHOTAS.ViewModels;

namespace SierraHOTAS.Models
{
    public class HOTASAxisMap : IHotasBaseMap
    {
        public event EventHandler<AxisDirectionChangedEventArgs> OnAxisDirectionChanged;
        public event EventHandler<AxisSegmentChangedEventArgs> OnAxisSegmentChanged;

        public int MapId { get; set; }
        public string MapName { get; set; }
        public HOTASButtonMap.ButtonType Type { get; set; }
        public ObservableCollection<HOTASButtonMap> ButtonMap { get; set; }
        public ObservableCollection<HOTASButtonMap> ReverseButtonMap { get; set; }

        public bool IsDirectional { get; set; } = true;
        public bool IsMultiAction { get; set; } = false;
        public string SoundFileName { get; set; }
        public double SoundVolume { get; set; }
        public ObservableCollection<Segment> Segments { get; set; }

        [JsonIgnore]
        public bool IsSegmentChanged { get; set; }

        private int _currentSegment;
        private int _arraySize = 10;
        private int _lastAvg;
        private readonly int[] _previousValues;

        public AxisDirection Direction { get; private set; }

        public HOTASAxisMap()
        {
            Segments = new ObservableCollection<Segment>();
            ButtonMap = new ObservableCollection<HOTASButtonMap>();
            ReverseButtonMap = new ObservableCollection<HOTASButtonMap>();
            SoundVolume = 1.0d;
            _previousValues = new int[_arraySize];
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

        private void AddSegmentBoundaryHandlers()
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
                if (Segments[i].Value > ushort.MaxValue)
                {
                    Segments[i].Value = ushort.MaxValue - 655;
                }
                if (Segments[i].Value < previous)
                {
                    Segments[i].Value = previous + 655;
                }
                previous = Segments[i].Value;
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
                ButtonMap.Add(new HOTASButtonMap() { MapId = 1, MapName = $"Axis Button 1", Type = HOTASButtonMap.ButtonType.Button });
            }

            if (IsDirectional)
            {
                if (ReverseButtonMap.Count == 0)
                {
                    ReverseButtonMap.Add(new HOTASButtonMap() { MapId = 1, MapName = $"Reverse Axis Button 1", Type = HOTASButtonMap.ButtonType.Button });
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
                    ButtonMap.Add(new HOTASButtonMap() { MapId = i, MapName = $"Axis Button {i}", Type = HOTASButtonMap.ButtonType.Button });
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
                        ReverseButtonMap.Add(new HOTASButtonMap() { MapId = i, MapName = $"Reverse Axis Button {i}", Type = HOTASButtonMap.ButtonType.Button });
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

        public HOTASButtonMap GetButtonMapFromRawValue(int value)
        {
            HOTASButtonMap map;

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

        private HOTASButtonMap GetSingleActionMap()
        {
            HOTASButtonMap map;
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

        private HOTASButtonMap GetMultiActionMap(int segment)
        {
            HOTASButtonMap map;
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
            foreach (var b in ButtonMap)
            {
                if (b.ActionName != "<No Action>") return;
                b.ActionName = string.Empty;
                b.ActionCatalogItem.Actions.Clear();
            }
            foreach (var b in ReverseButtonMap)
            {
                if (b.ActionName != "<No Action>") return;
                b.ActionName = string.Empty;
                b.ActionCatalogItem.Actions.Clear();
            }
        }
    }
}

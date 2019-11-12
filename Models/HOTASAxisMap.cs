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

        public ObservableCollection<Segment> Segments { get; set; }
        
        [JsonIgnore]
        public bool IsSegmentChanged { get; set; }

        private int _currentSegment;
        private int _lastValue;
        private AxisDirection _direction;

        public HOTASAxisMap()
        {
            Segments = new ObservableCollection<Segment>();
            ButtonMap = new ObservableCollection<HOTASButtonMap>();
            ReverseButtonMap = new ObservableCollection<HOTASButtonMap>();
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
                _direction = value < _lastValue ? AxisDirection.Backward : AxisDirection.Forward;
            }
            else
            {
                _direction = AxisDirection.Forward;
            }

            _lastValue = value;

            OnAxisDirectionChanged?.Invoke(this, new AxisDirectionChangedEventArgs() { NewDirection = _direction });
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

            if (IsMultiAction)
            {
                CreateMultiActionButtonMapList(segments);
            }
            else
            {
                CreateSingleActionButtonMapList();
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

        public void Clear()
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
                if (_direction == AxisDirection.Forward)
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
                if (_direction == AxisDirection.Forward)
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

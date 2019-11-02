﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SierraHOTAS.Models
{
    public class HOTASAxisMap : IHotasBaseMap
    {
        public int MapId { get; set; }
        public string MapName { get; set; }
        public HOTASButtonMap.ButtonType Type { get; set; }
        public ObservableCollection<HOTASButtonMap> ButtonMap { get; set; }
        public bool IsDirectional { get; set; }
        public Dictionary<int, int> Segments { get; }

        public HOTASAxisMap()
        {
            Segments = new Dictionary<int, int>();
            ButtonMap = new ObservableCollection<HOTASButtonMap>();
        }

        public void CalculateSegmentRange(int segments)
        {
            var segmentRangeBoundary = ushort.MaxValue / (segments);
            Segments.Clear();
            for (var i = 1; i < segments; i++)
            {
                Segments.Add(i, (segmentRangeBoundary * i));
            }

            Segments.Add(segments, ushort.MaxValue);

            //try not to lose any macros already established. if more segments, then don't lose any macros. if less segments only trim from bottom of list
            if (segments < ButtonMap.Count)
            {
                for (var i = ButtonMap.Count; i > segments; i--)
                {
                    ButtonMap.RemoveAt(i-1);
                }
            }
            else
            {
                for (var i = ButtonMap.Count+1; i <= segments; i++)
                {
                    ButtonMap.Add(new HOTASButtonMap() { MapId = i, MapName = $"Axis Button {i}", Type = HOTASButtonMap.ButtonType.Button });
                }
            }
        }

        public void Clear()
        {
            Segments.Clear();
        }

        public int GetSegmentFromRawValue(int value)
        {
            var segmentRange = Segments.FirstOrDefault(r => value <= r.Value);
            return segmentRange.Key;
        }

        public HOTASButtonMap GetButtonMapFromRawValue(int value)
        {
            var segment = GetSegmentFromRawValue(value);
            var map = ButtonMap.FirstOrDefault(m => m.MapId == segment) as HOTASButtonMap;
            return map;
        }

        public void ClearUnassignedActions()
        {            
            foreach (var b in ButtonMap)
            {
                if (b.ActionName != "<No Action>") return;
                b.ActionName = string.Empty;
                b.Actions.Clear();
            }
        }
    }
}

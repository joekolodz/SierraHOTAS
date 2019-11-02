using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace SierraHOTAS.Models
{
    public class HOTASAxisMap : IHotasBaseMap
    {
        public int MapId { get; set; }
        public string MapName { get; set; }
        public HOTASButtonMap.ButtonType Type { get; set; }
        public Dictionary<int, HOTASButtonMap> MapRanges { get; set; }
        public bool IsDirectional { get; set; }
        public Dictionary<int, int> Segments { get; }

        public HOTASAxisMap()
        {
            Segments = new Dictionary<int, int>();
            MapRanges = new Dictionary<int, HOTASButtonMap>();
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
            if (segments < MapRanges.Count)
            {
                for (var i = MapRanges.Count - 1; i > segments; i--)
                {
                    MapRanges.Remove(i);
                }
            }
            else
            {
                for (var i = MapRanges.Count; i <= segments; i++)
                {
                    MapRanges.Add(i, new HOTASButtonMap(){MapId = MapId, MapName = $"Axis Button {i}", Type = HOTASButtonMap.ButtonType.Button});
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
            MapRanges.TryGetValue(segment, out var map);
            return map;
        }

        public void ClearUnassignedActions()
        {
            foreach (var b in MapRanges)
            {
                if (b.Value.ActionName != "<No Action>") return;
                b.Value.ActionName = string.Empty;
                b.Value.Actions.Clear();
            }
        }
    }
}

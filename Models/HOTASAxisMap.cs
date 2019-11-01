using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SierraHOTAS.Models
{
    public class HOTASAxisMap : IHotasBaseMap
    {
        public int MapId { get; set; }
        public string MapName { get; set; }
        public HOTASButtonMap.ButtonType Type { get; set; }
        public Dictionary<int, HOTASButtonMap> MapRanges { get; set; }
        public bool IsDirectional { get; set; }
        public int SegmentCount => _segmentRanges.Count;

        private readonly Dictionary<int, int> _segmentRanges;

        public HOTASAxisMap()
        {
            _segmentRanges = new Dictionary<int, int>();
            MapRanges = new Dictionary<int, HOTASButtonMap>();
        }

        public void CalculateSegmentRange(int segments)
        {
            var segmentRangeBoundary = ushort.MaxValue / (segments);
            _segmentRanges.Clear();
            for (var i = 1; i < segments; i++)
            {
                _segmentRanges.Add(i, (segmentRangeBoundary * i));
            }

            _segmentRanges.Add(segments, ushort.MaxValue);
        }

        public void Clear()
        {
            _segmentRanges.Clear();
        }

        public int GetSegmentFromRawValue(int value)
        {
            var segmentRange = _segmentRanges.FirstOrDefault(r => value <= r.Value);
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

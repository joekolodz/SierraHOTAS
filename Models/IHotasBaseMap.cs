namespace SierraHOTAS.Models
{
    public interface IHotasBaseMap
    {
        HOTASButtonMap.ButtonType Type { get; set; }
        string MapName { get; set; }
        int MapId { get; set; }
        void ClearUnassignedActions();
        void CalculateSegmentRange(int segments);
        void ClearSegments();
    }
}

using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModels
{
    public interface IBaseMapViewModel
    {
        int ButtonId { get; set; }
        string ButtonName { get; set; }
        HOTASButtonMap.ButtonType Type { get; set; }
        void SetAxis(int value);
        bool IsDisabledForced { get; set; }
        bool IsRecording { get; set; }
    }
}

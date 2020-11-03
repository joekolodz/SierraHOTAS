using SierraHOTAS.ModeProfileWindow.ViewModels;

namespace SierraHOTAS.ViewModels
{
    public class DeleteModeProfileEvent
    {
        public ModeActivationItem ActivationItem { get; set; }

        public DeleteModeProfileEvent(ModeActivationItem activationItem)
        {
            ActivationItem = activationItem;
        }
    }
}

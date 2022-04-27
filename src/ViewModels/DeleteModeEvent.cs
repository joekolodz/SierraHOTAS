using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModels
{
    public class DeleteModeEvent
    {
        public ModeActivationItem ActivationItem { get; set; }

        public DeleteModeEvent(ModeActivationItem activationItem)
        {
            ActivationItem = activationItem;
        }
    }
}

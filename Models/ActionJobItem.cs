using System.Collections.ObjectModel;

namespace SierraHOTAS.Models
{
    public class ActionJobItem
    {
        public int Offset { get; set; }
        public ObservableCollection<ButtonAction> Actions { get; set; }
    }
}

using SierraHOTAS.ViewModels;

namespace SierraHOTAS.Models
{
    public class AxisDirectionChangedEventArgs : System.EventArgs
    {
        public AxisDirection NewDirection { get; set; }
    }
}

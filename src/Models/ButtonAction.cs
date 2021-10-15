namespace SierraHOTAS.Models
{
    public class ButtonAction
    {
        public int ScanCode { get; set; }
        public bool IsKeyUp { get; set; }
        public bool IsExtended { get; set; }
        public int TimeInMilliseconds { get; set; }

        public ButtonAction()
        {
        }
    }
}

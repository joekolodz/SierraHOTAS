namespace SierraHOTAS.Models
{
    public class ButtonAction
    {
        public int ScanCode { get; set; }
        //public int Flags { get; set; } //KBDLLHOOKSTRUCTFlags
        public bool IsKeyUp { get; set; }
        public bool IsExtended { get; set; }

        public int TimeInMilliseconds { get; set; }

        public ButtonAction()
        {
        }
    }
}

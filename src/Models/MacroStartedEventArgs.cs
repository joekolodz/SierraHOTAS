namespace SierraHOTAS.Models
{
    public class MacroStartedEventArgs
    {
        public int Offset { get; set; }
        public int Code { get; set; }

        public MacroStartedEventArgs(int offset, int code)
        {
            Offset = offset;
            Code = code;
        }
    }
}

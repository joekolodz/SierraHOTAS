namespace SierraHOTAS.Models
{
    public class MacroCancelledEventArgs
    {
        public int Offset { get; set; }
        public int Code { get; set; }

        public MacroCancelledEventArgs(int offset, int code)
        {
            Offset = offset;
            Code = code;
        }
    }
}

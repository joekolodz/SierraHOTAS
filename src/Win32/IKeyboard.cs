namespace SierraHOTAS.Win32
{
    public interface IKeyboard
    {
        int KeyDownRepeatDelay { get; set; }
        bool IsKeySuppressionActive { get; set; }
        void SendKeyPress(int scanCode, bool isKeyUp, bool isExtended);
    }
}

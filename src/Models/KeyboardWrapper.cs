using SierraHOTAS.Win32;

namespace SierraHOTAS.Models
{
    public class KeyboardWrapper : IKeyboard
    {
        public int KeyDownRepeatDelay { get; set; } = 35;
        public bool IsKeySuppressionActive { get; set; }

        public virtual void SendKeyPress(int scanCode, bool isKeyUp, bool isExtended)
        {
            Keyboard.SendKeyPress(scanCode, isKeyUp, isExtended);
        }
    }
}

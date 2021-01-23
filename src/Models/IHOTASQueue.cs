using System;
using System.Collections.ObjectModel;
using SharpDX.DirectInput;

namespace SierraHOTAS.Models
{
    public interface IHOTASQueue
    {
        event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        event EventHandler<ButtonPressedEventArgs> ButtonReleased;
        event EventHandler<AxisChangedEventArgs> AxisChanged;
        event EventHandler<ModeProfileSelectedEventArgs> ModeProfileSelected;
        event EventHandler ShiftReleased;
        event EventHandler LostConnectionToDevice;
        void Listen(IJoystick joystick, ObservableCollection<IHotasBaseMap> buttonMap);
        void ForceButtonPress(JoystickOffset offset, bool isDown);
        void Stop();
        IHotasBaseMap GetMap(int buttonOffset);
        void SetButtonMap(ObservableCollection<IHotasBaseMap> buttonMap);
    }
}
using System;
using System.Collections.ObjectModel;
using SharpDX.DirectInput;

namespace SierraHOTAS.Models
{
    public interface IHOTASQueue
    {
        event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        event EventHandler<MacroCancelledEventArgs> MacroCancelled;
        event EventHandler<MacroStartedEventArgs> MacroStarted;
        event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        event EventHandler<ButtonPressedEventArgs> ButtonReleased;
        event EventHandler<AxisChangedEventArgs> AxisChanged;
        event EventHandler<ModeProfileSelectedEventArgs> ModeProfileSelected;
        event EventHandler<EventArgs> ShiftReleased;
        event EventHandler<EventArgs> LostConnectionToDevice;
        void Listen(IJoystick joystick, ObservableCollection<IHotasBaseMap> buttonMap);
        void ForceButtonPress(JoystickOffset offset, bool isDown);
        void Stop();
        IHotasBaseMap GetMap(int buttonOffset);
        void SetButtonMap(ObservableCollection<IHotasBaseMap> buttonMap);
    }
}
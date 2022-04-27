using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        event EventHandler<ModeSelectedEventArgs> ModeSelected;
        event EventHandler<EventArgs> ShiftReleased;
        event EventHandler<EventArgs> LostConnectionToDevice;
        void Listen(IJoystick joystick, Dictionary<int, ObservableCollection<IHotasBaseMap>> modes, Dictionary<int, ModeActivationItem> modeActivationButtons);
        void ForceButtonPress(JoystickOffset offset, bool isDown);
        void Stop();
        IHotasBaseMap GetMap(int buttonOffset);
        void SetButtonMap(ObservableCollection<IHotasBaseMap> buttonMap);
        void SetMode(int mode);
        void SetModes(Dictionary<int, ObservableCollection<IHotasBaseMap>> modes);
    }
}
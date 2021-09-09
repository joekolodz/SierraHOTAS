using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpDX.DirectInput;

namespace SierraHOTAS.Models
{
    public interface IHOTASDevice
    {
        event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        event EventHandler<MacroStartedEventArgs> MacroStarted;
        event EventHandler<MacroCancelledEventArgs> MacroCancelled;
        event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        event EventHandler<ModeProfileSelectedEventArgs> ModeProfileSelected;
        event EventHandler ShiftReleased;
        event EventHandler<AxisChangedEventArgs> AxisChanged;
        event EventHandler<LostConnectionToDeviceEventArgs> LostConnectionToDevice;

        Guid DeviceId { get; set; }

        Guid ProductId { get; set; }

        string Name { get; set; }
        Capabilities Capabilities { get; set; }
        bool IsDeviceLoaded { get; }
        ObservableCollection<IHotasBaseMap> ButtonMap { get; }
        Dictionary<int, ObservableCollection<IHotasBaseMap>> ModeProfiles { get; }

        void SetModeProfile(Dictionary<int, ObservableCollection<IHotasBaseMap>> profile);
        int SetupNewModeProfile();
        void CopyModeProfileFromTemplate(int templateModeSource, int destinationMode);
        //void ReAcquireJoystick();
        void ListenAsync();
        void SetButtonMap(ObservableCollection<IHotasBaseMap> buttonMap);
        void SetMode(int mode);
        void ForceButtonPress(JoystickOffset offset, bool isDown);
        void Stop();
        void ClearUnassignedActions();
        void ClearButtonMap();
        bool GetButtonState(int mapId);
    }
}

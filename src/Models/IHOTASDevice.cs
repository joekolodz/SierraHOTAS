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
        event EventHandler<ModeSelectedEventArgs> ModeSelected;
        event EventHandler<EventArgs> ShiftReleased;
        event EventHandler<AxisChangedEventArgs> AxisChanged;
        event EventHandler<LostConnectionToDeviceEventArgs> LostConnectionToDevice;

        Guid DeviceId { get; set; }

        Guid ProductId { get; set; }

        string Name { get; set; }
        Capabilities Capabilities { get; set; }
        bool IsDeviceLoaded { get; }
        ObservableCollection<IHotasBaseMap> ButtonMap { get; }
        Dictionary<int, ObservableCollection<IHotasBaseMap>> Modes { get; }

        void SetMode(Dictionary<int, ObservableCollection<IHotasBaseMap>> profile);
        int SetupNewMode();
        void CopyModeFromTemplate(int templateModeSource, int destinationMode);
        void ListenAsync();
        void OverlayAllModesToDevice();
        void OverlayAllModesToDevice(Dictionary<int, ObservableCollection<IHotasBaseMap>> profilesToMergeIn);
        void ApplyButtonMap(ObservableCollection<IHotasBaseMap> existingButtonMap);
        void SetMode(int mode);
        void ForceButtonPress(JoystickOffset offset, bool isDown);
        void Stop();
        void ClearUnassignedActions();
        void RemoveMode(int mode);
        void Reset();
        bool GetButtonState(int mapId);
        void SetModeActivation(Dictionary<int, ModeActivationItem> modeActivationButtons);
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SierraHOTAS.Models
{
    public interface IHOTASCollection
    {
        event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        event EventHandler<MacroStartedEventArgs> MacroStarted;
        event EventHandler<MacroCancelledEventArgs> MacroCancelled;
        event EventHandler<RepeatStartedEventArgs> RepeatStarted;
        event EventHandler<RepeatCancelledEventArgs> RepeatCancelled;
        event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        event EventHandler<AxisChangedEventArgs> AxisChanged;
        event EventHandler<ModeChangedEventArgs> ModeChanged;
        event EventHandler<LostConnectionToDeviceEventArgs> LostConnectionToDevice;
        ObservableCollection<IHOTASDevice> Devices { get; set; }
        int Mode { get; set; }
        ActionCatalog ActionCatalog { get; }
        Dictionary<int, ModeActivationItem> ModeActivationButtons { get; }
        void SetCatalog(ActionCatalog catalog);
        IHOTASDevice AddDevice(IHOTASDevice device);
        /// <summary>
        /// device is replaced based on the DeviceId
        /// </summary>
        /// <param name="newDevice"></param>
        void ReplaceDevice(IHOTASDevice newDevice);
        void Start();
        void Stop();
        void ResetProfile();
        void ApplyButtonMapToAllProfiles();
        void ListenToAllDevices();
        void ListenToDevice(IHOTASDevice device);
        int SetupNewMode();
        void CopyModeFromTemplate(int templateModeSource, int destinationMode);
        void ForceButtonPress(IHOTASDevice device, JoystickOffset offset, bool isDown);
        IHOTASDevice GetDevice(Guid instanceId);
        void ClearUnassignedActions();
        ObservableCollection<IHOTASDevice> RefreshMissingDevices();

        /// <summary>
        /// Activate the profile for the given Mode
        /// </summary>
        /// <param name="mode"></param>
        void SetMode(int mode);

        /// <summary>
        /// Automatically determine if a Mode button is selected. If so, activate that Mode's profile.
        /// </summary>
        void AutoSetMode();

        bool RemoveMode(ModeActivationItem item);

        /// <summary>
        /// The activation button should be applied across all profiles in the set to ensure the new mode can be reached from any device.
        /// </summary>
        void ApplyActivationButtonToAllProfiles();
    }
}
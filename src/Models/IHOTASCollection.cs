using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpDX.DirectInput;
using SierraHOTAS.ModeProfileWindow.ViewModels;

namespace SierraHOTAS.Models
{
    public interface IHOTASCollection
    {
        event EventHandler<KeystrokeSentEventArgs> KeystrokeDownSent;
        event EventHandler<KeystrokeSentEventArgs> KeystrokeUpSent;
        event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        event EventHandler<AxisChangedEventArgs> AxisChanged;
        event EventHandler<ModeProfileChangedEventArgs> ModeProfileChanged;
        event EventHandler<LostConnectionToDeviceEventArgs> LostConnectionToDevice;
        ObservableCollection<HOTASDevice> Devices { get; set; }
        int Mode { get; set; }
        Dictionary<int, ModeActivationItem> ModeProfileActivationButtons { get; }
        void AddDevice(HOTASDevice device);
        /// <summary>
        /// device is replaced based on the DeviceId
        /// </summary>
        /// <param name="newDevice"></param>
        void ReplaceDevice(HOTASDevice newDevice);
        void Start();
        void Stop();
        void ClearButtonMap();
        void ListenToAllDevices();
        void ListenToDevice(HOTASDevice device);
        int SetupNewModeProfile();
        void CopyModeProfileFromTemplate(int templateModeSource, int destinationMode);
        void ForceButtonPress(HOTASDevice device, JoystickOffset offset, bool isDown);
        HOTASDevice GetDevice(Guid instanceId);
        void ClearUnassignedActions();
        ObservableCollection<HOTASDevice> GetHOTASDevices();

        /// <summary>
        /// Activate the profile for the given Mode
        /// </summary>
        /// <param name="mode"></param>
        void SetMode(int mode);

        /// <summary>
        /// Automatically determine if a Mode button is selected. If so, activate that Mode's profile.
        /// </summary>
        void AutoSetMode();

        bool RemoveModeProfile(ModeActivationItem item);

        /// <summary>
        /// The activation button should be applied across all profiles in the set to ensure the new mode can be reached from any device.
        /// </summary>
        void ApplyActivationButtonToAllProfiles();
    }
}
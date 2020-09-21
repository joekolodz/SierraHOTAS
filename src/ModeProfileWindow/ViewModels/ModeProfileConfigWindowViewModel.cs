using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace SierraHOTAS.ModeProfileWindow.ViewModels
{
    public class ModeProfileConfigWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EventArgs> NewModeSavedEventHandler;

        public string ProfileName { get; set; }
        public string DeviceName { get; set; }
        public string ActivationButtonName { get; set; }
        public bool IsActivationErrorVisible { get; set; }
        public Dispatcher AppDispatcher { get; set; }

        private int _activationButtonId;
        private Guid _deviceId;
        private ModeActivationItem _activationItem;
        private bool _isActivationButtonValid = false;
        private readonly int _mode;
        private readonly Dictionary<int, ModeActivationItem> _activationButtonList;
        private HOTASButtonMap _buttonMap;

        private CommandHandler _createNewModeProfileCommand;
        public ICommand SaveNewModeProfileCommand => _createNewModeProfileCommand ?? (_createNewModeProfileCommand = new CommandHandler(SaveNewModeProfile, CanExecuteSaveNewMode));

        public ModeProfileConfigWindowViewModel()
        {
        }

        public ModeProfileConfigWindowViewModel(int mode, Dictionary<int, ModeActivationItem> activationButtonList)
        {
            _mode = mode;
            _activationButtonList = activationButtonList;
        }

        public void DeviceList_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!(e.Device.ButtonMap.FirstOrDefault(m => m.MapId == e.ButtonId) is HOTASButtonMap map))
            {
                Logging.Log.Info($"Couldn't find a button map. No mode activation has been set.");
                return;
            }

            Logging.Log.Info($"{e.ButtonId} from {e.Device.Name} - {sender}; ShiftMode:{map.ShiftModePage}");

            _buttonMap = map;
            DeviceName = e.Device.Name;
            _deviceId = e.Device.DeviceId;
            ActivationButtonName = map.MapName;
            _activationButtonId = map.MapId;
            ValidateActivationButton();

            AppDispatcher?.Invoke(() =>
            {
                OnPropertyChanged(nameof(DeviceName));
                OnPropertyChanged(nameof(ActivationButtonName));
                OnPropertyChanged(nameof(IsActivationErrorVisible));
                _createNewModeProfileCommand.ForceCanExecuteChanged();
            });
        }

        private void SaveNewModeProfile()
        {
            _buttonMap.ShiftModePage = _mode;
            _activationItem = new ModeActivationItem()
            {
                Mode = _mode,
                ProfileName = ProfileName,
                DeviceName = DeviceName,
                DeviceId = _deviceId,
                ButtonName = ActivationButtonName,
                ButtonId = _activationButtonId
            };
            _activationButtonList.Add(_mode, _activationItem);

            NewModeSavedEventHandler?.Invoke(this, new EventArgs());
            Logging.Log.Info($"Profile name: {ProfileName}, Device: {DeviceName}, Button: {_activationButtonId}");
        }

        private void ValidateActivationButton()
        {
            Logging.Log.Debug($"canexecute {_buttonMap?.ShiftModePage}");

            _isActivationButtonValid = _buttonMap != null;

            foreach (var mapId in _activationButtonList)
            {
                if (mapId.Value.DeviceId != _deviceId || mapId.Value.ButtonId != _activationButtonId) continue;
                _isActivationButtonValid = false;
                break;
            }

            IsActivationErrorVisible = !_isActivationButtonValid;
        }
        private bool CanExecuteSaveNewMode()
        {
            return _isActivationButtonValid;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

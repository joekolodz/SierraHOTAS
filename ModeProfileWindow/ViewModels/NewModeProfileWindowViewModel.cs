using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels.Commands;

namespace SierraHOTAS.ModeProfileWindow.ViewModels
{
    public class NewModeProfileWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EventArgs> NewModeSavedEventHandler;

        public string ProfileName { get; set; }
        public string DeviceName { get; set; }
        public Guid DeviceId { get; set; }
        public string ActivationButtonName { get; set; }
        public int ActivationButtonId { get; set; }
        public bool IsActivationErrorVisible { get; set; }
        public ModeActivationItem ActivationItem { get; set; }

        private bool _isActivationButtonValid = false;

        public Dispatcher AppDispatcher { get; set; }

        private readonly int _mode;
        private readonly Dictionary<int, ModeActivationItem> _activationButtonList;
        private HOTASButtonMap _buttonMap;

        private CommandHandler _createNewModeProfileCommand;
        public ICommand SaveNewModeProfileCommand => _createNewModeProfileCommand ?? (_createNewModeProfileCommand = new CommandHandler(SaveNewModeProfile, CanExecuteSaveNewMode));

        public NewModeProfileWindowViewModel()
        {
        }

        public NewModeProfileWindowViewModel(int mode, Dictionary<int, ModeActivationItem> activationButtonList)
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
            DeviceId = e.Device.DeviceId;
            ActivationButtonId = _buttonMap.MapId;
            ActivationButtonName = map.MapName;
            ActivationButtonId = map.MapId;
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
            ActivationItem = new ModeActivationItem()
            {
                Mode = _mode,
                ProfileName = ProfileName,
                DeviceName = DeviceName,
                DeviceId = DeviceId,
                ButtonName = ActivationButtonName,
                ButtonId = ActivationButtonId
            };
            _activationButtonList.Add(_mode, ActivationItem);

            NewModeSavedEventHandler?.Invoke(this, new EventArgs());
        }

        private void ValidateActivationButton()
        {
            Logging.Log.Debug($"canexecute {_buttonMap?.ShiftModePage}");

            _isActivationButtonValid = _buttonMap != null;

            foreach (var mapId in _activationButtonList)
            {
                if (mapId.Value.DeviceId != DeviceId || mapId.Value.ButtonId != ActivationButtonId) continue;
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

using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SierraHOTAS.ViewModels
{
    public class ModeConfigWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EventArgs> NewModeSaved;
        public event EventHandler<EventArgs> SaveCancelled;

        public string ModeName
        {
            get => _modeName;
            set
            {
                if (value == _modeName) return;
                _modeName = value;
                OnPropertyChanged();
            }
        }

        public string DeviceName
        {
            get => _deviceName;
            set
            {
                if (value == _deviceName) return;
                _deviceName = value;
                OnPropertyChanged();
            }
        }

        public string ActivationButtonName
        {
            get => _activationButtonName;
            set
            {
                if (value == _activationButtonName) return;
                _activationButtonName = value;
                OnPropertyChanged();
            }
        }

        public bool IsActivationErrorVisible
        {
            get => _isActivationErrorVisible;
            set
            {
                if (value == _isActivationErrorVisible) return;
                _isActivationErrorVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsShift
        {
            get => _isShift;
            set
            {
                if (value == _isShift) return;
                _isShift = value;
                OnPropertyChanged();
            }
        }

        public int CopyTemplateMode
        {
            get => _copyTemplateMode;
            set
            {
                if (value == _copyTemplateMode) return;
                _copyTemplateMode = value;
                OnPropertyChanged();
            }
        }

        public int InheritFromMode
        {
            get => _inheritFromMode;
            set
            {
                if (value == _inheritFromMode) return;
                _inheritFromMode = value;
                OnPropertyChanged();
            }
        }

        public bool IsInheritedVisible => _mode != 1;
        public bool IsShiftVisible => _mode != 1;

        public bool IsTemplateModeVisible => _mode != 1 && _activationButtonList != null && !_activationButtonList.ContainsKey(_mode);

        public Dictionary<int, string> TemplateModes { get; set; }
        public Dictionary<int, string> InheritModes { get; set; }

        private readonly IDispatcher _appDispatcher;
        private int _activationButtonId;
        private Guid _deviceId;
        private ModeActivationItem _activationItem;
        private bool _isActivationButtonValid = false;
        private string _modeName;
        private string _deviceName;
        private string _activationButtonName;
        private bool _isActivationErrorVisible;
        private bool _isShift;
        private int _inheritFromMode;
        private int _copyTemplateMode;
        private readonly int _mode;
        private readonly Dictionary<int, ModeActivationItem> _activationButtonList;
        private HOTASButton _button;
        private IEventAggregator _eventAggregator;

        private CommandHandler _saveModeCommand;
        public ICommand SaveModeCommand => _saveModeCommand ?? (_saveModeCommand = new CommandHandler(SaveMode, CanExecuteSaveMode));

        private CommandHandler _cancelCommand;

        public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new CommandHandler(Cancel));

        public ModeConfigWindowViewModel()
        {
        }

        public ModeConfigWindowViewModel(IEventAggregator eventAggregator, IDispatcher appDispatcher, int mode, Dictionary<int, ModeActivationItem> activationButtonList)
        {
            _appDispatcher = appDispatcher ?? throw new ArgumentNullException(nameof(appDispatcher));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _activationButtonList = activationButtonList ?? throw new ArgumentNullException(nameof(activationButtonList));
            _mode = mode;

            BuildTemplateList();
            BuildInheritList();

            //if there is no activation item, then we are creating a new one for the mode supplied. there will be nothing to assign here.
            if (!_activationButtonList.TryGetValue(_mode, out _activationItem)) return;

            ModeName = _activationItem.ModeName;
            DeviceName = _activationItem.DeviceName;
            ActivationButtonName = _activationItem.ButtonName;
            IsShift = _activationItem.IsShift;
            InheritFromMode = _activationItem.InheritFromMode;
            _deviceId = _activationItem.DeviceId;
            _activationButtonId = _activationItem.ButtonId;

            _isActivationButtonValid = true;

            _appDispatcher.Invoke(() =>
            {
                ((CommandHandler)SaveModeCommand).ForceCanExecuteChanged();
            });
        }

        public void DeviceList_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //get the button map from the device on which the button was pressed
            if (!(e.Device.ButtonMap.FirstOrDefault(m => m.MapId == e.ButtonId) is HOTASButton map))
            {
                Logging.Log.Info($"Couldn't find a button map. No mode activation has been set.");
                return;
            }

            Logging.Log.Info($"{e.ButtonId} from {e.Device.Name} - {sender}; ShiftMode:{map.ShiftModePage}; IsShift:{map.IsShift}");

            _button = map;
            DeviceName = e.Device.Name;
            _deviceId = e.Device.DeviceId;
            ActivationButtonName = map.MapName;
            _activationButtonId = map.MapId;
            ValidateActivationButton();

            _appDispatcher?.Invoke(() =>
            {
                OnPropertyChanged(nameof(TemplateModes));
                OnPropertyChanged(nameof(InheritModes));
                _saveModeCommand.ForceCanExecuteChanged();
            });
        }

        private void BuildTemplateList()
        {
            TemplateModes = new Dictionary<int, string> { { 0, "- Empty -" } };
            foreach (var kv in _activationButtonList)
            {
                TemplateModes.Add(kv.Key, kv.Value.ModeName);
            }
        }

        private void BuildInheritList()
        {
            InheritModes = new Dictionary<int, string> { { 0, "- Do Not Inherit -" } };
            foreach (var kv in _activationButtonList)
            {
                if (kv.Key == _mode) continue;
                InheritModes.Add(kv.Key, kv.Value.ModeName);
            }
        }

        private void SaveMode()
        {
            if (_activationButtonList.ContainsKey(_mode))
            {
                var isRemoved = _activationButtonList.Remove(_mode);
                if (isRemoved)
                {
                    _eventAggregator.Publish(new DeleteModeEvent(_activationItem));
                }
            }

            if (_button != null)
            {
                _button.ShiftModePage = _mode;
            }

            _activationItem = new ModeActivationItem()
            {
                Mode = _mode,
                InheritFromMode = _inheritFromMode,
                IsShift = _isShift,
                ModeName = _modeName,
                DeviceName = _deviceName,
                DeviceId = _deviceId,
                ButtonName = _activationButtonName,
                ButtonId = _activationButtonId,
                TemplateMode = _copyTemplateMode
            };

            _activationButtonList.Add(_mode, _activationItem);

            Logging.Log.Info($"Mode name: {ModeName}, Device: {DeviceName}, Button: {_activationButtonId}");

            NewModeSaved?.Invoke(this, new EventArgs());
        }

        private void Cancel()
        {
            SaveCancelled?.Invoke(this, new EventArgs());
        }

        private void ValidateActivationButton()
        {
            Logging.Log.Debug($"canexecute {_button?.ShiftModePage}");

            _isActivationButtonValid = _button != null;

            foreach (var mapId in _activationButtonList)
            {
                if (mapId.Value == _activationItem) continue;
                if (mapId.Value.DeviceId != _deviceId || mapId.Value.ButtonId != _activationButtonId) continue;
                _isActivationButtonValid = false;
                break;
            }

            IsActivationErrorVisible = !_isActivationButtonValid;
        }
        private bool CanExecuteSaveMode()
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

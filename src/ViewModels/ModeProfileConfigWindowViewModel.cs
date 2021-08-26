using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using SierraHOTAS.ModeProfileWindow.ViewModels;
using SierraHOTAS.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SierraHOTAS.ViewModels
{
    public class ModeProfileConfigWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EventArgs> NewModeProfileSaved;
        public event EventHandler<EventArgs> SaveCancelled;

        public string ProfileName
        {
            get => _profileName;
            set
            {
                if (value == _profileName) return;
                _profileName = value;
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

        public bool IsShiftVisible
        {
            get => _mode != 1;
            set
            {
            }
        }

        public bool IsTemplateModeVisible
        {
            get => _isTemplateModeVisible;
            set
            {
                if (value == _isTemplateModeVisible) return;
                _isTemplateModeVisible = value;
                OnPropertyChanged();
            }
        }
        public Dictionary<int, string> TemplateModes { get; set; }

        private readonly IDispatcher _appDispatcher;
        private int _activationButtonId;
        private Guid _deviceId;
        private ModeActivationItem _activationItem;
        private bool _isActivationButtonValid = false;
        private string _profileName;
        private string _deviceName;
        private string _activationButtonName;
        private bool _isActivationErrorVisible;
        private bool _isShift;
        private bool _isTemplateModeVisible = true;
        private int _selectedTemplateMode;
        private readonly int _mode;
        private readonly Dictionary<int, ModeActivationItem> _activationButtonList;
        private HOTASButton _button;
        private IEventAggregator _eventAggregator;

        private CommandHandler _saveModeProfileCommand;
        public ICommand SaveModeProfileCommand => _saveModeProfileCommand ?? (_saveModeProfileCommand = new CommandHandler(SaveModeProfile, CanExecuteSaveMode));

        private CommandHandler _cancelCommand;

        public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new CommandHandler(Cancel));

        public ModeProfileConfigWindowViewModel()
        {
        }

        public ModeProfileConfigWindowViewModel(IEventAggregator eventAggregator, IDispatcher appDispatcher, int mode, Dictionary<int, ModeActivationItem> activationButtonList)
        {
            _appDispatcher = appDispatcher ?? throw new ArgumentNullException(nameof(appDispatcher));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _activationButtonList = activationButtonList ?? throw new ArgumentNullException(nameof(activationButtonList));
            _mode = mode;

            BuildTemplateList();
            IsTemplateModeVisible = TemplateModes.Count > 1;

            //if there is no activation item, then we are creating a new one for the mode supplied. there will be nothing to assign here.
            if (!_activationButtonList.TryGetValue(_mode, out _activationItem)) return;

            ProfileName = _activationItem.ProfileName;
            DeviceName = _activationItem.DeviceName;
            ActivationButtonName = _activationItem.ButtonName;
            IsShift = _activationItem.IsShift;
            _deviceId = _activationItem.DeviceId;
            _activationButtonId = _activationItem.ButtonId;

            _isActivationButtonValid = true;
            IsTemplateModeVisible = false;

            _appDispatcher.Invoke(() =>
            {
                ((CommandHandler)SaveModeProfileCommand).ForceCanExecuteChanged();
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
                _saveModeProfileCommand.ForceCanExecuteChanged();
            });
        }

        private void BuildTemplateList()
        {
            TemplateModes = new Dictionary<int, string> { { 0, "- Empty -" } };
            foreach (var kv in _activationButtonList)
            {
                TemplateModes.Add(kv.Key, kv.Value.ProfileName);
            }
        }

        private void SaveModeProfile()
        {
            if (_activationButtonList.ContainsKey(_mode))
            {
                var isRemoved = _activationButtonList.Remove(_mode);
                if (isRemoved)
                {
                    _eventAggregator.Publish(new DeleteModeProfileEvent(_activationItem));
                }
            }

            if (_button != null)
            {
                _button.ShiftModePage = _mode;
            }

            _activationItem = new ModeActivationItem()
            {
                Mode = _mode,
                IsShift = IsShift,
                ProfileName = ProfileName,
                DeviceName = DeviceName,
                DeviceId = _deviceId,
                ButtonName = ActivationButtonName,
                ButtonId = _activationButtonId,
                TemplateMode = _selectedTemplateMode
            };

            _activationButtonList.Add(_mode, _activationItem);

            Logging.Log.Info($"Profile name: {ProfileName}, Device: {DeviceName}, Button: {_activationButtonId}");

            NewModeProfileSaved?.Invoke(this, new EventArgs());
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

        public void TemplateModeSelected(object sender, RoutedEventArgs routedEventArgs)
        {
            if ((routedEventArgs is SelectionChangedEventArgs args) && args.AddedItems.Count > 0)
            {
                Logging.Log.Info($"{args.AddedItems[0]}");
                var selectedItem = (KeyValuePair<int, string>)args.AddedItems[0];
                _selectedTemplateMode = selectedItem.Key;
            }
        }
    }
}

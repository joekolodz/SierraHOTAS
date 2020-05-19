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
        public Guid DeviceInstanceId { get; set; }
        public string ActivationButtonName { get; set; }
        public int ActivationButtonId { get; set; }
        public bool IsActivationErrorVisible { get; set; }
        public ObservableCollection<ModeActivationItem> ModeActivationItems { get; set; }

        private bool _isActivationButtonValid = true;

        public Dispatcher AppDispatcher { get; set; }

        private int _mode;
        private readonly Dictionary<int, (Guid, int)> _activationButtonList;
        private HOTASButtonMap _buttonMap;

        private CommandHandler _createNewModeProfileCommand;
        public ICommand SaveNewModeProfileCommand => _createNewModeProfileCommand ?? (_createNewModeProfileCommand = new CommandHandler(SaveNewModeProfile, CanExecuteSaveNewMode));

        public NewModeProfileWindowViewModel()
        {
        }

        public NewModeProfileWindowViewModel(int mode, Dictionary<int, (Guid, int)> activationButtonList, HOTASCollection devices)
        {
            _mode = mode;
            _activationButtonList = activationButtonList;

            //todo: show the list of profiles with their name and activation button
            ModeActivationItems = new ObservableCollection<ModeActivationItem>();
            BuildActivationButtonGrid();
        }

        public void DeviceList_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!(e.Device.ButtonMap.FirstOrDefault(m => m.MapId == e.ButtonId) is HOTASButtonMap map))
            {
                Logging.Log.Info($"Couldn't find a button map. No mode activation has been set.");
                return;
            }

            //HOTASButtonMap map = null;
            //foreach (var m in e.Device.ButtonMap)
            //{
            //    if (m.MapId != e.ButtonId) continue;
            //    map = (HOTASButtonMap)m;
            //    break;
            //}
            //if (map == null) return;

            Logging.Log.Info($"{e.ButtonId} from {e.Device.Name} - {sender}; ShiftMode:{map.ShiftModePage}");

            _buttonMap = map;
            DeviceName = e.Device.Name;
            DeviceInstanceId = e.Device.InstanceId;
            ActivationButtonId = _buttonMap.MapId;
            ActivationButtonName = map.MapName;
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
            NewModeSavedEventHandler?.Invoke(this, new EventArgs());
        }

        private void ValidateActivationButton()
        {
            Logging.Log.Debug($"canexecute {_buttonMap?.ShiftModePage}");

            _isActivationButtonValid = true;
            foreach (var mapId in _activationButtonList)
            {
                if (mapId.Value.Item1 != DeviceInstanceId || mapId.Value.Item2 != ActivationButtonId) continue;
                _isActivationButtonValid = false;
                break;
            }

            IsActivationErrorVisible = !_isActivationButtonValid;
        }
        private bool CanExecuteSaveNewMode()
        {
            return _isActivationButtonValid;
        }

        private void BuildActivationButtonGrid()
        {
            foreach (var a in _activationButtonList)
            {
                var item = new ModeActivationItem()
                {
                    Mode = a.Key.ToString(),
                    DeviceName = a.Value.Item1.ToString(),
                    ActivationButtonName = a.Value.Item2.ToString()
                };
                ModeActivationItems.Add(item);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

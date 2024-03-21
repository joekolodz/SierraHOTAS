using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SierraHOTAS.ViewModels
{
    public class ButtonMapViewModel : IBaseMapViewModel, INotifyPropertyChanged
    {
        private readonly HOTASButton _hotasButton;

        public ActionCatalogItem ActionItem
        {
            get => _hotasButton.ActionCatalogItem;
            set => _hotasButton.ActionCatalogItem = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<EventArgs> RecordingStarted;
        public event EventHandler<EventArgs> RecordingStopped;
        public event EventHandler<EventArgs> RecordingCancelled;

        public int ButtonId
        {
            get => _hotasButton.MapId;
            set => _hotasButton.MapId = value;
        }

        public string ButtonName
        {
            get => _hotasButton.MapName;
            set
            {
                if (_hotasButton.MapName == value) return;
                _hotasButton.MapName = value;
                OnPropertyChanged(nameof(ButtonName));
            }
        }

        public HOTASButton.ButtonType Type
        {
            get => _hotasButton.Type;
            set => _hotasButton.Type = value;
        }

        public bool IsOneShot
        {
            get => _hotasButton.IsOneShot;
            set
            {
                if (_hotasButton.IsOneShot == value) return;
                _hotasButton.IsOneShot = value;
                OnPropertyChanged(nameof(IsOneShot));
            }
        }

        public int RepeatCount
        {
            get => _hotasButton.RepeatCount;
            set
            {
                if (_hotasButton.RepeatCount == value) return;
                _hotasButton.RepeatCount = value;
                OnPropertyChanged(nameof(RepeatCount));
            }
        }

        public string ActionName
        {
            get => ActionItem.ActionName;
            set
            {
                if (ActionItem.ActionName == value) return;
                Logging.Log.Info($"about to change ActionName from: {ActionItem.ActionName} to: {value}");
                ActionItem.ActionName = value;
                OnPropertyChanged(nameof(ActionName));
            }
        }

        public ObservableCollection<ButtonActionViewModel> Actions { get; set; }

        private CommandHandler _recordMacroStartCommand;
        public ICommand RecordMacroStartCommand => _recordMacroStartCommand ?? (_recordMacroStartCommand = new CommandHandler(RecordMacroStart, RecordMacroStartCanExecute));


        private CommandHandler _recordMacroStopCommand;
        public ICommand RecordMacroStopCommand => _recordMacroStopCommand ?? (_recordMacroStopCommand = new CommandHandler(RecordMacroStop, RecordMacroStopCanExecute));

        
        private CommandHandler _recordMacroCancelCommand;
        public ICommand RecordMacroCancelCommand => _recordMacroCancelCommand ?? (_recordMacroCancelCommand = new CommandHandler(RecordMacroCancel, RecordMacroCancelCanExecute));


        public bool IsDisabledForced { get; set; }

        public bool IsRecording { get; set; }

        public int ActivateShiftModePage
        {
            get => _hotasButton.ShiftModePage;
            set
            {
                if (_hotasButton.ShiftModePage == value) return;
                Logging.Log.Info($"about to change Mode from: {_hotasButton.ShiftModePage} to: {value}");
                _hotasButton.ShiftModePage = value;
                OnPropertyChanged(nameof(ActivateShiftModePage));
            }
        }

        public ButtonMapViewModel()
        {
            _hotasButton = new HOTASButton();
            Actions = new ObservableCollection<ButtonActionViewModel>();
            ActionItem = new ActionCatalogItem();
            AddHandlers();
        }

        public ButtonMapViewModel(HOTASButton button)
        {
            _hotasButton = button;
            IsRecording = false;
            IsDisabledForced = false;
            Actions = new ObservableCollection<ButtonActionViewModel>();
            ActionItem = button.ActionCatalogItem;
            AddHandlers();
            BuildButtonActionViewModel(button.ActionCatalogItem.Actions);
        }

        public override string ToString()
        {
            return $"ButtonMap MapId:{_hotasButton.MapId}, {_hotasButton.MapName}";
        }

        public ObservableCollection<ButtonAction> GetHotasActions()
        {
            return ActionItem.Actions;
        }

        public void AssignActions(ActionCatalogItem actionCatalogItem)
        {
            if (actionCatalogItem.NoAction)
            {
                actionCatalogItem = new ActionCatalogItem();
            }

            RemoveHandlers();
            ActionItem = actionCatalogItem;
            AddHandlers();

            ReBuildButtonActionViewModel();
            OnPropertyChanged(nameof(ActionName));
        }

        private void AddHandlers()
        {
            ActionItem.Actions.CollectionChanged += Actions_CollectionChanged;
            ActionItem.PropertyChanged += ActionItem_PropertyChanged;
        }

        private void RemoveHandlers()
        {
            ActionItem.Actions.CollectionChanged -= Actions_CollectionChanged;
            ActionItem.PropertyChanged -= ActionItem_PropertyChanged;
        }

        private void ActionItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        private void Actions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ReBuildButtonActionViewModel();
        }

        public void ReBuildButtonActionViewModel()
        {
            BuildButtonActionViewModel(ActionItem.Actions);
        }

        private void BuildButtonActionViewModel(ObservableCollection<ButtonAction> actions)
        {
            Actions.Clear();
            foreach (var a in actions)
            {
                Actions.Add(new ButtonActionViewModel(a));
            }
        }

        private void RecordMacroStart()
        {
            if (IsDisabledForced) return;

            Actions.Clear();

            _hotasButton.Record();

            IsRecording = true;
            RecordingStarted?.Invoke(this, EventArgs.Empty);
        }

        private bool RecordMacroStartCanExecute()
        {
            if (IsDisabledForced) return false;
            return !IsRecording;
        }

        private void RecordMacroStop()
        {
            if (IsDisabledForced) return;

            _hotasButton.Stop();

            ReBuildButtonActionViewModel();

            IsRecording = false;
            RecordingStopped?.Invoke(this, EventArgs.Empty);
        }

        private bool RecordMacroStopCanExecute()
        {
            if (IsDisabledForced) return false;
            return IsRecording;
        }
        private void RecordMacroCancel()
        {
            if (IsDisabledForced) return;

            _hotasButton.Cancel();

            ReBuildButtonActionViewModel();

            IsRecording = false;
            RecordingCancelled?.Invoke(this, EventArgs.Empty);
        }

        private bool RecordMacroCancelCanExecute()
        {
            if (IsDisabledForced) return false;
            return IsRecording;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetAxis(int value)
        {
            return;//not implemented for normal button maps, only radial and linear
        }
    }
}

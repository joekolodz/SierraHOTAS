using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using SierraHOTAS.Annotations;
using SierraHOTAS.ViewModels.Commands;
using SharpDX.DirectInput;
using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModels
{
    public class ButtonMapViewModel : IBaseMapViewModel, INotifyPropertyChanged
    {
        private readonly HOTASButtonMap _hotasButtonMap;

        public ActionCatalogItem ActionItem
        {
            get => _hotasButtonMap.ActionCatalogItem; 
            set => _hotasButtonMap.ActionCatalogItem = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler RecordingStarted;
        public event EventHandler RecordingStopped;
        public event EventHandler RecordingCancelled;

        public int ButtonId
        {
            get => _hotasButtonMap.MapId;
            set => _hotasButtonMap.MapId = value;
        }

        public string ButtonName
        {
            get => _hotasButtonMap.MapName;
            set
            {
                if (_hotasButtonMap.MapName == value) return;
                _hotasButtonMap.MapName = value;
                OnPropertyChanged(nameof(ButtonName));
            }
        }

        public HOTASButtonMap.ButtonType Type
        {
            get => _hotasButtonMap.Type;
            set => _hotasButtonMap.Type = value;
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

        public RelayCommandWithParameter RecordMacroStartCommandWithParameter { get; set; }

        public RelayCommandWithParameter RecordMacroStopCommandWithParameter { get; set; }

        public RelayCommandWithParameter RecordMacroCancelCommandWithParameter { get; set; }

        public bool IsDisabledForced { get; set; }

        public bool IsRecording { get; set; }

        public int ActivateShiftModePage
        {
            get => _hotasButtonMap.ShiftModePage;
            set
            {
                if (_hotasButtonMap.ShiftModePage == value) return;
                Logging.Log.Info($"about to change Mode from: {_hotasButtonMap.ShiftModePage} to: {value}");
                _hotasButtonMap.ShiftModePage = value;
                OnPropertyChanged(nameof(ActivateShiftModePage));
            }
        }

        public ButtonMapViewModel()
        {
            _hotasButtonMap = new HOTASButtonMap();
            Actions = new ObservableCollection<ButtonActionViewModel>();
            ActionItem = new ActionCatalogItem();
            AddHandlers();
        }

        public ButtonMapViewModel(HOTASButtonMap buttonMap)
        {
            _hotasButtonMap = buttonMap;

            RecordMacroStartCommandWithParameter = new RelayCommandWithParameter(RecordMacroStart, RecordMacroStartCanExecute);
            RecordMacroStopCommandWithParameter = new RelayCommandWithParameter(RecordMacroStop, RecordMacroStopCanExecute);
            RecordMacroCancelCommandWithParameter = new RelayCommandWithParameter(RecordMacroCancel, RecordMacroCancelCanExecute);
            IsRecording = false;
            IsDisabledForced = false;
            Actions = new ObservableCollection<ButtonActionViewModel>();
            ActionItem = buttonMap.ActionCatalogItem;
            AddHandlers();
            BuildButtonActionViewModel(buttonMap.ActionCatalogItem.Actions);
        }

        public override string ToString()
        {
            return $"ButtonMap MapId:{_hotasButtonMap.MapId}, {_hotasButtonMap.MapName}";
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

        private void RecordMacroStart(object parameter)
        {
            if (IsDisabledForced) return;

            Actions.Clear();

            _hotasButtonMap.Record();

            IsRecording = true;
            Debug.WriteLine($"RECORDING {ButtonName}...");
            RecordingStarted?.Invoke(this, EventArgs.Empty);
        }

        private bool RecordMacroStartCanExecute(object parameter)
        {
            if (IsDisabledForced) return false;
            return !IsRecording;
        }

        private void RecordMacroStop(object parameter)
        {
            if (IsDisabledForced) return;

            _hotasButtonMap.Stop();

            ReBuildButtonActionViewModel();

            IsRecording = false;
            Debug.WriteLine($"STOPPED - Recorded==>{_hotasButtonMap}");
            RecordingStopped?.Invoke(this, EventArgs.Empty);
        }

        private bool RecordMacroStopCanExecute(object parameter)
        {
            if (IsDisabledForced) return false;
            return IsRecording;
        }
        private void RecordMacroCancel(object parameter)
        {
            if (IsDisabledForced) return;

            _hotasButtonMap.Cancel();

            ReBuildButtonActionViewModel();

            IsRecording = false;
            Debug.WriteLine("CANCELLED");
            RecordingCancelled?.Invoke(this, EventArgs.Empty);
        }

        private bool RecordMacroCancelCanExecute(object parameter)
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
            return;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModel.Commands;
using SharpDX.DirectInput;
using SierraHOTAS.ViewModels;

namespace SierraHOTAS.ViewModel
{
    public class ButtonMapViewModel : IBaseMapViewModel, INotifyPropertyChanged
    {
        private readonly HOTASButtonMap _hotasButtonMap;
        public ActionCatalogItem ActionItem { get; set; }

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
                _hotasButtonMap.ActionName = value;
                OnPropertyChanged(nameof(ActionName));
            }
        }

        public ObservableCollection<ButtonActionViewModel> Actions { get; set; }

        public RelayCommandWithParameter RecordMacroStartCommandWithParameter { get; set; }

        public RelayCommandWithParameter RecordMacroStopCommandWithParameter { get; set; }

        public RelayCommandWithParameter RecordMacroCancelCommandWithParameter { get; set; }

        public bool IsDisabledForced { get; set; }

        public bool IsRecording { get; set; }

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
            _hotasButtonMap.Actions.CollectionChanged += Actions_CollectionChanged;

            RecordMacroStartCommandWithParameter = new RelayCommandWithParameter(RecordMacroStart, RecordMacroStartCanExecute);
            RecordMacroStopCommandWithParameter = new RelayCommandWithParameter(RecordMacroStop, RecordMacroStopCanExecute);
            RecordMacroCancelCommandWithParameter = new RelayCommandWithParameter(RecordMacroCancel, RecordMacroCancelCanExecute);
            IsRecording = false;
            IsDisabledForced = false;
            Actions = new ObservableCollection<ButtonActionViewModel>();
            ActionItem = new ActionCatalogItem();
            ActionName = buttonMap.ActionName;
            AddHandlers();
            BuildButtonActionViewModel(buttonMap.Actions);
        }


        public ObservableCollection<ButtonAction> GetHotasActions()
        {
            return _hotasButtonMap.Actions;
        }

        public void AssignActions(ActionCatalogItem actionCatalogItem)
        {
            _hotasButtonMap.Actions = actionCatalogItem.Actions;
            _hotasButtonMap.ActionName = actionCatalogItem.ActionName;

            RemoveHandlers();
            ActionItem = actionCatalogItem;
            AddHandlers();

            ReBuildButtonActionViewModel();
            OnPropertyChanged(nameof(ActionName));
        }

        private void AddHandlers()
        {
            ActionItem.PropertyChanged += ActionItem_PropertyChanged;
        }

        private void RemoveHandlers()
        {
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

        private void ReBuildButtonActionViewModel()
        {
            BuildButtonActionViewModel(_hotasButtonMap.Actions);
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

            //save changes
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

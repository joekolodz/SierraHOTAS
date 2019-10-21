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
    public class MapViewModel : INotifyPropertyChanged
    {
        private readonly HOTASMap _hotasMap;
        public ActionCatalogItem ActionItem { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler RecordingStarted;
        public event EventHandler RecordingStopped;
        public event EventHandler RecordingCancelled;

        public uint Offset
        {
            get => _hotasMap.Offset;
            set => _hotasMap.Offset = value;
        }

        public int ButtonId
        {
            get => _hotasMap.ButtonId;
            set => _hotasMap.ButtonId = value;
        }

        public string ButtonName
        {
            get => _hotasMap.ButtonName;
            set
            {
                if (_hotasMap.ButtonName == value) return;
                _hotasMap.ButtonName = value;
                OnPropertyChanged(nameof(ButtonName));
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
                _hotasMap.ActionName = value;
                OnPropertyChanged(nameof(ActionName));
            }
        }

        public ObservableCollection<ButtonActionViewModel> Actions { get; set; }

        public RelayCommandWithParameter RecordMacroStartCommandWithParameter { get; set; }

        public RelayCommandWithParameter RecordMacroStopCommandWithParameter { get; set; }

        public RelayCommandWithParameter RecordMacroCancelCommandWithParameter { get; set; }

        public bool IsDisabledForced { get; set; }

        public bool IsRecording { get; set; }

        public MapViewModel()
        {
            _hotasMap = new HOTASMap();
            Actions = new ObservableCollection<ButtonActionViewModel>();
            ActionItem = new ActionCatalogItem();
            AddHandlers();
        }

        public MapViewModel(HOTASMap map)
        {
            _hotasMap = map;
            _hotasMap.Actions.CollectionChanged += Actions_CollectionChanged;

            RecordMacroStartCommandWithParameter = new RelayCommandWithParameter(RecordMacroStart, RecordMacroStartCanExecute);
            RecordMacroStopCommandWithParameter = new RelayCommandWithParameter(RecordMacroStop, RecordMacroStopCanExecute);
            RecordMacroCancelCommandWithParameter = new RelayCommandWithParameter(RecordMacroCancel, RecordMacroCancelCanExecute);
            IsRecording = false;
            IsDisabledForced = false;
            Actions = new ObservableCollection<ButtonActionViewModel>();
            ActionItem = new ActionCatalogItem();
            ActionName = map.ActionName;
            AddHandlers();
            BuildButtonActionViewModel(map.Actions);
        }


        public ObservableCollection<ButtonAction> GetHotasActions()
        {
            return _hotasMap.Actions;
        }

        public void AssignActions(ActionCatalogItem actionCatalogItem)
        {
            _hotasMap.Actions = actionCatalogItem.Actions;
            _hotasMap.ActionName = actionCatalogItem.ActionName;

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
            BuildButtonActionViewModel(_hotasMap.Actions);
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

            _hotasMap.Record();

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

            _hotasMap.Stop();

            //save changes
            ReBuildButtonActionViewModel();


            IsRecording = false;
            Debug.WriteLine($"STOPPED - Recorded==>{_hotasMap}");
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

            _hotasMap.Cancel();

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
    }
}

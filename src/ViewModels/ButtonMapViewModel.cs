﻿using SierraHOTAS.Annotations;
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

        private void RecordMacroStart()
        {
            if (IsDisabledForced) return;

            Actions.Clear();

            _hotasButtonMap.Record();

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

            _hotasButtonMap.Stop();

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

            _hotasButtonMap.Cancel();

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
            return;
        }
    }
}

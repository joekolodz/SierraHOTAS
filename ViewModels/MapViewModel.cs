using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SierraHOTAS.Annotations;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModel.Commands;
using SharpDX.DirectInput;

namespace SierraHOTAS.ViewModel
{
    public class MapViewModel : INotifyPropertyChanged
    {
        private readonly HOTASMap _hotasMap;

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
        }

        private void Actions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            BuildButtonActionViewModel(_hotasMap);
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
            BuildButtonActionViewModel(map);
        }

        private void BuildButtonActionViewModel(HOTASMap map)
        {
            Actions.Clear();
            foreach (var a in map.Actions)
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
            BuildButtonActionViewModel(_hotasMap);


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

            BuildButtonActionViewModel(_hotasMap);

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

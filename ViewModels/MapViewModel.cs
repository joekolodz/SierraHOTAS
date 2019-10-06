using System;
using System.Collections.Generic;
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
    public class MapViewModel //: INotifyPropertyChanged
    {
        private readonly HOTASMap _hotasMap;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler RecordingStarted;
        public event EventHandler RecordingStopped;
        public event EventHandler RecordingCancelled;

        public JoystickOffset Offset
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
            set => _hotasMap.ButtonName = value;
        }

        public string Action
        {
            get => _hotasMap.Action;
            set => _hotasMap.Action = value;
        }

        public List<ButtonAction> Actions
        {
            get => _hotasMap.Actions;
            set => _hotasMap.Actions = value;
        }

        public RelayCommandWithParameter RecordMacroStartCommandWithParameter { get; set; }

        public RelayCommandWithParameter RecordMacroStopCommandWithParameter { get; set; }

        public RelayCommandWithParameter RecordMacroCancelCommandWithParameter { get; set; }

        public bool IsDisabledForced { get; set; }

        public bool IsRecording { get; set; }


        public MapViewModel(HOTASMap map)
        {
            _hotasMap = map;
            RecordMacroStartCommandWithParameter = new RelayCommandWithParameter(RecordMacroStart, RecordMacroStartCanExecute);
            RecordMacroStopCommandWithParameter = new RelayCommandWithParameter(RecordMacroStop, RecordMacroStopCanExecute);
            RecordMacroCancelCommandWithParameter = new RelayCommandWithParameter(RecordMacroCancel, RecordMacroCancelCanExecute);
            IsRecording = false;
            IsDisabledForced = false;
        }
        private void RecordMacroStart(object parameter)
        {
            if (IsDisabledForced) return;

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
            Action = _hotasMap.Action;

            IsRecording = false;
            Debug.WriteLine($"STOPPED - Recorded==>{Action}");
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
            //clear changes


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

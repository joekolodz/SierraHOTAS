﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using SierraJSON;

namespace SierraHOTAS.Models
{
    public class HOTASButton : IHotasBaseMap
    {
        public enum ButtonType
        {
            AxisLinear,
            AxisRadial,
            POV,
            Button
        }

        public int MapId { get; set; }
        public string MapName { get; set; }
        public ButtonType Type { get; set; }
        public int ShiftModePage { get; set; }
        public bool IsShift { get; set; }
        public bool IsOneShot { get; set; }
        public int RepeatCount{ get; set; }

        public Guid ActionId
        {
            get => ActionCatalogItem.Id;
            set => ActionCatalogItem.Id = value;
        }

        [SierraJsonIgnore]
        public string ActionName
        {
            get => ActionCatalogItem.ActionName;
            set => ActionCatalogItem.ActionName = value;
        }

        [SierraJsonIgnore]
        public ActionCatalogItem ActionCatalogItem { get; set; }

        [SierraJsonIgnore]
        public bool IsMacro => ActionCatalogItem.Actions.Any(a => a.TimeInMilliseconds > 0);

        private bool _isRecording;
        private ObservableCollection<ButtonAction> _actionsHistory;

        public HOTASButton()
        {
            ActionCatalogItem = new ActionCatalogItem();
        }

        private void SetRecordState(bool isRecording)
        {
            _isRecording = isRecording;
            Keyboard.IsKeySuppressionActive = isRecording;
            if (isRecording)
            {
                AddKeyboardHandlers();
            }
            else
            {
                RemoveKeyboardHandlers();
            }
        }

        public void Record()
        {
            _actionsHistory = ActionCatalogItem.Actions.ToList().ToObservableCollection();//make an actual copy
            ActionCatalogItem.Actions.Clear();
            SetRecordState(true);
        }

        public void Stop()
        {
            SetRecordState(false);
            _actionsHistory = null;
        }

        public void Cancel()
        {
            SetRecordState(false);
            ActionCatalogItem.Actions.Clear();
            foreach (var a in _actionsHistory)
            {
                ActionCatalogItem.Actions.Add(a);
            }
        }

        public void ResetShift()
        {
            IsShift = false;
            ShiftModePage = 0;
        }

        private void AddKeyboardHandlers()
        {
            Keyboard.KeyDownEvent += Keyboard_KeyDownEvent;
            Keyboard.KeyUpEvent += Keyboard_KeyUpEvent;
        }

        private void RemoveKeyboardHandlers()
        {
            Keyboard.KeyDownEvent -= Keyboard_KeyDownEvent;
            Keyboard.KeyUpEvent -= Keyboard_KeyUpEvent;
        }

        private void Keyboard_KeyDownEvent(object sender, Keyboard.KeystrokeEventArgs e)
        {
            RecordKeypress(e);
        }

        private void Keyboard_KeyUpEvent(object sender, Keyboard.KeystrokeEventArgs e)
        {
            RecordKeypress(e);
        }

        private void RecordKeypress(Keyboard.KeystrokeEventArgs e)
        {
            if (!_isRecording) return;

            ActionCatalogItem.Actions.Add(new ButtonAction() { ScanCode = e.Code, IsKeyUp = e.IsKeyUp, IsExtended = e.IsExtended, TimeInMilliseconds = 0 });
        }

        public void ClearUnassignedActions()
        {
            if (ActionName != ActionCatalogItem.NO_ACTION_TEXT) return;
            ActionName = string.Empty;
            ActionCatalogItem.Actions.Clear();
        }

        public void CalculateSegmentRange(int segments)
        {
            return;
        }

        public void ClearSegments()
        {
            return;
        }

        public override string ToString()
        {
            var o = "";
            foreach (var a in ActionCatalogItem.Actions)
            {
                var upDown = "v";
                if (a.IsKeyUp)
                {
                    upDown = "^";
                }
                o += $"[{Enum.GetName(typeof(Win32Structures.ScanCodeShort), a.ScanCode)}{upDown}]";
            }
            return o;
        }
    }
}

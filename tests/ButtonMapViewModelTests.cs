using NSubstitute;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class ButtonMapViewModelTests
    {
        [Fact]
        public void constructor()
        {
            var mapVm = new ButtonMapViewModel();
            Assert.Empty(mapVm.Actions);
            Assert.NotNull(mapVm.ActionItem);
        }

        [Fact]
        public void constructor_param()
        {
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { ActionName = "test action" } };
            var mapVm = new ButtonMapViewModel(map);
            Assert.Empty(mapVm.Actions);
            Assert.NotNull(mapVm.ActionItem);
            Assert.Equal(43, mapVm.ButtonId);
            Assert.Equal("test map", mapVm.ButtonName);
            Assert.Equal(HOTASButtonMap.ButtonType.Button, mapVm.Type);
            Assert.Equal("test action", mapVm.ActionItem.ActionName);
            Assert.False(mapVm.IsRecording);
            Assert.False(mapVm.IsDisabledForced);
        }

        [Fact]
        public void to_string()
        {
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { ActionName = "test action" } };
            var mapVm = new ButtonMapViewModel(map);
            Assert.Equal($"ButtonMap MapId:{map.MapId}, {map.MapName}", mapVm.ToString());
        }

        [Fact]
        public void get_hotas_actions()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ActionCatalogItem = catalog };

            var mapVm = new ButtonMapViewModel(map);
            Assert.NotEmpty(mapVm.GetHotasActions());
        }

        [Fact]
        public void assign_actions()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button };

            var mapVm = new ButtonMapViewModel(map);
            Assert.Empty(mapVm.Actions);
            mapVm.AssignActions(catalog);
            Assert.NotEmpty(mapVm.GetHotasActions());
            Assert.NotEmpty(mapVm.Actions);
            Assert.Same(catalog, mapVm.ActionItem);
        }

        [Fact]
        public void assign_actions_with_no_action()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action", NoAction = true };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button };

            var mapVm = new ButtonMapViewModel(map);
            Assert.Empty(mapVm.Actions);
            mapVm.AssignActions(catalog);
            Assert.Empty(mapVm.GetHotasActions());
            Assert.Empty(mapVm.Actions);
            Assert.Equal(string.Empty, mapVm.ActionName);
        }

        [Fact]
        public void rebuild_button_action_view_model()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button };

            var mapVm = new ButtonMapViewModel(map);
            mapVm.AssignActions(catalog);
            Assert.NotEmpty(mapVm.Actions);
            mapVm.Actions.Clear();
            mapVm.ReBuildButtonActionViewModel();
            Assert.NotEmpty(mapVm.Actions);
            Assert.Equal("ESCAPE", mapVm.Actions[0].ScanCode);
        }

        [Fact]
        public void activate_shift_mode_page()
        {
            var shiftModePage = 43;
            var newShiftModePage = 1;
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ShiftModePage = shiftModePage };

            var mapVm = new ButtonMapViewModel(map);
            mapVm.AssignActions(catalog);
            Assert.Equal(shiftModePage, mapVm.ActivateShiftModePage);

            Assert.PropertyChanged(mapVm, "ActivateShiftModePage", () => mapVm.ActivateShiftModePage = 1);
            Assert.NotEqual(shiftModePage, mapVm.ActivateShiftModePage);
            Assert.Equal(newShiftModePage, mapVm.ActivateShiftModePage);
        }

        [Fact]
        public void action_name_changed()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button };

            var mapVm = new ButtonMapViewModel(map);
            mapVm.AssignActions(catalog);

            Assert.Equal("test action", mapVm.ActionName);

            var newActionName = "new action name";
            Assert.PropertyChanged(mapVm, "ActionName", () => mapVm.ActionName = newActionName);
            Assert.Equal(newActionName, mapVm.ActionItem.ActionName);
        }

        [Fact]
        public void record_macro_start_can_execute_not_disabled()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ActionCatalogItem = catalog };
            var mapVm = new ButtonMapViewModel(map);
            mapVm.IsDisabledForced = false;
            Assert.True(mapVm.RecordMacroStartCommand.CanExecute(default));
        }


        [Fact]
        public void record_macro_start_can_execute_is_disabled()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ActionCatalogItem = catalog };
            var mapVm = new ButtonMapViewModel(map);
            mapVm.IsDisabledForced = true;
            Assert.False(mapVm.RecordMacroStartCommand.CanExecute(default));
        }

        [Fact]
        public void record_macro_start_command()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ActionCatalogItem = catalog };
            var mapVm = new ButtonMapViewModel(map);

            Assert.NotEmpty(mapVm.Actions);
            Assert.False(mapVm.IsRecording);

            Assert.Raises<EventArgs>(a => mapVm.RecordingStarted += a, a => mapVm.RecordingStarted -= a, () => mapVm.RecordMacroStartCommand.Execute(default));

            Assert.Empty(mapVm.Actions);
            Assert.True(mapVm.IsRecording);
        }

        [Fact]
        public void record_macro_stop_can_execute_not_disabled()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ActionCatalogItem = catalog };
            var mapVm = new ButtonMapViewModel(map);
            mapVm.IsDisabledForced = false;
            mapVm.IsRecording = true;
            Assert.True(mapVm.RecordMacroStopCommand.CanExecute(default));
        }


        [Fact]
        public void record_macro_stop_can_execute_is_disabled()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ActionCatalogItem = catalog };
            var mapVm = new ButtonMapViewModel(map);
            mapVm.IsDisabledForced = true;
            Assert.False(mapVm.RecordMacroStopCommand.CanExecute(default));
        }

        [Fact]
        public void record_macro_stop_command()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ActionCatalogItem = catalog };
            var mapVm = new ButtonMapViewModel(map);

            Assert.NotEmpty(mapVm.Actions);
            Assert.False(mapVm.IsRecording);

            mapVm.RecordMacroStartCommand.Execute(default);
            Assert.Raises<EventArgs>(a => mapVm.RecordingStopped += a, a => mapVm.RecordingStopped -= a, () => mapVm.RecordMacroStopCommand.Execute(default));

            Assert.Empty(mapVm.Actions);//when recording is started, this Actions list is cleared out. this test stops recording without adding any new keypresses
            Assert.False(mapVm.IsRecording);
        }

        [Fact]
        public void record_macro_cancel_can_execute_not_disabled()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ActionCatalogItem = catalog };
            var mapVm = new ButtonMapViewModel(map);
            mapVm.IsDisabledForced = false;
            mapVm.IsRecording = true;
            Assert.True(mapVm.RecordMacroCancelCommand.CanExecute(default));
        }


        [Fact]
        public void record_macro_cancel_can_execute_is_disabled()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ActionCatalogItem = catalog };
            var mapVm = new ButtonMapViewModel(map);
            mapVm.IsDisabledForced = true;
            Assert.False(mapVm.RecordMacroCancelCommand.CanExecute(default));
        }

        [Fact]
        public void record_macro_cancel_command()
        {
            var catalog = new ActionCatalogItem() { ActionName = "test action" };
            catalog.Actions = new ObservableCollection<ButtonAction>()
            {
                new ButtonAction() {ScanCode = 1},
                new ButtonAction() {ScanCode = 2},
                new ButtonAction() {ScanCode = 3},
            };
            var map = new HOTASButtonMap() { MapName = "test map", MapId = 43, Type = HOTASButtonMap.ButtonType.Button, ActionCatalogItem = catalog };
            var mapVm = new ButtonMapViewModel(map);

            Assert.NotEmpty(mapVm.Actions);
            Assert.False(mapVm.IsRecording);

            mapVm.RecordMacroStartCommand.Execute(default);
            Assert.Raises<EventArgs>(a => mapVm.RecordingCancelled += a, a => mapVm.RecordingCancelled -= a, () => mapVm.RecordMacroCancelCommand.Execute(default));

            Assert.NotEmpty(mapVm.Actions);//when recording is started, this Actions list is cleared out, but saved. when the recording is cancelled, this list is restored.
            Assert.False(mapVm.IsRecording);
        }
    }
}

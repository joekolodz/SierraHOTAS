using NSubstitute;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class ModeConfigWindowViewModelTests
    {
        private class TestDispatcher : IDispatcher
        {
            public void Invoke(Action callback)
            {
                callback.Invoke();
            }
        }

        [Fact]
        public void basic_constructor_null()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), 1, null));
            Assert.Equal("Value cannot be null.\r\nParameter name: activationButtonList", exception.Message);

            exception = Assert.Throws<ArgumentNullException>(() => new ModeConfigWindowViewModel(null, Substitute.For<IDispatcher>(), 1, new Dictionary<int, ModeActivationItem>()));
            Assert.Equal("Value cannot be null.\r\nParameter name: eventAggregator", exception.Message);

            exception = Assert.Throws<ArgumentNullException>(() => new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), null, 1, new Dictionary<int, ModeActivationItem>()));
            Assert.Equal("Value cannot be null.\r\nParameter name: appDispatcher", exception.Message);
        }

        [Fact]
        public void basic_constructor_no_mode()
        {
            const int mode = 0;
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, new ModeActivationItem() {ButtonId = 1, ButtonName = "test button"}}
            };

            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, activationButtonList);
            Assert.Equal(2, profileVm.TemplateModes.Count);
            Assert.True(profileVm.IsTemplateModeVisible);
            Assert.Null(profileVm.ModeName);
        }

        [Fact]
        public void basic_constructor_template_not_visible_by_mode()
        {
            const int mode = 1;
            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, new Dictionary<int, ModeActivationItem>());
            Assert.False(profileVm.IsTemplateModeVisible);
        }

        [Fact]
        public void basic_constructor_template_not_visible_by_null_activation_button_list()
        {
            var profileVm = new ModeConfigWindowViewModel();
            Assert.False(profileVm.IsTemplateModeVisible);
        }

        [Fact]
        public void basic_constructor_template_not_visible_activation_button_list_has_items()
        {
            const int mode = 1;
            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, new Dictionary<int, ModeActivationItem>(){ {1,new ModeActivationItem()}});
            Assert.False(profileVm.IsTemplateModeVisible);
        }

        [Fact]
        public void basic_constructor_no_valid_mode()
        {
            const int mode = 1;
            var deviceId = Guid.NewGuid();
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, new ModeActivationItem() {ButtonId = 1, ButtonName = "test button", ModeName = "select combat mode", IsShift = true, DeviceId = deviceId}}
            };

            var dispatcher = new TestDispatcher();


            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), dispatcher, mode, activationButtonList);


            Assert.Equal(2, profileVm.TemplateModes.Count);
            Assert.False(profileVm.IsTemplateModeVisible);
            Assert.Equal("select combat mode", profileVm.ModeName);
            Assert.Equal("test button", profileVm.ActivationButtonName);
            Assert.True(profileVm.IsShift);
        }

        [Fact]
        public void basic_constructor_shift_not_set()
        {
            const int mode = 1;
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, new ModeActivationItem() {ButtonId = 1, ButtonName = "test button", ModeName = "select combat mode"}}
            };

            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, activationButtonList);
            Assert.False(profileVm.IsShift);
        }

        [Fact]
        public void properties_raise_property_changed()
        {
            const int mode = 1;
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, new ModeActivationItem() {ButtonId = 1, ButtonName = "test button", ModeName = "select combat mode"}}
            };

            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, activationButtonList);
            profileVm.ModeName = string.Empty;
            profileVm.DeviceName = string.Empty;
            profileVm.ActivationButtonName = string.Empty;
            profileVm.IsActivationErrorVisible = false;
            profileVm.IsShift = false;

            Assert.PropertyChanged(profileVm, "ModeName", () => profileVm.ModeName = "changed'");
            Assert.PropertyChanged(profileVm, "DeviceName", () => profileVm.DeviceName = "changed'");
            Assert.PropertyChanged(profileVm, "ActivationButtonName", () => profileVm.ActivationButtonName = "changed'");
            Assert.PropertyChanged(profileVm, "IsActivationErrorVisible", () => profileVm.IsActivationErrorVisible = true);
            Assert.PropertyChanged(profileVm, "IsShift", () => profileVm.IsShift = true);
        }

        [Fact]
        public void is_shift_visible()
        {
            var mode = 0;
            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, new Dictionary<int, ModeActivationItem>());

            Assert.True(profileVm.IsShiftVisible);

            mode = 1;
            profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, new Dictionary<int, ModeActivationItem>());
            Assert.False(profileVm.IsShiftVisible);
        }

        [Fact]
        public void device_list_button_pressed_button_not_found()
        {
            const int mode = 1;
            const string expectedDeviceName = "not set";
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, new ModeActivationItem() {ButtonId = 1, ButtonName = "test button", ModeName = "select combat mode"}}
            };

            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, activationButtonList);
            profileVm.DeviceName = expectedDeviceName;

            var device = new HOTASDevice();

            profileVm.DeviceList_ButtonPressed(new object(), new ButtonPressedEventArgs() { ButtonId = 1, Device = device });
            Assert.Equal(expectedDeviceName, profileVm.DeviceName);
        }

        [Fact]
        public void device_list_button_pressed_button_found()
        {
            const int mode = 1;
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, new ModeActivationItem() {ButtonId = 1, ButtonName = "test button", ModeName = "select combat mode"}}
            };

            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), new TestDispatcher(), mode, activationButtonList);
            profileVm.DeviceName = "not set";

            var device1 = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.NewGuid(), Guid.NewGuid(), "test device 1", Substitute.For<IHOTASQueue>());
            device1.ButtonMap.Add(new HOTASButton() { MapId = 1 });
            device1.ButtonMap.Add(new HOTASButton() { MapId = 2 });

            profileVm.DeviceList_ButtonPressed(new object(), new ButtonPressedEventArgs() { ButtonId = 1, Device = device1 });
            Assert.Equal("test device 1", profileVm.DeviceName);

            var device2 = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.NewGuid(), Guid.NewGuid(), "test device 2", Substitute.For<IHOTASQueue>());
            device2.ButtonMap.Add(new HOTASButton() { MapId = 4, MapName = "test activation name" });
            device2.ButtonMap.Add(new HOTASButton() { MapId = 5 });

            //button and device do not match up so devicename does not change
            profileVm.DeviceList_ButtonPressed(new object(), new ButtonPressedEventArgs() { ButtonId = 1, Device = device2 });
            Assert.Equal("test device 1", profileVm.DeviceName);

            profileVm.PropertyChanged += ProfileVm_PropertyChanged_for_basic_constructor_no_valid_mode_test;
            property_chagned_for_basic_constructor_no_valid_mode_test = false;
            profileVm.DeviceList_ButtonPressed(new object(), new ButtonPressedEventArgs() { ButtonId = 4, Device = device2 });
            Assert.Equal("test device 2", profileVm.DeviceName);
            Assert.Equal("test activation name", profileVm.ActivationButtonName);
            Assert.False(profileVm.IsActivationErrorVisible);
            Assert.True(property_chagned_for_basic_constructor_no_valid_mode_test);
        }

        private bool property_chagned_for_basic_constructor_no_valid_mode_test = false;
        private void ProfileVm_PropertyChanged_for_basic_constructor_no_valid_mode_test(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TemplateModes")
            {
                property_chagned_for_basic_constructor_no_valid_mode_test = true;
            }
        }

        [Fact]
        public void save_mode_profile_command_remove_existing_activation_button()
        {
            const int modeToRemove = 2;

            var activationButton = new ModeActivationItem() { ButtonId = 1, ButtonName = "test button 1", ModeName = "select combat mode" };
            var removeThisItem = new ModeActivationItem() { ButtonId = 2, ButtonName = "test button 2", ModeName = "select nav mode" };
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, activationButton},
                {2, removeThisItem}
            };

            var subEventAggregator = Substitute.For<IEventAggregator>();
            var profileVm = new ModeConfigWindowViewModel(subEventAggregator, Substitute.For<IDispatcher>(), modeToRemove, activationButtonList);
            profileVm.SaveModeCommand.Execute(default);
            subEventAggregator.Received().Publish(Arg.Any<DeleteModeEvent>());
        }

        [Fact]
        public void save_mode_profile_command_shift_mode_page_set()
        {
            const int mode = 1;

            var activationButton = new ModeActivationItem() { ButtonId = 1, ButtonName = "test button 1", ModeName = "select combat mode" };
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, activationButton},
            };

            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, activationButtonList);
            var device1 = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.NewGuid(), Guid.NewGuid(), "test device 1", Substitute.For<IHOTASQueue>());
            var button = new HOTASButton() { MapId = 1, ShiftModePage = 0 };
            device1.ButtonMap.Add(button);

            //shift mode test needs a button to have been pressed
            profileVm.DeviceList_ButtonPressed(new object(), new ButtonPressedEventArgs() { ButtonId = 1, Device = device1 });

            profileVm.SaveModeCommand.Execute(default);
            Assert.Equal(mode, button.ShiftModePage);
        }

        [Fact]
        public void save_mode_profile_command_activation_item_set()
        {
            const int mode = 1;

            var activationButton = new ModeActivationItem() { ButtonId = 1, ButtonName = "test button 1", ModeName = "select combat mode", DeviceName = "original device" };
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, activationButton},
            };

            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, activationButtonList);
            var device1 = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.NewGuid(), Guid.NewGuid(), "new device 1", Substitute.For<IHOTASQueue>());
            var button = new HOTASButton() { MapId = 43, ShiftModePage = 0, ActionName = "new action", MapName = "new map" };
            device1.ButtonMap.Add(button);

            //test needs a button to have been pressed
            profileVm.DeviceList_ButtonPressed(new object(), new ButtonPressedEventArgs() { ButtonId = 43, Device = device1 });

            profileVm.SaveModeCommand.Execute(default);

            Assert.Single(activationButtonList);
            Assert.Equal(43, activationButtonList[1].ButtonId);
            Assert.Equal("new device 1", activationButtonList[1].DeviceName);
            Assert.Equal("new map", activationButtonList[1].ButtonName);
        }

        [Fact]
        public void save_mode_profile_command_new_mode_profile_saved_event()
        {
            const int mode = 1;

            var activationButton = new ModeActivationItem() { ButtonId = 1, ButtonName = "test button 1", ModeName = "select combat mode", DeviceName = "original device" };
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, activationButton},
            };

            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, activationButtonList);
            var device1 = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.NewGuid(), Guid.NewGuid(), "new device 1", Substitute.For<IHOTASQueue>());
            var button = new HOTASButton() { MapId = 43, ShiftModePage = 0, ActionName = "new action", MapName = "new map" };
            device1.ButtonMap.Add(button);

            //test needs a button to have been pressed
            profileVm.DeviceList_ButtonPressed(new object(), new ButtonPressedEventArgs() { ButtonId = 43, Device = device1 });

            Assert.Raises<EventArgs>(a => profileVm.NewModeSaved += a, a => profileVm.NewModeSaved -= a, () => profileVm.SaveModeCommand.Execute(default));

        }

        [Fact]
        public void cancel_mode_profile_command()
        {
            const int mode = 1;

            var activationButton = new ModeActivationItem() { ButtonId = 1, ButtonName = "test button 1", ModeName = "select combat mode", DeviceName = "original device" };
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, activationButton},
            };

            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, activationButtonList);

            Assert.Raises<EventArgs>(a => profileVm.SaveCancelled += a, a => profileVm.SaveCancelled -= a, () => profileVm.CancelCommand.Execute(default));
        }

        [Fact]
        public void validate_activation_button()
        {
            var device1Id = Guid.NewGuid();
            var device2Id = Guid.NewGuid();
            const int mode = 1;
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, new ModeActivationItem() {ButtonId = 1, ButtonName = "test button", ModeName = "select combat mode", DeviceId = device1Id, DeviceName = "first device"}},
                {2, new ModeActivationItem() {ButtonId = 2, ButtonName = "test button", ModeName = "already mapped", DeviceId = device2Id, DeviceName = "second device"}}
            };

            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), new TestDispatcher(), mode, activationButtonList);

            var device1 = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.NewGuid(), device1Id, "first device", Substitute.For<IHOTASQueue>());
            device1.ButtonMap.Add(new HOTASButton() { MapId = 1 });

            var device2 = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.NewGuid(), device2Id, "second device", Substitute.For<IHOTASQueue>());
            device2.ButtonMap.Add(new HOTASButton() { MapId = 2, MapName = "test activation name" });

            Assert.False(profileVm.IsActivationErrorVisible);
            //press a button on a device tha is already in the activation list
            profileVm.DeviceList_ButtonPressed(new object(), new ButtonPressedEventArgs() { ButtonId = device2.ButtonMap[0].MapId, Device = device2 });
            Assert.True(profileVm.IsActivationErrorVisible);
            Assert.False(profileVm.SaveModeCommand.CanExecute(default));
        }

        [Fact]
        public void can_execute_save_command_true()
        {
            const int mode = 1;

            var activationButton = new ModeActivationItem() { ButtonId = 1, ButtonName = "test button 1", ModeName = "select combat mode", DeviceName = "original device" };
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, activationButton},
            };

            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, activationButtonList);
            Assert.True(profileVm.SaveModeCommand.CanExecute(default));
        }

        [Fact]
        public void template_mode_selected()
        {
            const int mode = 1;
            const int selectThisTemplateMode = 2;

            var activationButton1 = new ModeActivationItem() { ButtonId = 1, ButtonName = "test button 1", ModeName = "select combat mode", DeviceName = "original device" };
            var activationButton2 = new ModeActivationItem() { ButtonId = 2, ButtonName = "test button 2", ModeName = "select nav mode", DeviceName = "original device" };
            var activationButtonList = new Dictionary<int, ModeActivationItem>
            {
                {1, activationButton1},
                {selectThisTemplateMode, activationButton2}
            };

            var profileVm = new ModeConfigWindowViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IDispatcher>(), mode, activationButtonList);
            var device1 = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.NewGuid(), Guid.NewGuid(), "new device 1", Substitute.For<IHOTASQueue>());
            var button = new HOTASButton() { MapId = 43, ShiftModePage = 0, ActionName = "new action 1", MapName = "new map 1" };
            device1.ButtonMap.Add(button);

            profileVm.DeviceList_ButtonPressed(new object(), new ButtonPressedEventArgs() { ButtonId = 43, Device = device1 });
            
            profileVm.SaveModeCommand.Execute(default);
            Assert.Equal(0, activationButtonList[1].TemplateMode);//template mode is unselected by default

            profileVm.CopyTemplateMode = selectThisTemplateMode;

            button = new HOTASButton() { MapId = 44, ShiftModePage = 0, ActionName = "new action 2", MapName = "new map 2" };
            device1.ButtonMap.Add(button);

            profileVm.DeviceList_ButtonPressed(new object(), new ButtonPressedEventArgs() { ButtonId = 44, Device = device1 });

            profileVm.SaveModeCommand.Execute(default);

            Assert.Equal(selectThisTemplateMode, activationButtonList[1].TemplateMode);//template mode is now selected
        }

        [Fact]
        public void addicted_to_code_coverage()
        {
            var _ = new ModeConfigWindowViewModel();
        }
    }
}

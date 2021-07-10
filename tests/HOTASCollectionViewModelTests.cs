using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Pose;
using SharpDX.DirectInput;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using SierraHOTAS.ModeProfileWindow.ViewModels;
using SierraHOTAS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using Xunit;
using Xunit.Abstractions;
using EventArgs = System.EventArgs;

namespace SierraHOTAS.Tests
{
    public class HOTASCollectionViewModelTests
    {
        private ITestOutputHelper _output;
        public HOTASCollectionViewModelTests(ITestOutputHelper output)
        {
            _output = output;
        }

        //public static class DispatcherUtil
        //{
        //    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //    public static void DoEvents()
        //    {
        //        var frame = new DispatcherFrame();
        //        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
        //            new DispatcherOperationCallback(ExitFrame), frame);
        //        Dispatcher.PushFrame(frame);
        //    }

        //    private static object ExitFrame(object frame)
        //    {
        //        ((DispatcherFrame)frame).Continue = false;
        //        return null;
        //    }
        //}

        private static HOTASCollectionViewModel CreateHotasCollectionViewModel(out IEventAggregator subEventAggregator, out IHOTASCollection subHotasCollection, out IFileSystem subFileSystem, out MediaPlayerFactory subMediaPlayerFactory, out Dictionary<int, ModeActivationItem> subModeProfileButtons, out ActionCatalogViewModel subActionVm)
        {
            subFileSystem = Substitute.For<IFileSystem>();
            subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();

            subHotasCollection = Substitute.For<IHOTASCollection>();
            subHotasCollection.Devices = new ObservableCollection<HOTASDevice>() { new HOTASDevice() };

            subModeProfileButtons = new Dictionary<int, ModeActivationItem>();
            subHotasCollection.ModeProfileActivationButtons.Returns(subModeProfileButtons);

            subActionVm = Substitute.For<ActionCatalogViewModel>();

            subEventAggregator = Substitute.For<IEventAggregator>();

            var hotasVm = new HOTASCollectionViewModel(Dispatcher.CurrentDispatcher, subEventAggregator, subFileSystem, subMediaPlayerFactory, subHotasCollection, subActionVm);
            return hotasVm;
        }

        private static HOTASButtonMap AddHotasButtonMap(ObservableCollection<IHotasBaseMap> buttons, int existingButtonMapId = 1, HOTASButtonMap.ButtonType type = HOTASButtonMap.ButtonType.Button, string mapName = "Button1", string actionName = "Fire", int scanCode = 43)
        {
            var map = CreateHotasButtonMap(existingButtonMapId, type, mapName, actionName, scanCode);
            buttons.Add(map);
            return map;
        }

        private static HOTASButtonMap CreateHotasButtonMap(int existingButtonMapId = 1, HOTASButtonMap.ButtonType type = HOTASButtonMap.ButtonType.Button, string mapName = "Button1", string actionName = "Fire", int scanCode = 43)
        {
            var map = new HOTASButtonMap()
            {
                MapId = existingButtonMapId,
                Type = type,
                MapName = mapName,
                ActionName = actionName,
                ActionCatalogItem = new ActionCatalogItem()
                {
                    ActionName = actionName,
                    Actions = new ObservableCollection<ButtonAction>()
                    {
                        new ButtonAction() { Flags = 0, ScanCode = scanCode },
                        new ButtonAction() { Flags = 1, ScanCode = scanCode },
                    }
                }
            };
            return map;
        }

        private static HOTASAxisMap AddHotasAxisMap(ObservableCollection<IHotasBaseMap> buttons, int existingButtonMapId = 1, HOTASButtonMap.ButtonType type = HOTASButtonMap.ButtonType.AxisLinear, string mapName = "Button1", string actionName = "Fire", int scanCode = 43)
        {
            var map = new HOTASAxisMap()
            {
                MapId = existingButtonMapId,
                Type = type,
                MapName = mapName,
                ButtonMap = new ObservableCollection<HOTASButtonMap>()
                {
                    new HOTASButtonMap()
                    {
                        ActionName = actionName,
                        ActionCatalogItem = new ActionCatalogItem()
                        {
                            ActionName = actionName,
                            Actions = new ObservableCollection<ButtonAction>()
                            {
                                new ButtonAction() { Flags = 0, ScanCode = scanCode },
                                new ButtonAction() { Flags = 1, ScanCode = scanCode },
                            }
                        }
                    }
                },
                ReverseButtonMap = new ObservableCollection<HOTASButtonMap>()
                {
                    new HOTASButtonMap()
                    {
                        ActionName = actionName,
                        ActionCatalogItem = new ActionCatalogItem()
                        {
                            ActionName = actionName,
                            Actions = new ObservableCollection<ButtonAction>()
                            {
                                new ButtonAction() { Flags = 0, ScanCode = scanCode },
                                new ButtonAction() { Flags = 1, ScanCode = scanCode },
                            }
                        }
                    }
                }

            };
            buttons.Add(map);
            return map;
        }

        [Fact]
        public void construction()
        {
            var shimPath = Shim.Replace(() => Path.GetFileNameWithoutExtension(Is.A<string>())).With((string s) => "Test");
            PoseContext.Isolate(() =>
            {
                Assert.Equal("Test", Path.GetFileNameWithoutExtension("This parameter doesn't matter"));
            }, shimPath);
        }

        [Fact]
        public void set_mode_without_rebuild_map()
        {
            const int expectedMode = 43;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            var receivedEvents = new List<int>();

            hotasVm.ModeProfileChanged += delegate (object sender, ModeProfileChangedEventArgs e)
            {
                receivedEvents.Add(e.Mode);
            };

            //Test
            hotasVm.Initialize();
            hotasVm.SetMode(expectedMode);

            Assert.Single(receivedEvents);
            subDeviceList.Received().SetMode(expectedMode);
        }

        [Fact]
        public void set_mode_with_rebuild_map()
        {
            const int expectedMode = 43;
            const string firstExpectedButtonName = "original";
            const string secondExpectedButtonName = "edited";

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            var device = subDeviceList.Devices[0];
            subDeviceList.GetDevice(default).ReturnsForAnyArgs(device);

            var buttonMap = new HOTASButtonMap() {Type = HOTASButtonMap.ButtonType.Button, MapName = firstExpectedButtonName};
            subDeviceList.Devices[0].ButtonMap.Add(buttonMap);

            hotasVm.Initialize();

            Assert.Equal(firstExpectedButtonName, hotasVm.Devices[0].ButtonMap[0].ButtonName);

            buttonMap.MapName = secondExpectedButtonName;

            hotasVm.SetMode(expectedMode);

            Assert.Equal(secondExpectedButtonName, hotasVm.Devices[0].ButtonMap[0].ButtonName);
        }

        [Fact]
        public void set_mode_exit_if_same_mode()
        {
            const int expectedMode = 43;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            subDeviceList.Mode = expectedMode;

            var receivedEvents = new List<int>();

            hotasVm.ModeProfileChanged += delegate (object sender, ModeProfileChangedEventArgs e)
            {
                receivedEvents.Add(e.Mode);
            };

            //Test
            hotasVm.Initialize();
            hotasVm.SetMode(expectedMode);

            Assert.Empty(receivedEvents);
        }

        [Fact]
        public void selection_changed_command()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out _, out _, out _, out _, out _);
            var model = new DeviceViewModel() { Name = "Test" };

            //Test
            hotasVm.Initialize();
            hotasVm.SelectionChangedCommand.Execute(model);

            Assert.Same(model, hotasVm.SelectedDevice);
        }

        [Fact]
        public void file_save_command()
        {
            const string expectedProfileSetFileName = "Test File Name";

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out var subFileSystem, out _, out _, out _);
            subFileSystem.LastSavedFileName = expectedProfileSetFileName;
            hotasVm.Initialize();
            hotasVm.SaveFileCommand.Execute(default);

            subFileSystem.ReceivedWithAnyArgs().FileSave(default);
            subFileSystem.DidNotReceive().FileSaveAs(default);
            subDeviceList.Received().ClearUnassignedActions();
            Assert.Equal(expectedProfileSetFileName, hotasVm.ProfileSetFileName);
        }

        [Fact]
        public void file_save_as_command()
        {
            const string expectedProfileSetFileName = "Test File Name";

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out var subFileSystem, out _, out _, out _);
            subFileSystem.LastSavedFileName = expectedProfileSetFileName;
            hotasVm.Initialize();
            hotasVm.SaveFileAsCommand.Execute(default);

            subFileSystem.ReceivedWithAnyArgs().FileSaveAs(default);
            subFileSystem.DidNotReceive().FileSave(default);
            subDeviceList.DidNotReceive().ClearUnassignedActions();
            Assert.Equal(expectedProfileSetFileName, hotasVm.ProfileSetFileName);
        }

        [Fact]
        public void file_open_command_valid_file()
        {
            var deviceGuid = Guid.NewGuid();
            var productGuid = Guid.NewGuid();
            const int existingButtonMapId = 48;
            const string expectedActionName = "Release";
            const int loadedButtonMapId = 48;
            const int modeActivationButtonId = 1000;

            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subDirectInput = Substitute.For<IDirectInput>();
            subDirectInputFactory.CreateDirectInput().Returns(subDirectInput);

            var subJoystick = Substitute.For<IJoystick>();
            subJoystick.Capabilities.Returns(new Capabilities(){AxeCount = 2, ButtonCount = 1});
            var subJoystickFactory = Substitute.For<JoystickFactory>();
            subJoystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);

            var subHotasQueueFactory = Substitute.For<HOTASQueueFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subHotasQueue = Substitute.For<IHOTASQueue>();
            subHotasQueueFactory.CreateHOTASQueue().Returns(subHotasQueue);

            var loadedHotasCollection = new HOTASCollection(subDirectInputFactory, subJoystickFactory, subHotasQueueFactory, subMediaPlayerFactory);
            loadedHotasCollection.Devices.Add(new HOTASDevice(subDirectInput, subJoystickFactory, productGuid, deviceGuid, "loaded device", subHotasQueue));

            var testMap = loadedHotasCollection.Devices[0].ButtonMap.First(m => m.MapId == 48) as HOTASButtonMap;
            Assert.NotNull(testMap);
            var i = loadedHotasCollection.Devices[0].ButtonMap.IndexOf(testMap);
            loadedHotasCollection.Devices[0].ButtonMap[i] = CreateHotasButtonMap(testMap.MapId, HOTASButtonMap.ButtonType.Button, "Button1", "Release");
            testMap.ActionName = expectedActionName;

            loadedHotasCollection.ModeProfileActivationButtons.Add(1, new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid });

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out var subFileSystem, out _, out _, out _);

            var existingDevice = subDeviceList.Devices[0];
            existingDevice.DeviceId = deviceGuid;
            existingDevice.Name = "existing device";
            AddHotasButtonMap(existingDevice.ButtonMap, existingButtonMapId);

            subDeviceList.GetDevice(deviceGuid).Returns(existingDevice);

            subFileSystem.FileOpenDialog().Returns(loadedHotasCollection);

            hotasVm.Initialize();
            hotasVm.OpenFileCommand.Execute(default);


            //check that the in-memory button (existing) is replaced by the one loaded from the file
            var actualMap = hotasVm.Devices[0].ButtonMap.First(m => m.ButtonId == loadedButtonMapId) as ButtonMapViewModel;
            Assert.NotNull(actualMap);
            Assert.Equal(expectedActionName, actualMap.ActionName);

            //mode profiles should be loaded
            Assert.Equal(modeActivationButtonId, subDeviceList.ModeProfileActivationButtons[1].ButtonId);

            //one device vm will have been created
            Assert.Single(hotasVm.Devices);
            Assert.Equal(deviceGuid, hotasVm.Devices[0].InstanceId);

            //check action catalog is rebuilt with the button loaded from file
            Assert.Equal(2, hotasVm.ActionCatalog.Catalog.Count);
            Assert.Contains(hotasVm.ActionCatalog.Catalog, item => item.ActionName == "<No Action>");
            Assert.Contains(hotasVm.ActionCatalog.Catalog, item => item.ActionName == "Release");

            subDeviceList.Received().Stop();
            subFileSystem.Received().FileOpenDialog();
            subDeviceList.Received().AutoSetMode();
            subDeviceList.Received().ListenToAllDevices();
        }

        [Fact]
        public void file_open_command_valid_file_existing_device()
        {
            //mainly for code coverage of BuildDevicesViewModelFromLoadedDevices

            var existingDeviceId = Guid.NewGuid();
            var loadedDeviceId = Guid.NewGuid();
            var productGuid = Guid.NewGuid();

            const int existingButtonMapId = 48;
            const string expectedActionName = "Release";
            const int loadedButtonMapId = 48;
            const int modeActivationButtonId = 1000;

            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subDirectInput = Substitute.For<IDirectInput>();
            subDirectInputFactory.CreateDirectInput().Returns(subDirectInput);

            var subJoystick = Substitute.For<IJoystick>();
            subJoystick.Capabilities.Returns(new Capabilities() { AxeCount = 2, ButtonCount = 1 });
            var subJoystickFactory = Substitute.For<JoystickFactory>();
            subJoystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);

            var subHotasQueueFactory = Substitute.For<HOTASQueueFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subHotasQueue = Substitute.For<IHOTASQueue>();
            subHotasQueueFactory.CreateHOTASQueue().Returns(subHotasQueue);

            var loadedHotasCollection = new HOTASCollection(subDirectInputFactory, subJoystickFactory, subHotasQueueFactory, subMediaPlayerFactory);
            loadedHotasCollection.Devices.Add(new HOTASDevice(subDirectInput, subJoystickFactory, productGuid, existingDeviceId, "loaded device", subHotasQueue));

            var testMap = loadedHotasCollection.Devices[0].ButtonMap.First(m => m.MapId == 48) as HOTASButtonMap;
            Assert.NotNull(testMap);
            var i = loadedHotasCollection.Devices[0].ButtonMap.IndexOf(testMap);
            loadedHotasCollection.Devices[0].ButtonMap[i] = CreateHotasButtonMap(testMap.MapId, HOTASButtonMap.ButtonType.Button, "Button1", "Release");
            testMap.ActionName = expectedActionName;

            loadedHotasCollection.ModeProfileActivationButtons.Add(1, new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = loadedDeviceId });

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out var subFileSystem, out _, out _, out _);

            var existingDevice = subDeviceList.Devices[0];
            existingDevice.DeviceId = existingDeviceId;
            existingDevice.Name = "existing device";
            existingDevice.Capabilities = new Capabilities() { AxeCount = 2, ButtonCount = 1 };


            AddHotasButtonMap(existingDevice.ButtonMap, existingButtonMapId);

            subDeviceList.GetDevice(existingDeviceId).Returns(existingDevice);

            subFileSystem.FileOpenDialog().Returns(loadedHotasCollection);

            hotasVm.Initialize();

            hotasVm.OpenFileCommand.Execute(default);


            //check that the in-memory button (existing) is replaced by the one loaded from the file
            var actualMap = hotasVm.Devices[0].ButtonMap.First(m => m.ButtonId == loadedButtonMapId) as ButtonMapViewModel;
            Assert.NotNull(actualMap);
            Assert.Equal(expectedActionName, actualMap.ActionName);

            //mode profiles should be loaded
            Assert.Equal(modeActivationButtonId, subDeviceList.ModeProfileActivationButtons[1].ButtonId);

            //one device vm will have been created
            Assert.Single(hotasVm.Devices);
            Assert.Equal(existingDeviceId, hotasVm.Devices[0].InstanceId);

            //check action catalog is rebuilt with the button loaded from file
            Assert.Equal(2, hotasVm.ActionCatalog.Catalog.Count);
            Assert.Contains(hotasVm.ActionCatalog.Catalog, item => item.ActionName == "<No Action>");
            Assert.Contains(hotasVm.ActionCatalog.Catalog, item => item.ActionName == "Release");

            subDeviceList.Received().Stop();
            subFileSystem.Received().FileOpenDialog();
            subDeviceList.Received().AutoSetMode();
            subDeviceList.Received().ListenToAllDevices();
        }

        [Fact]
        public void file_open_command_null_file()
        {
            var deviceGuid = Guid.NewGuid();
            const int existingButtonMapId = 4300;
            const int loadedButtonMapId = 2700;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out var subFileSystem, out _, out _, out _);

            var existingDevice = subDeviceList.Devices[0];
            existingDevice.DeviceId = deviceGuid;
            existingDevice.Name = "existing device";
            existingDevice.ButtonMap.Add(new HOTASButtonMap() { MapId = existingButtonMapId, Type = HOTASButtonMap.ButtonType.Button, MapName = "Button1", ActionName = "Fire" });
            subDeviceList.GetDevice(deviceGuid).Returns(existingDevice);

            subFileSystem.FileOpenDialog().Returns((HOTASCollection)null);

            hotasVm.Initialize();
            hotasVm.OpenFileCommand.Execute(default);


            //check that the in-memory button (existing) is replaced by the one loaded from the file
            Assert.Equal(existingButtonMapId, hotasVm.Devices[0].ButtonMap[0].ButtonId);
            Assert.NotEqual(loadedButtonMapId, hotasVm.Devices[0].ButtonMap[0].ButtonId);

            Assert.Single(hotasVm.ActionCatalog.Catalog);
            Assert.Equal("<No Action>", hotasVm.ActionCatalog.Catalog[0].ActionName);

            subDeviceList.DidNotReceive().Stop();
            subFileSystem.Received().FileOpenDialog();
            subDeviceList.DidNotReceive().AutoSetMode();
            subDeviceList.DidNotReceive().ListenToAllDevices();
        }

        [Fact]
        public void clear_active_profile_set_command()
        {
            var deviceGuid = Guid.NewGuid();
            const int existingButtonMapId = 4300;
            const int modeActivationButtonId = 1000;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out var subFileSystem, out _, out _, out _);

            const string expectedPropertyChanged = nameof(hotasVm.ModeActivationItems);
            var receivedEvents = new List<string>();
            hotasVm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            var existingDevice = subDeviceList.Devices[0];
            existingDevice.DeviceId = deviceGuid;
            existingDevice.Name = "existing device";
            AddHotasButtonMap(existingDevice.ButtonMap, existingButtonMapId);
            AddHotasButtonMap(existingDevice.ButtonMap, 0, HOTASButtonMap.ButtonType.Button, "Button2 - remove my actions", "<No Action>", 1);
            subDeviceList.ModeProfileActivationButtons.Add(1, new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid });

            subFileSystem.FileOpenDialog().Returns((HOTASCollection)null);

            hotasVm.Initialize();

            var existingButtonmapVm = hotasVm.Devices[0].ButtonMap[0];

            hotasVm.ClearActiveProfileSetCommand.Execute(default);

            var expectedButtonMapVm = hotasVm.Devices[0].ButtonMap[0];

            Assert.NotEqual(expectedButtonMapVm, existingButtonmapVm);
            Assert.Empty(subDeviceList.ModeProfileActivationButtons);
            Assert.Empty(((HOTASButtonMap)existingDevice.ButtonMap[1]).ActionCatalogItem.Actions);
            Assert.Equal(2, receivedEvents.Count);
            Assert.Equal(expectedPropertyChanged, receivedEvents[0]);
            Assert.Empty(hotasVm.ProfileSetFileName);
            subDeviceList.Received().ClearButtonMap();
        }

        [Fact]
        public void refresh_device_list_command_rescan_existing()
        {
            var deviceGuid = Guid.NewGuid();
            var ignoreGuid = Guid.NewGuid();
            const int existingButtonMapId = 4300;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);

            var existingDevice = subDeviceList.Devices[0];
            existingDevice.DeviceId = deviceGuid;
            existingDevice.Name = "existing device 1";
            AddHotasButtonMap(existingDevice.ButtonMap, existingButtonMapId, HOTASButtonMap.ButtonType.Button, "existing button");

            var rescannedDevice = new HOTASDevice { DeviceId = deviceGuid, Name = "rescanned device 1" };
            AddHotasButtonMap(rescannedDevice.ButtonMap, existingButtonMapId, HOTASButtonMap.ButtonType.Button, "rescanned button");
            subDeviceList.GetHOTASDevices().Returns(new ObservableCollection<HOTASDevice>() { rescannedDevice });

            hotasVm.Initialize();
            hotasVm.RefreshDeviceListCommand.Execute(default);

            Assert.Single(hotasVm.Devices);
            Assert.Equal(hotasVm.Devices[0].Name, rescannedDevice.Name);
            subDeviceList.Received().GetHOTASDevices();
            subDeviceList.Received().ListenToDevice(rescannedDevice);
            subDeviceList.Received().ReplaceDevice(rescannedDevice);
        }
        [Fact]
        public void refresh_device_list_command_new_device()
        {
            var deviceGuid = Guid.NewGuid();
            var ignoreGuid = Guid.NewGuid();
            const int existingButtonMapId = 4300;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);

            var existingDevice = subDeviceList.Devices[0];
            existingDevice.DeviceId = deviceGuid;
            existingDevice.Name = "existing device 1";
            AddHotasButtonMap(existingDevice.ButtonMap, existingButtonMapId, HOTASButtonMap.ButtonType.Button, "existing button");

            var newDevice = new HOTASDevice { DeviceId = ignoreGuid, Name = "new device 1" };
            AddHotasButtonMap(newDevice.ButtonMap, existingButtonMapId, HOTASButtonMap.ButtonType.Button, "ignore button");
            subDeviceList.GetHOTASDevices().Returns(new ObservableCollection<HOTASDevice>() { newDevice });

            hotasVm.Initialize();
            hotasVm.RefreshDeviceListCommand.Execute(default);


            Assert.Equal(2, hotasVm.Devices.Count);
            Assert.Single(hotasVm.Devices.Where(d => d.Name == newDevice.Name));
            subDeviceList.Received().GetHOTASDevices();
            subDeviceList.Received().GetHOTASDevices();
            subDeviceList.Received().ListenToDevice(newDevice);
            subDeviceList.DidNotReceive().ReplaceDevice(newDevice);

        }

        [Fact]
        public void clear_activity_list_command_new_device()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out _, out _, out _, out _, out _);
            hotasVm.Activity.Add(new ActivityItem());
            hotasVm.Activity.Add(new ActivityItem());

            hotasVm.Initialize();
            hotasVm.ClearActivityListCommand.Execute(default);

            Assert.Empty(hotasVm.Activity);
        }

        [Fact]
        public void create_new_mode_profile_command_no_modes()
        {
            var hotasVm = CreateHotasCollectionViewModel(out var subEventAggregator, out var subDeviceList, out _, out _, out _, out _);

            var activationButtons = new Dictionary<int, ModeActivationItem>();
            subDeviceList.ModeProfileActivationButtons.Returns(activationButtons);

            hotasVm.Initialize();
            hotasVm.CreateNewModeProfileCommand.Execute(default);

            subDeviceList.Received().SetupNewModeProfile();
            subDeviceList.Received().SetMode(0);
            subDeviceList.Received().ApplyActivationButtonToAllProfiles();
            subEventAggregator.ReceivedWithAnyArgs().Publish(new ShowMessageWindowEvent());
            subEventAggregator.ReceivedWithAnyArgs().Publish(new ShowModeProfileConfigWindowEvent(0, new Dictionary<int, ModeActivationItem>(), null, null, null));
        }

        [Fact]
        public void create_new_mode_profile_command_existing_modes_no_device()
        {
            var deviceGuid = Guid.NewGuid();
            const int modeActivationButtonId = 1000;

            var hotasVm = CreateHotasCollectionViewModel(out var subEventAggregator, out var subDeviceList, out _, out _, out _, out _);
            subDeviceList.ModeProfileActivationButtons.Add(1, new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid });

            hotasVm.Initialize();
            hotasVm.CreateNewModeProfileCommand.Execute(default);

            subDeviceList.Received().SetupNewModeProfile();
            subDeviceList.Received().SetMode(0);
            subDeviceList.Received().ApplyActivationButtonToAllProfiles();
            subEventAggregator.DidNotReceiveWithAnyArgs().Publish(new ShowMessageWindowEvent());
            subEventAggregator.ReceivedWithAnyArgs().Publish(new ShowModeProfileConfigWindowEvent(0, new Dictionary<int, ModeActivationItem>(), null, null, null));
        }

        [Fact]
        public void edit_mode_profile_command()
        {
            var deviceGuid = Guid.NewGuid();
            const int modeActivationButtonId = 1000;
            const int expectedMode = 1;

            var hotasVm = CreateHotasCollectionViewModel(out var subEventAggregator, out var subDeviceList, out _, out _, out _, out _);
            var item = new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid, Mode = expectedMode };
            subDeviceList.ModeProfileActivationButtons.Add(expectedMode, item);


            hotasVm.Initialize();
            hotasVm.EditModeProfileCommand.Execute(item);

            subDeviceList.DidNotReceive().SetupNewModeProfile();
            subDeviceList.DidNotReceive().SetMode(0);
            subDeviceList.Received().ApplyActivationButtonToAllProfiles();
            subEventAggregator.DidNotReceiveWithAnyArgs().Publish(new ShowMessageWindowEvent());
            subEventAggregator.ReceivedWithAnyArgs().Publish(new ShowModeProfileConfigWindowEvent(0, new Dictionary<int, ModeActivationItem>(), null, null, null));
        }

        [Fact]
        public void delete_new_mode_profile_command_existing_modes()
        {
            var deviceGuid = Guid.NewGuid();
            const int modeActivationButtonId = 1000;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            var item = new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid, Mode = 1 };
            subDeviceList.ModeProfileActivationButtons.Add(1, item);
            subDeviceList.RemoveModeProfile(item).Returns(true);

            hotasVm.Initialize();
            hotasVm.DeleteModeProfileCommand.Execute(item);

            subDeviceList.Received().RemoveModeProfile(item);
        }

        [Fact]
        public void delete_new_mode_profile_command_via_publish()
        {
            var deviceGuid = Guid.NewGuid();
            const int modeActivationButtonId = 1000;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            var item = new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid, Mode = 1 };
            subDeviceList.ModeProfileActivationButtons.Add(1, item);
            subDeviceList.RemoveModeProfile(item).Returns(true);

            hotasVm.Initialize();
            hotasVm.DeleteModeProfileCommand.Execute(item);
            subDeviceList.Received().RemoveModeProfile(item);
        }

        [Fact]
        public void create_new_mode_profile_command_from_template()
        {
            var hotasVm = CreateHotasCollectionViewModel(out var subEventAggregator, out var subDeviceList, out _, out _, out _, out _);

            var activationButtons = new Dictionary<int, ModeActivationItem>();
            activationButtons.Add(1, new ModeActivationItem(){Mode = 1, ButtonName = "bob", ProfileName = "bobs profile"});
            activationButtons.Add(2, new ModeActivationItem(){Mode = 2, ButtonName = "sponge", ProfileName = "sponges profile", TemplateMode = 1});
            subDeviceList.ModeProfileActivationButtons.Returns(activationButtons);

            subDeviceList.SetupNewModeProfile().Returns(2);
            hotasVm.Initialize();
            hotasVm.CreateNewModeProfileCommand.Execute(default);

            subDeviceList.Received().CopyModeProfileFromTemplate(1, 2);
        }

        [Fact]
        public void show_input_graph()
        {
            var hotasVm = CreateHotasCollectionViewModel(out var subEventAggregator, out _, out _, out _, out _, out _);

            hotasVm.Initialize();
            hotasVm.ShowInputGraphWindowCommand.Execute(default);

            subEventAggregator.ReceivedWithAnyArgs().Publish(new ShowInputGraphWindowEvent(null, null));
        }

        [Fact]
        public void initialize()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            subDeviceList.Devices.Add(new HOTASDevice());

            hotasVm.Initialize();
            subDeviceList.Received().Start();
            Assert.Equal(2, hotasVm.Devices.Count);
        }

        [Fact]
        public void dispose()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);

            hotasVm.Dispose();
            subDeviceList.Received().Stop();
        }

        [Fact]
        public void on_selection_changed_dispose()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out _, out _, out _, out _, out _);
            var d = new DeviceViewModel();
            hotasVm.lstDevices_OnSelectionChanged(d);
            Assert.Same(d, hotasVm.SelectedDevice);
        }

        [Fact]
        public void show_main_window()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out _, out _, out _, out _, out _);

            var receivedEvents = new List<EventArgs>();

            hotasVm.ShowMainWindow += delegate (object sender, EventArgs e)
            {
                receivedEvents.Add(e);
            };

            hotasVm.QuickProfilePanelViewModel.ShowWindow();

            Assert.Single(receivedEvents);
        }

        [Fact]
        public void add_activity_keystroke_down()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            hotasVm.Initialize();

            IHOTASQueue queue = new HOTASQueue();
            IHotasBaseMap map = new HOTASButtonMap() { Type = HOTASButtonMap.ButtonType.Button, ActionName = "test action" };
            queue.SetButtonMap(new ObservableCollection<IHotasBaseMap>() { map });

            subDeviceList.KeystrokeDownSent += Raise.EventWith(queue, new KeystrokeSentEventArgs(0, 0, 0, 0));

            Assert.Single(hotasVm.Activity);
        }

        [Fact]
        public void add_activity_keystroke_up()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            hotasVm.Initialize();

            IHOTASQueue queue = new HOTASQueue();
            var map = new HOTASButtonMap() { Type = HOTASButtonMap.ButtonType.Button, ActionName = "test action" };
            queue.SetButtonMap(new ObservableCollection<IHotasBaseMap>() { map });

            subDeviceList.KeystrokeUpSent += Raise.EventWith(queue, new KeystrokeSentEventArgs(0, 0, 0, 0));

            Assert.Single(hotasVm.Activity);
        }

        [Fact]
        public void add_activity_keystroke_down_for_not_axis()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            hotasVm.Initialize();

            IHOTASQueue queue = new HOTASQueue();
            var map = new HOTASButtonMap() { Type = HOTASButtonMap.ButtonType.AxisLinear };

            queue.SetButtonMap(new ObservableCollection<IHotasBaseMap>() { map });

            subDeviceList.KeystrokeDownSent += Raise.EventWith(queue, new KeystrokeSentEventArgs(0, 0, 0, 0));

            //test if button type says Axis, but map is something else, we should not add an activity
            Assert.Empty(hotasVm.Activity);
        }

        [Fact]
        public void add_activity_keystroke_down_for_axis_forward_direction()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            hotasVm.Initialize();

            IHOTASQueue queue = new HOTASQueue();
            var axisMap = new HOTASAxisMap() { Type = HOTASButtonMap.ButtonType.AxisLinear, MapName = "forward", IsDirectional = false };
            axisMap.ButtonMap = new ObservableCollection<HOTASButtonMap>() { new HOTASButtonMap() { ActionName = "button 1", MapName = "forward" } };
            axisMap.ReverseButtonMap = new ObservableCollection<HOTASButtonMap>() { new HOTASButtonMap() { ActionName = "button 2", MapName = "reverse" } };

            queue.SetButtonMap(new ObservableCollection<IHotasBaseMap>() { axisMap });

            subDeviceList.KeystrokeDownSent += Raise.EventWith(queue, new KeystrokeSentEventArgs(0, 0, 0, 0));

            Assert.Single(hotasVm.Activity);
            Assert.Equal("button 1", hotasVm.Activity.FirstOrDefault()?.ActionName);
        }

        [Fact]
        public void add_activity_keystroke_down_for_axis_reverse_direction()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            hotasVm.Initialize();

            IHOTASQueue queue = new HOTASQueue();
            var axisMap = new HOTASAxisMap() { Type = HOTASButtonMap.ButtonType.AxisLinear, MapName = "forward", IsDirectional = true };
            axisMap.ButtonMap = new ObservableCollection<HOTASButtonMap>() { new HOTASButtonMap() { ActionName = "button 1", MapName = "forward" } };
            axisMap.ReverseButtonMap = new ObservableCollection<HOTASButtonMap>() { new HOTASButtonMap() { ActionName = "button 2", MapName = "reverse" } };

            Assert.True(axisMap.Direction == AxisDirection.Forward);
            axisMap.SetAxis(800);
            axisMap.SetAxis(800);
            axisMap.SetAxis(800);
            axisMap.SetAxis(800);
            axisMap.SetAxis(800);
            axisMap.SetAxis(800);
            axisMap.SetAxis(800);
            axisMap.SetAxis(800);
            axisMap.SetAxis(800);
            axisMap.SetAxis(800);
            axisMap.SetAxis(700);

            Assert.True(axisMap.Direction == AxisDirection.Backward);

            queue.SetButtonMap(new ObservableCollection<IHotasBaseMap>() { axisMap });
            subDeviceList.KeystrokeDownSent += Raise.EventWith(queue, new KeystrokeSentEventArgs(0, 0, 0, 0));

            Assert.Single(hotasVm.Activity);
            Assert.Equal("button 2", hotasVm.Activity.FirstOrDefault()?.ActionName);
        }
    }
}

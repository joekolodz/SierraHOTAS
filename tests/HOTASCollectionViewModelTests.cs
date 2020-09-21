using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Permissions;
using System.Windows.Threading;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NSubstitute.Routing.Handlers;
using SierraHOTAS.Models;
using Xunit;
using Pose;
using SharpDX.DirectInput;
using SierraHOTAS.Factories;
using SierraHOTAS.ModeProfileWindow.ViewModels;
using SierraHOTAS.ViewModels;
using Xunit.Abstractions;
using Xunit.Sdk;

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

        private static HOTASCollectionViewModel CreateHotasCollectionViewModel(out IEventAggregator subEventAggregator, out IHOTASCollection subHotasCollection, out IFileSystem subFileSystem, out Dictionary<int, ModeActivationItem> subModeProfileButtons, out ActionCatalogViewModel subActionVm)
        {
            subFileSystem = Substitute.For<IFileSystem>();

            subHotasCollection = Substitute.For<IHOTASCollection>();
            subHotasCollection.Devices = new ObservableCollection<HOTASDevice>() { new HOTASDevice() };

            subModeProfileButtons = new Dictionary<int, ModeActivationItem>();
            subHotasCollection.ModeProfileActivationButtons.Returns(subModeProfileButtons);

            subActionVm = Substitute.For<ActionCatalogViewModel>();

            subEventAggregator = Substitute.For<IEventAggregator>();

            var hotasVm = new HOTASCollectionViewModel(Dispatcher.CurrentDispatcher, subEventAggregator, subFileSystem, subHotasCollection, subActionVm);
            return hotasVm;
        }

        private static HOTASButtonMap AddHotasButtonMap(ObservableCollection<IHotasBaseMap> buttons, int existingButtonMapId = 1, HOTASButtonMap.ButtonType type = HOTASButtonMap.ButtonType.Button, string mapName = "Button1", string actionName = "Fire", int scanCode = 43)
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
        public void set_mode()
        {
            const int expectedMode = 43;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subHotasCollection, out _, out _, out _);
            subHotasCollection.Mode = expectedMode;

            var receivedEvents = new List<int>();

            hotasVm.ModeProfileChanged += delegate (object sender, ModeProfileChangedEventArgs e)
            {
                receivedEvents.Add(e.Mode);
            };

            //Test
            hotasVm.Initialize();
            hotasVm.SetMode(expectedMode);

            Assert.Equal(expectedMode, receivedEvents[0]);
            subHotasCollection.Received().SetMode(expectedMode);
            //Assert that this should have been called
            hotasVm.Devices[0].RebuildMap();
            //verify hotasqueue.buttmap has the right buttonmap
        }

        [Fact]
        public void selection_changed_command()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out _, out _, out _, out _);
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

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subHotasCollection, out var subFileSystem, out _, out _);
            subFileSystem.LastSavedFileName = expectedProfileSetFileName;
            hotasVm.Initialize();
            hotasVm.SaveFileCommand.Execute(default);

            subFileSystem.ReceivedWithAnyArgs().FileSave(default);
            subFileSystem.DidNotReceive().FileSaveAs(default);
            subHotasCollection.Received().ClearUnassignedActions();
            Assert.Equal(expectedProfileSetFileName, hotasVm.ProfileSetFileName);
        }

        [Fact]
        public void file_save_as_command()
        {
            const string expectedProfileSetFileName = "Test File Name";

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subHotasCollection, out var subFileSystem, out _, out _);
            subFileSystem.LastSavedFileName = expectedProfileSetFileName;
            hotasVm.Initialize();
            hotasVm.SaveFileAsCommand.Execute(default);

            subFileSystem.ReceivedWithAnyArgs().FileSaveAs(default);
            subFileSystem.DidNotReceive().FileSave(default);
            subHotasCollection.DidNotReceive().ClearUnassignedActions();
            Assert.Equal(expectedProfileSetFileName, hotasVm.ProfileSetFileName);
        }

        [Fact]
        public void file_open_command_valid_file()
        {
            var deviceGuid = Guid.NewGuid();
            const int existingButtonMapId = 4300;
            const int loadedButtonMapId = 2700;
            const int modeActivationButtonId = 1000;

            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subDirectInput = Substitute.For<IDirectInput>();
            subDirectInputFactory.CreateDirectInput().Returns(subDirectInput);

            var subJoystick = Substitute.For<IJoystick>();
            subJoystick.Capabilities.Returns(new Capabilities());
            var subJoystickFactory = Substitute.For<JoystickFactory>();
            subJoystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);

            var subHotasQueueFactory = Substitute.For<HOTASQueueFactory>();
            var subHotasQueue = Substitute.For<IHOTASQueue>();
            subHotasQueueFactory.CreateHOTASQueue().Returns(subHotasQueue);

            var loadedHotasCollection = new HOTASCollection(subDirectInputFactory, subJoystickFactory, subHotasQueueFactory);
            loadedHotasCollection.Devices.Add(new HOTASDevice(subDirectInput, subJoystickFactory, deviceGuid, "loaded device", subHotasQueue));
            AddHotasButtonMap(loadedHotasCollection.Devices[0].ButtonMap, loadedButtonMapId, HOTASButtonMap.ButtonType.Button, "Button1", "Release");

            loadedHotasCollection.ModeProfileActivationButtons.Add(1, new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid });

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subHotasCollection, out var subFileSystem, out _, out _);

            var existingDevice = subHotasCollection.Devices[0];
            existingDevice.DeviceId = deviceGuid;
            existingDevice.Name = "existing device";
            AddHotasButtonMap(existingDevice.ButtonMap, existingButtonMapId);

            subHotasCollection.GetDevice(deviceGuid).Returns(existingDevice);

            subFileSystem.FileOpenDialog().Returns(loadedHotasCollection);



            hotasVm.Initialize();
            hotasVm.OpenFileCommand.Execute(default);


            //check that the in-memory button (existing) is replaced by the one loaded from the file
            Assert.Equal(loadedButtonMapId, hotasVm.Devices[0].ButtonMap[0].ButtonId);

            //mode profiles should be loaded
            Assert.Equal(modeActivationButtonId, subHotasCollection.ModeProfileActivationButtons[1].ButtonId);

            //one device vm will have been created
            Assert.Single(hotasVm.Devices);
            Assert.Equal(deviceGuid, hotasVm.Devices[0].InstanceId);

            //check action catalog is rebuilt with the button loaded from file
            Assert.Equal(2, hotasVm.ActionCatalog.Catalog.Count);
            Assert.Equal("<No Action>", hotasVm.ActionCatalog.Catalog[0].ActionName);
            Assert.Equal("Release", hotasVm.ActionCatalog.Catalog[1].ActionName);

            subHotasCollection.Received().Stop();
            subFileSystem.Received().FileOpenDialog();
            subHotasCollection.Received().AutoSetMode();
            subHotasCollection.Received().ListenToAllDevices();
        }

        [Fact]
        public void file_open_command_null_file()
        {
            var deviceGuid = Guid.NewGuid();
            const int existingButtonMapId = 4300;
            const int loadedButtonMapId = 2700;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subHotasCollection, out var subFileSystem, out _, out _);

            var existingDevice = subHotasCollection.Devices[0];
            existingDevice.DeviceId = deviceGuid;
            existingDevice.Name = "existing device";
            existingDevice.ButtonMap.Add(new HOTASButtonMap() { MapId = existingButtonMapId, Type = HOTASButtonMap.ButtonType.Button, MapName = "Button1", ActionName = "Fire" });
            subHotasCollection.GetDevice(deviceGuid).Returns(existingDevice);

            subFileSystem.FileOpenDialog().Returns((HOTASCollection)null);

            hotasVm.Initialize();
            hotasVm.OpenFileCommand.Execute(default);


            //check that the in-memory button (existing) is replaced by the one loaded from the file
            Assert.Equal(existingButtonMapId, hotasVm.Devices[0].ButtonMap[0].ButtonId);
            Assert.NotEqual(loadedButtonMapId, hotasVm.Devices[0].ButtonMap[0].ButtonId);

            Assert.Single(hotasVm.ActionCatalog.Catalog);
            Assert.Equal("<No Action>", hotasVm.ActionCatalog.Catalog[0].ActionName);

            subHotasCollection.DidNotReceive().Stop();
            subFileSystem.Received().FileOpenDialog();
            subHotasCollection.DidNotReceive().AutoSetMode();
            subHotasCollection.DidNotReceive().ListenToAllDevices();
        }

        [Fact]
        public void clear_active_profile_set_command()
        {
            var deviceGuid = Guid.NewGuid();
            const int existingButtonMapId = 4300;
            const int modeActivationButtonId = 1000;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subHotasCollection, out var subFileSystem, out _, out _);

            const string expectedPropertyChanged = nameof(hotasVm.ModeActivationItems);
            var receivedEvents = new List<string>();
            hotasVm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            var existingDevice = subHotasCollection.Devices[0];
            existingDevice.DeviceId = deviceGuid;
            existingDevice.Name = "existing device";
            AddHotasButtonMap(existingDevice.ButtonMap, existingButtonMapId);
            AddHotasButtonMap(existingDevice.ButtonMap, 0, HOTASButtonMap.ButtonType.Button, "Button2 - remove my actions", "<No Action>", 1);

            
            subHotasCollection.ModeProfileActivationButtons.Add(1, new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid });

            subFileSystem.FileOpenDialog().Returns((HOTASCollection)null);

            hotasVm.Initialize();

            var existingButtonmapVm = hotasVm.Devices[0].ButtonMap[0];

            hotasVm.ClearActiveProfileSetCommand.Execute(default);

            var expectedButtonMapVm = hotasVm.Devices[0].ButtonMap[0];

            Assert.NotEqual(expectedButtonMapVm, existingButtonmapVm);
            Assert.Empty(subHotasCollection.ModeProfileActivationButtons);
            Assert.Empty(((HOTASButtonMap)existingDevice.ButtonMap[1]).ActionCatalogItem.Actions);
            Assert.Equal(2, receivedEvents.Count);
            Assert.Equal(expectedPropertyChanged, receivedEvents[0]);
            Assert.Empty(hotasVm.ProfileSetFileName);
            subHotasCollection.Received().ClearButtonMap();
        }

        [Fact]
        public void refresh_device_list_command_rescan_existing()
        {
            var deviceGuid = Guid.NewGuid();
            var ignoreGuid = Guid.NewGuid();
            const int existingButtonMapId = 4300;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subHotasCollection, out _, out _, out _);

            var existingDevice = subHotasCollection.Devices[0];
            existingDevice.DeviceId = deviceGuid;
            existingDevice.Name = "existing device 1";
            AddHotasButtonMap(existingDevice.ButtonMap, existingButtonMapId, HOTASButtonMap.ButtonType.Button, "existing button");

            var rescannedDevice = new HOTASDevice { DeviceId = deviceGuid, Name = "rescanned device 1" };
            AddHotasButtonMap(rescannedDevice.ButtonMap, existingButtonMapId, HOTASButtonMap.ButtonType.Button, "rescanned button");
            subHotasCollection.RescanDevices().Returns(new ObservableCollection<HOTASDevice>() { rescannedDevice });

            hotasVm.Initialize();
            hotasVm.RefreshDeviceListCommand.Execute(default);

            Assert.Single(hotasVm.Devices);
            Assert.Equal(hotasVm.Devices[0].Name, rescannedDevice.Name);
            subHotasCollection.Received().RescanDevices();
            subHotasCollection.Received().ListenToDevice(rescannedDevice);
            subHotasCollection.Received().ReplaceDevice(rescannedDevice);
        }
        [Fact]
        public void refresh_device_list_command_new_device()
        {
            var deviceGuid = Guid.NewGuid();
            var ignoreGuid = Guid.NewGuid();
            const int existingButtonMapId = 4300;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subHotasCollection, out _, out _, out _);

            var existingDevice = subHotasCollection.Devices[0];
            existingDevice.DeviceId = deviceGuid;
            existingDevice.Name = "existing device 1";
            AddHotasButtonMap(existingDevice.ButtonMap, existingButtonMapId, HOTASButtonMap.ButtonType.Button, "existing button");

            var newDevice = new HOTASDevice { DeviceId = ignoreGuid, Name = "new device 1" };
            AddHotasButtonMap(newDevice.ButtonMap, existingButtonMapId, HOTASButtonMap.ButtonType.Button, "ignore button");
            subHotasCollection.RescanDevices().Returns(new ObservableCollection<HOTASDevice>() { newDevice });

            hotasVm.Initialize();
            hotasVm.RefreshDeviceListCommand.Execute(default);


            Assert.Equal(2, hotasVm.Devices.Count);
            Assert.Single(hotasVm.Devices.Where(d => d.Name == newDevice.Name));
            subHotasCollection.Received().RescanDevices();
            subHotasCollection.Received().RescanDevices();
            subHotasCollection.Received().ListenToDevice(newDevice);
            subHotasCollection.DidNotReceive().ReplaceDevice(newDevice);

        }

        [Fact]
        public void clear_activity_list_command_new_device()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out _, out _, out _, out _);
            hotasVm.Activity.Add(new ActivityItem());
            hotasVm.Activity.Add(new ActivityItem());

            hotasVm.Initialize();
            hotasVm.ClearActivityListCommand.Execute(default);

            Assert.Empty(hotasVm.Activity);
        }

//        [Fact]
        public void create_new_mode_profile_command_no_modes()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subHotasCollection, out _, out _, out _);

            hotasVm.Initialize();
            hotasVm.CreateNewModeProfileCommand.Execute(default);

            Assert.Empty(hotasVm.Activity);
        }

        [Fact]
        public void create_new_mode_profile_command_existing_modes()
        {
            var deviceGuid = Guid.NewGuid();
            const int modeActivationButtonId = 1000;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subHotasCollection, out _, out _, out _);
            subHotasCollection.ModeProfileActivationButtons.Add(1, new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid });


            hotasVm.Initialize();
            hotasVm.ClearActivityListCommand.Execute(default);

            Assert.Empty(hotasVm.Activity);
        }

    }
}

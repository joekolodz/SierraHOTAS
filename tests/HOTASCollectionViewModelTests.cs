﻿using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Pose;
using SharpDX.DirectInput;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using NLog.LayoutRenderers;
using SierraHOTAS.Controls;
using SierraHOTAS.Win32;
using Xunit;
using Xunit.Abstractions;
using EventArgs = System.EventArgs;

namespace SierraHOTAS.Tests
{
    public class HOTASCollectionViewModelTests
    {
        //private ITestOutputHelper _output;
        //public HOTASCollectionViewModelTests(ITestOutputHelper output)
        //{
        //    _output = output;
        //}

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

        private class TestDispatcher_AxisChanged : IDispatcher
        {
            public void Invoke(Action callback)
            {
                callback.Invoke();
            }
        }

        private static HOTASCollectionViewModel CreateHotasCollectionViewModel(out IEventAggregator subEventAggregator, out IHOTASCollection subHotasCollection, out IFileSystem subFileSystem, out MediaPlayerFactory subMediaPlayerFactory, out Dictionary<int, ModeActivationItem> subModeButtons)
        {
            return CreateHotasCollectionViewModel(out subEventAggregator, out subHotasCollection, out subFileSystem, out subMediaPlayerFactory, out subModeButtons, out _);
        }

        private static HOTASCollectionViewModel CreateHotasCollectionViewModel_WithEventAggregator(out IEventAggregator eventAggregator, out IHOTASCollection subHotasCollection)
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var deviceViewModelFactory = new DeviceViewModelFactory();

            subHotasCollection = Substitute.For<IHOTASCollection>();
            subHotasCollection.Devices = new ObservableCollection<IHOTASDevice>() { new HOTASDevice(Substitute.For<IDirectInput>(), Guid.Empty, Guid.NewGuid(), "test device", Substitute.For<IHOTASQueue>()) };

            var subModeButtons = new Dictionary<int, ModeActivationItem>();
            subHotasCollection.ModeActivationButtons.Returns(subModeButtons);
            subHotasCollection.ActionCatalog.Returns(Substitute.For<ActionCatalog>());

            eventAggregator = new EventAggregator();

            IDispatcher testDispatcher = new TestDispatcher_AxisChanged();
            subDispatcherFactory.CreateDispatcher().Returns(d => testDispatcher);

            var subQuickProfilePanelVm = new QuickProfilePanelViewModel(eventAggregator, subFileSystem);


            var hotasVm = new HOTASCollectionViewModel(subDispatcherFactory, eventAggregator, subFileSystem, subMediaPlayerFactory, subHotasCollection, subQuickProfilePanelVm, deviceViewModelFactory);
            return hotasVm;
        }

        private static HOTASCollectionViewModel CreateHotasCollectionViewModel_WithEventAggregator(IFileIO subFileIo, out IEventAggregator eventAggregator)
        {
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var deviceViewModelFactory = new DeviceViewModelFactory();

            var hotasCollection = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), new HOTASDeviceFactory(), new ActionCatalog());

            eventAggregator = new EventAggregator();

            IDispatcher testDispatcher = new TestDispatcher_AxisChanged();
            subDispatcherFactory.CreateDispatcher().Returns(d => testDispatcher);

            var subQuickProfilePanelVm = Substitute.For<QuickProfilePanelViewModel>();
            subQuickProfilePanelVm.GetAutoLoadPath().Returns("test path");

            var hotasVm = new HOTASCollectionViewModel(subDispatcherFactory, eventAggregator, new FileSystem(subFileIo, Substitute.For<FileDialogFactory>()), subMediaPlayerFactory, hotasCollection, subQuickProfilePanelVm, deviceViewModelFactory);
            return hotasVm;
        }
        private static HOTASCollectionViewModel CreateHotasCollectionViewModel(out IHOTASCollection hotasCollection, out IFileSystem subFileSystem, out QuickProfilePanelViewModel subQuickProfilePanelVm)
        {
            subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var deviceViewModelFactory = new DeviceViewModelFactory();

            hotasCollection = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), Substitute.For<HOTASDeviceFactory>(), Substitute.For<ActionCatalog>());
            hotasCollection.Devices = new ObservableCollection<IHOTASDevice>() { new HOTASDevice() { ProductId = Guid.NewGuid(), DeviceId = Guid.NewGuid() } };
            hotasCollection.ModeActivationButtons.Add(1, new ModeActivationItem());

            var subEventAggregator = new EventAggregator();

            IDispatcher testDispatcher = new TestDispatcher_AxisChanged();
            subDispatcherFactory.CreateDispatcher().Returns(d => testDispatcher);

            subQuickProfilePanelVm = Substitute.For<QuickProfilePanelViewModel>();

            var hotasVm = new HOTASCollectionViewModel(subDispatcherFactory, subEventAggregator, subFileSystem, subMediaPlayerFactory, hotasCollection, subQuickProfilePanelVm, deviceViewModelFactory);
            return hotasVm;
        }

        private static HOTASCollectionViewModel CreateHotasCollectionViewModel(out IFileSystem subFileSystem, out QuickProfilePanelViewModel subQuickProfilePanelVm)
        {
            subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var deviceViewModelFactory = new DeviceViewModelFactory();

            var hotasCollection = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), Substitute.For<HOTASDeviceFactory>(), Substitute.For<ActionCatalog>());
            hotasCollection.Devices = new ObservableCollection<IHOTASDevice>() { new HOTASDevice() { ProductId = Guid.NewGuid(), DeviceId = Guid.NewGuid() } };
            hotasCollection.ModeActivationButtons.Add(1, new ModeActivationItem());

            var subEventAggregator = new EventAggregator();

            IDispatcher testDispatcher = new TestDispatcher_AxisChanged();
            subDispatcherFactory.CreateDispatcher().Returns(d => testDispatcher);

            subQuickProfilePanelVm = Substitute.For<QuickProfilePanelViewModel>();

            var hotasVm = new HOTASCollectionViewModel(subDispatcherFactory, subEventAggregator, subFileSystem, subMediaPlayerFactory, hotasCollection, subQuickProfilePanelVm, deviceViewModelFactory);
            return hotasVm;
        }

        private static HOTASCollectionViewModel CreateHotasCollectionViewModel_OnlySubFileIO(IFileIO subFileIo)
        {
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var deviceViewModelFactory = new DeviceViewModelFactory();

            var hotasCollection = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), new HOTASDeviceFactory(), new ActionCatalog());

            var subEventAggregator = new EventAggregator();

            IDispatcher testDispatcher = new TestDispatcher_AxisChanged();
            subDispatcherFactory.CreateDispatcher().Returns(d => testDispatcher);

            var subQuickProfilePanelVm = Substitute.For<QuickProfilePanelViewModel>();
            subQuickProfilePanelVm.GetAutoLoadPath().Returns("test path");

            var hotasVm = new HOTASCollectionViewModel(subDispatcherFactory, subEventAggregator, new FileSystem(subFileIo, Substitute.For<FileDialogFactory>()), subMediaPlayerFactory, hotasCollection, subQuickProfilePanelVm, deviceViewModelFactory);
            return hotasVm;
        }

        private static HOTASCollectionViewModel CreateHotasCollectionViewModel_WithDispatcher(out IFileSystem subFileSystem, out QuickProfilePanelViewModel subQuickProfilePanelVm)
        {
            subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subDispatcherFactory = new DispatcherFactory();
            var deviceViewModelFactory = new DeviceViewModelFactory();

            var hotasCollection = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), Substitute.For<HOTASDeviceFactory>(), Substitute.For<ActionCatalog>());
            hotasCollection.Devices = new ObservableCollection<IHOTASDevice>() { new HOTASDevice() { ProductId = Guid.NewGuid(), DeviceId = Guid.NewGuid() } };
            hotasCollection.ModeActivationButtons.Add(1, new ModeActivationItem());

            var subEventAggregator = new EventAggregator();

            subQuickProfilePanelVm = Substitute.For<QuickProfilePanelViewModel>();

            var hotasVm = new HOTASCollectionViewModel(subDispatcherFactory, subEventAggregator, subFileSystem, subMediaPlayerFactory, hotasCollection, subQuickProfilePanelVm, deviceViewModelFactory);
            return hotasVm;
        }

        private static HOTASCollectionViewModel CreateHotasCollectionViewModel_DeviceVmFactorySub(out IFileSystem subFileSystem, out QuickProfilePanelViewModel subQuickProfilePanelVm, out DeviceViewModelFactory subDeviceViewModelFactory)
        {
            subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            subDeviceViewModelFactory = Substitute.For<DeviceViewModelFactory>();

            var hotasCollection = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), Substitute.For<HOTASDeviceFactory>(), Substitute.For<ActionCatalog>());
            hotasCollection.Devices = new ObservableCollection<IHOTASDevice>() { new HOTASDevice() { ProductId = Guid.NewGuid(), DeviceId = Guid.NewGuid() } };
            hotasCollection.ModeActivationButtons.Add(1, new ModeActivationItem());

            var subEventAggregator = new EventAggregator();
            IDispatcher testDispatcher = new TestDispatcher_AxisChanged();
            subDispatcherFactory.CreateDispatcher().Returns(d => testDispatcher);

            subQuickProfilePanelVm = Substitute.For<QuickProfilePanelViewModel>();

            var hotasVm = new HOTASCollectionViewModel(subDispatcherFactory, subEventAggregator, subFileSystem, subMediaPlayerFactory, hotasCollection, subQuickProfilePanelVm, subDeviceViewModelFactory);
            return hotasVm;
        }

        private static HOTASCollectionViewModel CreateHotasCollectionViewModel(out IEventAggregator subEventAggregator, out IHOTASCollection subHotasCollection, out IFileSystem subFileSystem, out MediaPlayerFactory subMediaPlayerFactory, out Dictionary<int, ModeActivationItem> subModeButtons, out QuickProfilePanelViewModel subQuickProfilePanelVm)
        {
            subFileSystem = Substitute.For<IFileSystem>();
            subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var deviceViewModelFactory = new DeviceViewModelFactory();

            var subJoystick = Substitute.For<IJoystick>();
            subJoystick.Capabilities.Returns(new Capabilities() { AxeCount = 2, ButtonCount = 1 });
            subJoystick.IsAxisPresent(Arg.Any<string>()).Returns(true);

            var subJoystickFactory = Substitute.For<JoystickFactory>();
            subJoystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);

            subHotasCollection = Substitute.For<IHOTASCollection>();
            subHotasCollection.Devices = new ObservableCollection<IHOTASDevice>() { new HOTASDevice(Substitute.For<IDirectInput>(), subJoystickFactory, Guid.NewGuid(), Guid.NewGuid(), "test joystick", Substitute.For<IHOTASQueue>()) };

            var subActionCatalaog = Substitute.For<ActionCatalog>();
            subHotasCollection.ActionCatalog.Returns(subActionCatalaog);

            subModeButtons = new Dictionary<int, ModeActivationItem>();
            subHotasCollection.ModeActivationButtons.Returns(subModeButtons);

            subEventAggregator = Substitute.For<IEventAggregator>();
            IDispatcher testDispatcher = new TestDispatcher_AxisChanged();
            subDispatcherFactory.CreateDispatcher().Returns(d => testDispatcher);

            subQuickProfilePanelVm = new QuickProfilePanelViewModel(subEventAggregator, subFileSystem);

            var hotasVm = new HOTASCollectionViewModel(subDispatcherFactory, subEventAggregator, subFileSystem, subMediaPlayerFactory, subHotasCollection, subQuickProfilePanelVm, deviceViewModelFactory);
            return hotasVm;
        }

        private HOTASCollection CreateHotasCollection()
        {
            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subJoystickFactory = Substitute.For<JoystickFactory>();
            var subHotasQueueFactory = Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>());
            var subHotasDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            var subActionCatalog = Substitute.For<ActionCatalog>();
            var hotasCollection = new HOTASCollection(subDirectInputFactory, subJoystickFactory, subHotasQueueFactory, subHotasDeviceFactory, subActionCatalog);
            return hotasCollection;
        }

        private HOTASCollection CreateHotasCollectionSubstitute()
        {
            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subJoystickFactory = Substitute.For<JoystickFactory>();
            var subHotasQueueFactory = Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>());
            var subHotasDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            var subHotasCollection = Substitute.For<HOTASCollection>(subDirectInputFactory, subJoystickFactory, subHotasQueueFactory, subHotasDeviceFactory, Substitute.For<ActionCatalog>());
            return subHotasCollection;
        }

        private static HOTASButton AddHotasButtonMap(ObservableCollection<IHotasBaseMap> buttons, int existingButtonMapId = 1, HOTASButton.ButtonType type = HOTASButton.ButtonType.Button, string mapName = "Button1", string actionName = "Fire", int scanCode = 43)
        {
            var map = CreateHotasButtonMap(existingButtonMapId, type, mapName, actionName, scanCode);
            buttons.Add(map);
            return map;
        }

        private static HOTASButton CreateHotasButtonMap(int existingButtonMapId = 1, HOTASButton.ButtonType type = HOTASButton.ButtonType.Button, string mapName = "Button1", string actionName = "Fire", int scanCode = 43)
        {
            var map = new HOTASButton()
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
                        new ButtonAction() {ScanCode = scanCode },
                        new ButtonAction() {IsExtended = true, ScanCode = scanCode },
                    }
                }
            };
            return map;
        }

        private static HOTASAxis AddHotasAxisMap(ObservableCollection<IHotasBaseMap> buttons, int existingButtonMapId = 1, HOTASButton.ButtonType type = HOTASButton.ButtonType.AxisLinear, string mapName = "Button1", string actionName = "Fire", int scanCode = 43)
        {
            var map = new HOTASAxis()
            {
                MapId = existingButtonMapId,
                Type = type,
                MapName = mapName,
                ButtonMap = new ObservableCollection<HOTASButton>()
                {
                    new HOTASButton()
                    {
                        ActionName = actionName,
                        ActionCatalogItem = new ActionCatalogItem()
                        {
                            ActionName = actionName,
                            Actions = new ObservableCollection<ButtonAction>()
                            {
                                new ButtonAction() { ScanCode = scanCode },
                                new ButtonAction() { IsExtended = true, ScanCode = scanCode },
                            }
                        }
                    }
                },
                ReverseButtonMap = new ObservableCollection<HOTASButton>()
                {
                    new HOTASButton()
                    {
                        ActionName = actionName,
                        ActionCatalogItem = new ActionCatalogItem()
                        {
                            ActionName = actionName,
                            Actions = new ObservableCollection<ButtonAction>()
                            {
                                new ButtonAction() { ScanCode = scanCode },
                                new ButtonAction() { IsExtended = true, ScanCode = scanCode },
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

            hotasVm.ModeChanged += delegate (object sender, ModeChangedEventArgs e)
            {
                receivedEvents.Add(e.Mode);
            };

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

            var buttonMap = new HOTASButton() { Type = HOTASButton.ButtonType.Button, MapName = firstExpectedButtonName };
            subDeviceList.Devices[0].ButtonMap.Add(buttonMap);

            hotasVm.Initialize();

            Assert.Equal(firstExpectedButtonName, hotasVm.Devices[0].ButtonMap[3].ButtonName);

            buttonMap.MapName = secondExpectedButtonName;

            hotasVm.SetMode(expectedMode);

            Assert.Equal(secondExpectedButtonName, hotasVm.Devices[0].ButtonMap[3].ButtonName);
        }

        [Fact]
        public void set_mode_exit_if_same_mode()
        {
            const int expectedMode = 43;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            subDeviceList.Mode = expectedMode;

            var receivedEvents = new List<int>();

            hotasVm.ModeChanged += delegate (object sender, ModeChangedEventArgs e)
            {
                receivedEvents.Add(e.Mode);
            };

            hotasVm.Initialize();
            hotasVm.SetMode(expectedMode);

            Assert.Empty(receivedEvents);
        }

        [Fact]
        public void selection_changed_command()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out _, out _, out _, out _, out _);
            var model = new DeviceViewModel() { Name = "Test" };

            hotasVm.Initialize();
            Assert.NotSame(model, hotasVm.SelectedDevice);

            hotasVm.SelectionChangedCommand.Execute(model);
            Assert.Same(model, hotasVm.SelectedDevice);
        }

        [Fact]
        public void selection_changed_public()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out _, out _, out _, out _, out _);
            var model = new DeviceViewModel() { Name = "Test" };

            hotasVm.Initialize();
            Assert.NotSame(model, hotasVm.SelectedDevice);

            hotasVm.lstDevices_OnSelectionChanged(model);
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

        //TODO - refactor to support action catalog persistence
        //[Fact]
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
            subJoystick.Capabilities.Returns(new Capabilities() { AxeCount = 2, ButtonCount = 1 });
            subJoystick.IsAxisPresent(Arg.Any<string>()).Returns(true);

            var subJoystickFactory = Substitute.For<JoystickFactory>();
            subJoystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);

            var subHotasQueueFactory = Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>());
            var subHotasQueue = Substitute.For<IHOTASQueue>();
            subHotasQueueFactory.CreateHOTASQueue().Returns(subHotasQueue);

            var subHotasDeviceFactory = Substitute.For<HOTASDeviceFactory>();

            var loadedHotasCollection = new HOTASCollection(subDirectInputFactory, subJoystickFactory, subHotasQueueFactory, subHotasDeviceFactory, Substitute.For<ActionCatalog>());
            loadedHotasCollection.Devices.Add(new HOTASDevice(subDirectInput, subJoystickFactory, productGuid, deviceGuid, "loaded device", subHotasQueue));

            var testMap = loadedHotasCollection.Devices[0].ButtonMap.First(m => m.MapId == 48) as HOTASButton;
            Assert.NotNull(testMap);
            var i = loadedHotasCollection.Devices[0].ButtonMap.IndexOf(testMap);
            loadedHotasCollection.Devices[0].ButtonMap[i] = CreateHotasButtonMap(testMap.MapId, HOTASButton.ButtonType.Button, "Button1", "Release");
            testMap.ActionName = expectedActionName;

            loadedHotasCollection.ModeActivationButtons.Add(1, new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid });

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
            Assert.Equal(modeActivationButtonId, subDeviceList.ModeActivationButtons[1].ButtonId);

            //one device vm will have been created
            Assert.Single(hotasVm.Devices);
            Assert.Equal(deviceGuid, hotasVm.Devices[0].InstanceId);

            //check action catalog is rebuilt with the button loaded from file
            Assert.Equal(2, hotasVm.ActionCatalog.Catalog.Count);
            Assert.Contains(hotasVm.ActionCatalog.Catalog, item => item.ActionName == ActionCatalogItem.NO_ACTION_TEXT);
            Assert.Contains(hotasVm.ActionCatalog.Catalog, item => item.ActionName == "Release");

            subDeviceList.Received().Stop();
            subFileSystem.Received().FileOpenDialog();
            subDeviceList.Received().AutoSetMode();
            subDeviceList.Received().ListenToAllDevices();
        }

        //TODO - refactor to support action catalog persistence
        //[Fact]
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
            subJoystick.IsAxisPresent(Arg.Any<string>()).Returns(true);

            var subJoystickFactory = Substitute.For<JoystickFactory>();
            subJoystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);

            var subHotasQueueFactory = Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>());
            var subHotasQueue = Substitute.For<IHOTASQueue>();
            subHotasQueueFactory.CreateHOTASQueue().Returns(subHotasQueue);

            var subHotasDeviceFactory = Substitute.For<HOTASDeviceFactory>();

            var loadedHotasCollection = new HOTASCollection(subDirectInputFactory, subJoystickFactory, subHotasQueueFactory, subHotasDeviceFactory, Substitute.For<ActionCatalog>());
            loadedHotasCollection.Devices.Add(new HOTASDevice(subDirectInput, subJoystickFactory, productGuid, existingDeviceId, "loaded device", subHotasQueue));

            var testMap = loadedHotasCollection.Devices[0].ButtonMap.First(m => m.MapId == 48) as HOTASButton;
            Assert.NotNull(testMap);
            var i = loadedHotasCollection.Devices[0].ButtonMap.IndexOf(testMap);
            loadedHotasCollection.Devices[0].ButtonMap[i] = CreateHotasButtonMap(testMap.MapId, HOTASButton.ButtonType.Button, "Button1", "Release");
            testMap.ActionName = expectedActionName;

            loadedHotasCollection.ModeActivationButtons.Add(1, new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = loadedDeviceId });

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
            Assert.Equal(modeActivationButtonId, subDeviceList.ModeActivationButtons[1].ButtonId);

            //one device vm will have been created
            Assert.Single(hotasVm.Devices);
            Assert.Equal(existingDeviceId, hotasVm.Devices[0].InstanceId);

            //check action catalog is rebuilt with the button loaded from file
            Assert.Equal(2, hotasVm.ActionCatalog.Catalog.Count);
            Assert.Contains(hotasVm.ActionCatalog.Catalog, item => item.ActionName == ActionCatalogItem.NO_ACTION_TEXT);
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
            existingDevice.ButtonMap.Add(new HOTASButton() { MapId = existingButtonMapId, Type = HOTASButton.ButtonType.Button, MapName = "Button1", ActionName = "Fire" });
            subDeviceList.GetDevice(deviceGuid).Returns(existingDevice);

            subFileSystem.FileOpenDialog().Returns((HOTASCollection)null);

            hotasVm.Initialize();
            hotasVm.OpenFileCommand.Execute(default);

            //check that the in-memory button (existing) is replaced by the one loaded from the file
            Assert.Equal(existingButtonMapId, hotasVm.Devices[0].ButtonMap[3].ButtonId);
            Assert.NotEqual(loadedButtonMapId, hotasVm.Devices[0].ButtonMap[3].ButtonId);

            Assert.Single(hotasVm.ActionCatalog.Catalog);
            Assert.Equal(ActionCatalogItem.NO_ACTION_TEXT, hotasVm.ActionCatalog.Catalog[0].ActionName);

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
            AddHotasButtonMap(existingDevice.ButtonMap, 0, HOTASButton.ButtonType.Button, "Button2 - remove my actions", ActionCatalogItem.NO_ACTION_TEXT, 1);
            subDeviceList.ModeActivationButtons.Add(1, new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid });

            subFileSystem.FileOpenDialog().Returns((HOTASCollection)null);

            hotasVm.Initialize();

            var existingButtonmapVm = hotasVm.Devices[0].ButtonMap[0];

            hotasVm.ResetProfileCommand.Execute(default);

            var expectedButtonMapVm = hotasVm.Devices[0].ButtonMap[0];

            Assert.NotEqual(expectedButtonMapVm, existingButtonmapVm);
            Assert.Empty(((HOTASButton)existingDevice.ButtonMap[2]).ActionCatalogItem.Actions);
            Assert.Equal(2, receivedEvents.Count);
            Assert.Equal(expectedPropertyChanged, receivedEvents[0]);
            Assert.Empty(hotasVm.ProfileSetFileName);
            subDeviceList.Received().ResetProfile();
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
            AddHotasButtonMap(existingDevice.ButtonMap, existingButtonMapId, HOTASButton.ButtonType.Button, "existing button");

            var rescannedDevice = new HOTASDevice { DeviceId = deviceGuid, Name = "rescanned device 1" };
            AddHotasButtonMap(rescannedDevice.ButtonMap, existingButtonMapId, HOTASButton.ButtonType.Button, "rescanned button");
            subDeviceList.RefreshMissingDevices().Returns(new ObservableCollection<IHOTASDevice>() { rescannedDevice });

            hotasVm.Initialize();
            hotasVm.RefreshDeviceListCommand.Execute(default);

            Assert.Single(hotasVm.Devices);
            Assert.Equal(hotasVm.Devices[0].Name, rescannedDevice.Name);
            subDeviceList.Received().RefreshMissingDevices();
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
            AddHotasButtonMap(existingDevice.ButtonMap, existingButtonMapId, HOTASButton.ButtonType.Button, "existing button");

            var newDevice = new HOTASDevice { DeviceId = ignoreGuid, Name = "new device 1" };
            AddHotasButtonMap(newDevice.ButtonMap, existingButtonMapId, HOTASButton.ButtonType.Button, "ignore button");
            subDeviceList.RefreshMissingDevices().Returns(new ObservableCollection<IHOTASDevice>() { newDevice });

            hotasVm.Initialize();
            hotasVm.RefreshDeviceListCommand.Execute(default);


            Assert.Equal(2, hotasVm.Devices.Count);
            Assert.Single(hotasVm.Devices.Where(d => d.Name == newDevice.Name));
            subDeviceList.Received().RefreshMissingDevices();
            subDeviceList.Received().RefreshMissingDevices();
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
            subDeviceList.ModeActivationButtons.Returns(activationButtons);

            hotasVm.Initialize();
            hotasVm.CreateNewModeCommand.Execute(default);

            subDeviceList.Received().SetupNewMode();
            subDeviceList.Received().SetMode(0);
            subDeviceList.Received().ApplyActivationButtonToAllProfiles();
            subEventAggregator.ReceivedWithAnyArgs().Publish(new ShowMessageWindowEvent());
            subEventAggregator.ReceivedWithAnyArgs().Publish(new ShowModeConfigWindowEvent(0, new Dictionary<int, ModeActivationItem>(), null, null, null));
        }

        [Fact]
        public void create_new_mode_profile_command_existing_modes_no_device()
        {
            var deviceGuid = Guid.NewGuid();
            const int modeActivationButtonId = 1000;

            var hotasVm = CreateHotasCollectionViewModel(out var subEventAggregator, out var subDeviceList, out _, out _, out _, out _);
            subDeviceList.ModeActivationButtons.Add(1, new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid });

            hotasVm.Initialize();
            hotasVm.CreateNewModeCommand.Execute(default);

            subDeviceList.Received().SetupNewMode();
            subDeviceList.Received().SetMode(0);
            subDeviceList.Received().ApplyActivationButtonToAllProfiles();
            subEventAggregator.DidNotReceiveWithAnyArgs().Publish(new ShowMessageWindowEvent());
            subEventAggregator.ReceivedWithAnyArgs().Publish(new ShowModeConfigWindowEvent(0, new Dictionary<int, ModeActivationItem>(), null, null, null));
        }

        [Fact]
        public void edit_mode_profile_command()
        {
            var deviceGuid = Guid.NewGuid();
            const int modeActivationButtonId = 1000;
            const int expectedMode = 1;

            var hotasVm = CreateHotasCollectionViewModel(out var subEventAggregator, out var subDeviceList, out _, out _, out _, out _);
            var item = new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid, Mode = expectedMode };
            subDeviceList.ModeActivationButtons.Add(expectedMode, item);


            hotasVm.Initialize();

            Assert.PropertyChanged(hotasVm, "ModeActivationItems", () => hotasVm.EditModeCommand.Execute(item));

            subDeviceList.DidNotReceive().SetupNewMode();
            subDeviceList.DidNotReceive().SetMode(0);
            subDeviceList.Received().ApplyActivationButtonToAllProfiles();
            subEventAggregator.DidNotReceiveWithAnyArgs().Publish(new ShowMessageWindowEvent());
            subEventAggregator.ReceivedWithAnyArgs().Publish(new ShowModeConfigWindowEvent(0, new Dictionary<int, ModeActivationItem>(), null, null, null));
        }

        [Fact]
        public void delete_new_mode_profile_command_existing_modes()
        {
            var deviceGuid = Guid.NewGuid();
            const int modeActivationButtonId = 1000;

            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            var item = new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid, Mode = 1 };
            subDeviceList.ModeActivationButtons.Add(1, item);
            subDeviceList.RemoveMode(item).Returns(true);

            hotasVm.Initialize();

            Assert.PropertyChanged(hotasVm, "ModeActivationItems", () => hotasVm.DeleteModeCommand.Execute(item));

            subDeviceList.Received().RemoveMode(item);
        }

        [Fact]
        public void delete_new_mode_profile_command_via_publish()
        {
            var deviceGuid = Guid.NewGuid();
            const int modeActivationButtonId = 1000;
            var hotasVm = CreateHotasCollectionViewModel_WithEventAggregator(out var eventAggregator, out var subDeviceList);
            var item = new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid, Mode = 1 };
            subDeviceList.ModeActivationButtons.Add(1, item);
            subDeviceList.RemoveMode(item).Returns(true);

            hotasVm.Initialize();
            Assert.PropertyChanged(hotasVm, "ModeActivationItems", () => eventAggregator.Publish(new DeleteModeEvent(item)));
            subDeviceList.Received().RemoveMode(item);
        }

        [Fact]
        public void mode_activation_idems_property_getter()
        {
            var deviceGuid = Guid.NewGuid();
            const int modeActivationButtonId = 1000;
            var hotasVm = CreateHotasCollectionViewModel_WithEventAggregator(out var eventAggregator, out IHOTASCollection subDeviceList);
            var item = new ModeActivationItem() { ButtonId = modeActivationButtonId, DeviceId = deviceGuid, Mode = 1 };
            subDeviceList.ModeActivationButtons.Add(1, item);
            subDeviceList.RemoveMode(item).Returns(true);

            hotasVm.Initialize();
            Assert.PropertyChanged(hotasVm, "ModeActivationItems", () => eventAggregator.Publish(new DeleteModeEvent(item)));
            subDeviceList.Received().RemoveMode(item);

            Assert.Equal(hotasVm.ModeActivationItems[0].DeviceId, subDeviceList.ModeActivationButtons[item.Mode].DeviceId);
        }

        [Fact]
        public void create_new_mode_profile_command_from_template()
        {
            var hotasVm = CreateHotasCollectionViewModel(out var subEventAggregator, out var subDeviceList, out _, out _, out _, out _);

            var activationButtons = new Dictionary<int, ModeActivationItem>();
            activationButtons.Add(1, new ModeActivationItem() { Mode = 1, ButtonName = "bob", ModeName = "bobs profile" });
            activationButtons.Add(2, new ModeActivationItem() { Mode = 2, ButtonName = "sponge", ModeName = "sponges profile", TemplateMode = 1 });
            subDeviceList.ModeActivationButtons.Returns(activationButtons);

            subDeviceList.SetupNewMode().Returns(2);
            hotasVm.Initialize();
            hotasVm.CreateNewModeCommand.Execute(default);

            subDeviceList.Received().CopyModeFromTemplate(1, 2);
        }

        [Fact]
        public void show_input_graph()
        {
            var hotasVm = CreateHotasCollectionViewModel(out var subEventAggregator, out _, out _, out _, out _, out _);

            hotasVm.Initialize();
            hotasVm.ShowInputGraphWindowCommand.Execute(default);

            subEventAggregator.ReceivedWithAnyArgs().Publish(new ShowInputGraphWindowEvent(null, null, null));
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
        public void add_activity_keystroke_down()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            hotasVm.Initialize();

            IHOTASQueue queue = new HOTASQueue(Substitute.For<IKeyboard>());
            IHotasBaseMap map = new HOTASButton() { Type = HOTASButton.ButtonType.Button, ActionName = "test action" };
            queue.SetModesCollection(new Dictionary<int, ObservableCollection<IHotasBaseMap>> { { 1, new ObservableCollection<IHotasBaseMap>(){map} } });
            queue.ActivateMode(1);

            subDeviceList.KeystrokeDownSent += Raise.EventWith(queue, new KeystrokeSentEventArgs(0, 0, 0, false, false));

            Assert.Single(hotasVm.Activity);
        }

        [Fact]
        public void add_activity_keystroke_up()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            hotasVm.Initialize();

            IHOTASQueue queue = new HOTASQueue(Substitute.For<IKeyboard>());
            var map = new HOTASButton() { Type = HOTASButton.ButtonType.Button, ActionName = "test action" };
            queue.SetModesCollection(new Dictionary<int, ObservableCollection<IHotasBaseMap>> { { 1, new ObservableCollection<IHotasBaseMap>(){map} } });
            queue.ActivateMode(1);

            subDeviceList.KeystrokeUpSent += Raise.EventWith(queue, new KeystrokeSentEventArgs(0, 0, 0, false, false));

            Assert.Single(hotasVm.Activity);
        }

        [Fact]
        public void add_activity_keystroke_down_for_not_axis()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            hotasVm.Initialize();

            IHOTASQueue queue = new HOTASQueue(Substitute.For<IKeyboard>());
            var map = new HOTASButton() { Type = HOTASButton.ButtonType.AxisLinear };
            queue.SetModesCollection(new Dictionary<int, ObservableCollection<IHotasBaseMap>> { { 1, new ObservableCollection<IHotasBaseMap>() { map } } });
            queue.ActivateMode(1);

            subDeviceList.KeystrokeDownSent += Raise.EventWith(queue, new KeystrokeSentEventArgs(0, 0, 0, false, false));

            //test if button type says Axis, but map is something else, we should not add an activity
            Assert.Empty(hotasVm.Activity);
        }

        [Fact]
        public void add_activity_keystroke_down_for_axis_forward_direction()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            hotasVm.Initialize();

            IHOTASQueue queue = new HOTASQueue(Substitute.For<IKeyboard>());
            var axisMap = new HOTASAxis() { Type = HOTASButton.ButtonType.AxisLinear, MapName = "forward", IsDirectional = false };
            axisMap.ButtonMap = new ObservableCollection<HOTASButton>() { new HOTASButton() { ActionName = "button 1", MapName = "forward" } };
            axisMap.ReverseButtonMap = new ObservableCollection<HOTASButton>() { new HOTASButton() { ActionName = "button 2", MapName = "reverse" } };
            queue.SetModesCollection(new Dictionary<int, ObservableCollection<IHotasBaseMap>> { { 1, new ObservableCollection<IHotasBaseMap>() { axisMap } } });
            queue.ActivateMode(1);

            subDeviceList.KeystrokeDownSent += Raise.EventWith(queue, new KeystrokeSentEventArgs(0, 0, 0, false, false));

            Assert.Single(hotasVm.Activity);
            Assert.Equal("button 1", hotasVm.Activity.FirstOrDefault()?.ActionName);
        }

        [Fact]
        public void add_activity_keystroke_down_for_axis_reverse_direction()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            hotasVm.Initialize();

            IHOTASQueue queue = new HOTASQueue(Substitute.For<IKeyboard>());
            var axisMap = new HOTASAxis() { Type = HOTASButton.ButtonType.AxisLinear, MapName = "forward", IsDirectional = true };
            axisMap.ButtonMap = new ObservableCollection<HOTASButton>() { new HOTASButton() { ActionName = "button 1", MapName = "forward" } };
            axisMap.ReverseButtonMap = new ObservableCollection<HOTASButton>() { new HOTASButton() { ActionName = "button 2", MapName = "reverse" } };

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
            Assert.True(axisMap.Direction == AxisDirection.Forward);
            axisMap.SetAxis(700);

            Assert.True(axisMap.Direction == AxisDirection.Backward);

            queue.SetModesCollection(new Dictionary<int, ObservableCollection<IHotasBaseMap>> { { 1, new ObservableCollection<IHotasBaseMap>() { axisMap } } });
            queue.ActivateMode(1);

            subDeviceList.KeystrokeDownSent += Raise.EventWith(queue, new KeystrokeSentEventArgs(0, 0, 0, false, false));

            Assert.Single(hotasVm.Activity);
            Assert.Equal("button 2", hotasVm.Activity.FirstOrDefault()?.ActionName);
        }

        [Fact]
        public void snap_to_button()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out _);
            hotasVm.Initialize();
            Assert.True(hotasVm.SnapToButton); //default is true
            Assert.PropertyChanged(hotasVm, "SnapToButton", () => hotasVm.SnapToButton = false);
        }

        [Fact]
        public void quick_profile_show_main_window()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out var subQuickProfilePanelVm);
            hotasVm.Initialize();            
            Assert.Raises<EventArgs>(a => hotasVm.ShowMainWindow += a, a => hotasVm.ShowMainWindow -= a, () => subQuickProfilePanelVm.ShowWindow());
        }

        [Fact]
        public void quick_profile_closed()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out var subDeviceList, out _, out _, out _, out var subQuickProfilePanelVm);
            hotasVm.Initialize();
            Assert.Raises<EventArgs>(a => hotasVm.Close += a, a => hotasVm.Close -= a, () => subQuickProfilePanelVm.CloseApp());
        }

        [Fact]
        public void quick_load_profile_via_publish()
        {
            var fileContents = GetJsonFile();
            var subFileIo = Substitute.For<IFileIO>();
            subFileIo.ReadAllText(Arg.Any<string>()).Returns(fileContents);

            var hotasVm = CreateHotasCollectionViewModel_WithEventAggregator(subFileIo, out var eventAggregator);
            hotasVm.Devices = new ObservableCollection<DeviceViewModel>();

            var deviceId = new Guid("3905f630-ed1c-11e9-8001-444553540000");

            var profileEvent = new QuickProfileSelectedEvent()
            {
                Id = 43,
                Name = "not null",
                Path = "not null"
            };

            Assert.Empty(hotasVm.Devices);
            eventAggregator.Publish(profileEvent);
            Assert.NotNull(hotasVm.Devices);
            Assert.Equal(deviceId, hotasVm.Devices[0].InstanceId);
        }

        [Fact]
        public void load_profile_no_file()
        {
            var productId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();

            var hotasVm = CreateHotasCollectionViewModel_WithDispatcher(out var subFileSystem, out var subQuickProfilePanelVm);

            subFileSystem.FileOpen(Arg.Any<string>()).ReturnsForAnyArgs(i => null);
            subQuickProfilePanelVm.GetAutoLoadPath().Returns("test path");

            Assert.Null(hotasVm.Devices);
            hotasVm.Initialize();
            Assert.Empty(hotasVm.Devices);

            Assert.Equal("Could not load test path!!! Is this a SierraHOTAS compatible JSON file?", hotasVm.ProfileSetFileName);
        }

        [Fact]
        public void load_profile_bad_guid()
        {
            var productId = Guid.NewGuid();
            var deviceId = Guid.Empty;

            var hotasVm = CreateHotasCollectionViewModel(out _, out IFileSystem subFileSystem, out QuickProfilePanelViewModel subQuickProfilePanelVm);

            var newHotasCollection = CreateHotasCollection();
            newHotasCollection.Devices.Add(new HOTASDevice() { DeviceId = deviceId });


            subFileSystem.FileOpen(Arg.Any<string>()).ReturnsForAnyArgs(newHotasCollection);
            subQuickProfilePanelVm.GetAutoLoadPath().Returns("test path");

            Assert.Null(hotasVm.Devices);
            hotasVm.Initialize();
            Assert.Empty(hotasVm.Devices);
            Assert.Equal("Invalid device found in profile. Check logs", hotasVm.ProfileSetFileName);
        }



        private string GetJsonFile()
        {
            var json = @"
                {
                  ""JsonFormatVersion"": ""1.0.0"",
                  ""Devices"": [
                    {
                      ""DeviceId"": ""3905f630-ed1c-11e9-8001-444553540000"",
                      ""ProductId"": ""204603eb-0000-0000-0000-504944564944"",
                      ""Name"": ""RIGHT VPC Stick MT-50CM"",
                      ""Modes"": {
                        ""1"": [
                          {
                            ""MapId"": 48,
                            ""MapName"": ""Button1"",
                            ""Type"": 3,
                            ""ActionId"": ""7cf8c1da-b58c-4ee7-acf0-22847bf36cc3""
                          },
                          {
                            ""MapId"": 49,
                            ""MapName"": ""Button2"",
                            ""Type"": 3,
                            ""ActionId"": ""2f65169a-b89d-4716-ab64-5b431a2e67e2""
                          }
                        ],
                        ""3"": [
                          {
                            ""MapId"": 57,
                            ""MapName"": ""Button10"",
                            ""Type"": 3,
                            ""ActionId"": ""7cf8c1da-b58c-4ee7-acf0-22847bf36cc3""
                          },
                          {
                            ""MapId"": 63,
                            ""MapName"": ""Button16"",
                            ""Type"": 3,
                            ""ActionId"": ""2f65169a-b89d-4716-ab64-5b431a2e67e2""
                          }
                        ]
                      }
                    },
                    {
                      ""DeviceId"": ""ffeaf2a0-9145-11ea-8001-444553540000"",
                      ""ProductId"": ""205503eb-0000-0000-0000-504944564944"",
                      ""Name"": ""VPC Throttle MT-50 CM2"",
                      ""Modes"": {
                        ""1"": [
                          {
                            ""MapId"": 48,
                            ""MapName"": ""Button1"",
                            ""Type"": 3,
                            ""ActionId"": ""7cf8c1da-b58c-4ee7-acf0-22847bf36cc3""
                          },
                          {
                            ""MapId"": 52,
                            ""MapName"": ""Button5"",
                            ""Type"": 3,
                            ""ActionId"": ""2f65169a-b89d-4716-ab64-5b431a2e67e2""
                          }
                        ],
                        ""3"": [
                          {
                            ""MapId"": 51,
                            ""MapName"": ""Button4"",
                            ""Type"": 3,
                            ""ActionId"": ""7cf8c1da-b58c-4ee7-acf0-22847bf36cc3""
                          },
                          {
                            ""MapId"": 53,
                            ""MapName"": ""Button6"",
                            ""Type"": 3,
                            ""ActionId"": ""2f65169a-b89d-4716-ab64-5b431a2e67e2""
                          }
                        ]
                      }
                    }
                  ],
                  ""ActionCatalog"": {
                    ""Catalog"": [
                      {
                        ""Id"": ""7cf8c1da-b58c-4ee7-acf0-22847bf36cc3"",
                        ""ActionName"": ""APU Pushbutton - Start/Stop"",
                        ""Actions"": [
                          {
                            ""ScanCode"": 42
                          },
                          {
                            ""ScanCode"": 30
                          },
                          {
                            ""ScanCode"": 30,
                            ""IsKeyUp"": true
                          },
                          {
                            ""ScanCode"": 42,
                            ""IsKeyUp"": true
                          }
                        ]
                      },
                      {
                        ""Id"": ""2f65169a-b89d-4716-ab64-5b431a2e67e2"",
                        ""ActionName"": ""APU Pushbutton Cover - Open/Close"",
                        ""Actions"": [
                          {
                            ""ScanCode"": 29
                          },
                          {
                            ""ScanCode"": 30
                          },
                          {
                            ""ScanCode"": 30,
                            ""IsKeyUp"": true
                          },
                          {
                            ""ScanCode"": 29,
                            ""IsKeyUp"": true
                          }
                        ]
                      }
                    ]
                  },
                  ""ModeActivationButtons"": {
                    ""1"": {
                      ""Mode"": 1,
                      ""ModeName"": ""Pilot"",
                      ""DeviceName"": ""VPC Throttle MT-50 CM2"",
                      ""DeviceId"": ""ffeaf2a0-9145-11ea-8001-444553540000"",
                      ""ButtonName"": ""Button56"",
                      ""ButtonId"": 103
                    },
                    ""2"": {
                      ""Mode"": 2,
                      ""InheritFromMode"": 1,
                      ""ModeName"": ""George"",
                      ""DeviceName"": ""VPC Throttle MT-50 CM2"",
                      ""DeviceId"": ""ffeaf2a0-9145-11ea-8001-444553540000"",
                      ""ButtonName"": ""Button57"",
                      ""ButtonId"": 104
                    }
                  }
                }
                ";

            return json;
        }


        [Fact]
        public void load_profile()
        {
            var fileContents = GetJsonFile();
            var subFileIo = Substitute.For<IFileIO>();
            subFileIo.ReadAllText(Arg.Any<string>()).Returns(fileContents);

            var hotasVm = CreateHotasCollectionViewModel_OnlySubFileIO(subFileIo);
            var subHotasCollection = CreateHotasCollectionSubstitute();

            Assert.Single(hotasVm.ActionCatalog.Catalog);
            Assert.Null(hotasVm.Devices);

            Assert.PropertyChanged(hotasVm, "ModeActivationItems", hotasVm.Initialize);

            Assert.NotNull(hotasVm.Devices);
            Assert.Equal(2, hotasVm.Devices.Count);
            Assert.Equal(3, hotasVm.ActionCatalog.Catalog.Count);

            subHotasCollection.Received().Stop();
        }

        //TODO - refactor to support action catalog persistence
        //[Fact]
        public void load_profile_add_button_list_to_catalog()
        {
            var productId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();

            var hotasVm = CreateHotasCollectionViewModel(out var subFileSystem, out var subQuickProfilePanelVm);
            Assert.Single(hotasVm.ActionCatalog.Catalog);
            Assert.Equal(ActionCatalogItem.NO_ACTION_TEXT, hotasVm.ActionCatalog.Catalog[0].ActionName);

            var subHotasCollection = CreateHotasCollectionSubstitute();

            var subDevice = new HOTASDevice(Substitute.For<IDirectInput>(), productId, deviceId, "test device",
                Substitute.For<IHOTASQueue>())
            {
                ButtonMap =
                {
                    new HOTASAxis() {MapName = "test1", MapId = 43, Type = HOTASButton.ButtonType.AxisLinear, ButtonMap = new ObservableCollection<HOTASButton>(){new HOTASButton(){MapName = "bm1", ActionName = "abm1", ActionCatalogItem = new ActionCatalogItem(){ActionName = "action1", Actions = new ObservableCollection<ButtonAction>(){new ButtonAction(){ScanCode = 1}}}}}},
                    new HOTASAxis() {MapName = "test2", MapId = 44, Type = HOTASButton.ButtonType.AxisLinear, ButtonMap = new ObservableCollection<HOTASButton>(){new HOTASButton(){MapName = "bm2", ActionName = "abm2", ActionCatalogItem = new ActionCatalogItem(){ActionName = "action2", Actions = new ObservableCollection<ButtonAction>(){new ButtonAction(){ScanCode = 2}}}}}},
                }
            };




            var profile = new Dictionary<int, ObservableCollection<IHotasBaseMap>>();
            var map = new ObservableCollection<IHotasBaseMap>()
            {
                new HOTASAxis() {MapName = "test1", MapId = 43, Type = HOTASButton.ButtonType.AxisLinear, ButtonMap = new ObservableCollection<HOTASButton>(){new HOTASButton(){MapName = "bm1", ActionName = "abm1", ActionCatalogItem = new ActionCatalogItem(){ActionName = "action1", Actions = new ObservableCollection<ButtonAction>(){new ButtonAction(){ScanCode = 1}}}}}},
                new HOTASAxis() {MapName = "test2", MapId = 44, Type = HOTASButton.ButtonType.AxisLinear, ButtonMap = new ObservableCollection<HOTASButton>(){new HOTASButton(){MapName = "bm2", ActionName = "abm2", ActionCatalogItem = new ActionCatalogItem(){ActionName = "action2", Actions = new ObservableCollection<ButtonAction>(){new ButtonAction(){ScanCode = 2}}}}}},
            };
            profile.Add(1, map);

            subDevice.SetMode(profile);
            
            subHotasCollection.Devices.Add(subDevice);

            subHotasCollection.ModeActivationButtons.Add(1, new ModeActivationItem() { ButtonId = 43, DeviceId = deviceId });
            subHotasCollection.SetupNewMode();


            subFileSystem.FileOpen(Arg.Any<string>()).ReturnsForAnyArgs(subHotasCollection);
            subFileSystem.LastSavedFileName.Returns("last saved file");
            subQuickProfilePanelVm.GetAutoLoadPath().Returns("test path");

            hotasVm.Initialize();

            Assert.Equal(3, hotasVm.ActionCatalog.Catalog.Count);
            Assert.Equal(ActionCatalogItem.NO_ACTION_TEXT, hotasVm.ActionCatalog.Catalog[0].ActionName);
            Assert.Equal("action1", hotasVm.ActionCatalog.Catalog[1].ActionName);
            Assert.Equal("action2", hotasVm.ActionCatalog.Catalog[2].ActionName);
        }


        [Fact]
        public void device_recording_stopped_no_map()
        {
            var fileContents = GetJsonFile();
            var subFileIo = Substitute.For<IFileIO>();
            subFileIo.ReadAllText(Arg.Any<string>()).Returns(fileContents);

            var hotasVm = CreateHotasCollectionViewModel_OnlySubFileIO(subFileIo);

            hotasVm.Initialize();
            var deviceVm = hotasVm.Devices.FirstOrDefault();

            Assert.NotNull(deviceVm);
            deviceVm.RecordingStopped += Raise.Event();
            Assert.Equal(3, hotasVm.ActionCatalog.Catalog.Count);
        }

        [Fact]
        public void device_recording_stopped_with_map()
        {
            var fileContents = GetJsonFile();
            var subFileIo = Substitute.For<IFileIO>();
            subFileIo.ReadAllText(Arg.Any<string>()).Returns(fileContents);

            var hotasVm = CreateHotasCollectionViewModel_OnlySubFileIO(subFileIo);

            hotasVm.Initialize();

            var deviceVm = hotasVm.Devices.FirstOrDefault();
            Assert.NotNull(deviceVm);
            deviceVm.RecordingStopped += Raise.EventWith(new ButtonMapViewModel() { ActionName = "test button map" }, new EventArgs());
            Assert.Equal(3, hotasVm.ActionCatalog.Catalog.Count);
        }

        [Fact]
        public void device_list_button_pressed()
        {
            var deviceGuid = Guid.NewGuid();
            var hotasVm = CreateHotasCollectionViewModel_WithEventAggregator(out _, out IHOTASCollection subDeviceList);
            var hotasDevice = subDeviceList.Devices[0];
            hotasDevice.DeviceId = deviceGuid;

            hotasVm.Initialize();

            Assert.Raises<ButtonPressedViewModelEventArgs>(e => hotasVm.ButtonPressed += e, e => hotasVm.ButtonPressed -= e, () =>
            {
                subDeviceList.ButtonPressed += Raise.EventWith(new object(), new ButtonPressedEventArgs() { Device = (HOTASDevice)hotasDevice });
            });
        }

        [Fact]
        public void device_list_axis_changed()
        {
            var deviceGuid = Guid.NewGuid();
            var hotasVm = CreateHotasCollectionViewModel_WithEventAggregator(out _, out IHOTASCollection subDeviceList);
            var hotasDevice = subDeviceList.Devices[0];
            hotasDevice.DeviceId = deviceGuid;

            hotasVm.Initialize();

            Assert.Raises<AxisChangedViewModelEventArgs>(e => hotasVm.AxisChanged += e, e => hotasVm.AxisChanged -= e, () =>
            {
                subDeviceList.AxisChanged += Raise.EventWith(new object(), new AxisChangedEventArgs() { AxisId = 1, Device = (HOTASDevice)hotasDevice, Value = 1 });
            });
        }
    }
}

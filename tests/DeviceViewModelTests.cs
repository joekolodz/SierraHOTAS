using NSubstitute;
using SharpDX.DirectInput;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SierraHOTAS.Win32;
using Xunit;
using JoystickOffset = SierraHOTAS.Models.JoystickOffset;

namespace SierraHOTAS.Tests
{
    public class DeviceViewModelTests
    {
        private class TestJoystick_LostConnection : IJoystick
        {
            public TestJoystick_LostConnection()
            {
                Capabilities = new Capabilities();
            }

            public int BufferSize { get; set; }
            public void Acquire()
            {

            }

            public Capabilities Capabilities { get; }

            public void GetCurrentState(ref JoystickState joystickState)
            {

            }

            public bool IsAxisPresent(string axisName)
            {
                return true;
            }

            public void Poll()
            {

            }

            public JoystickUpdate[] GetBufferedData()
            {
                throw new NotImplementedException("GetBufferedData called");
            }

            public void Unacquire()
            {

            }

            public void Dispose()
            {

            }
        }

        private class TestJoystick_AxisChanged : IJoystick
        {
            private JoystickUpdate[] data;

            public TestJoystick_AxisChanged()
            {
                Capabilities = new Capabilities() { AxeCount = 2 };
                data = new JoystickUpdate[1];
            }

            public int BufferSize { get; set; }
            public void Acquire()
            {

            }

            public Capabilities Capabilities { get; }
            public void GetCurrentState(ref JoystickState joystickState)
            {

            }

            public bool IsAxisPresent(string axisName)
            {
                return true;
            }

            public void Poll()
            {
                data[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Slider1, Sequence = 0, Timestamp = 0, Value = JitterDetection.Threshold * 100 };
            }

            public JoystickUpdate[] GetBufferedData()
            {
                return data;
            }

            public void Unacquire()
            {

            }

            public void Dispose()
            {

            }
        }

        private class TestDispatcher_AxisChanged : IDispatcher
        {
            public void Invoke(Action callback)
            {
                callback.Invoke();
            }
        }

        private static DeviceViewModel CreateDeviceViewMode(out IHOTASDevice hotasDevice)
        {
            const string deviceName = "test device";
            var subFileSystem = Substitute.For<IFileSystem>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subHotasQueueFactory = Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>());

            var hotasDeviceFactory = new HOTASDeviceFactory();

            var deviceId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            hotasDevice = hotasDeviceFactory.CreateHOTASDevice(subDirectInputFactory.CreateDirectInput(), productId, deviceId, deviceName, subHotasQueueFactory.CreateHOTASQueue());
            hotasDevice.Capabilities = new Capabilities() { AxeCount = 0, ButtonCount = 2 };

            var deviceVm = new DeviceViewModel(subDispatcherFactory.CreateDispatcher(), subFileSystem, subMediaPlayerFactory, hotasDevice);
            return deviceVm;
        }

        private static DeviceViewModel CreateDeviceViewMode(string deviceName, out IHOTASDevice hotasDevice)
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subHotasQueueFactory = Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>());

            var hotasDeviceFactory = new HOTASDeviceFactory();

            var deviceId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            hotasDevice = hotasDeviceFactory.CreateHOTASDevice(subDirectInputFactory.CreateDirectInput(), productId, deviceId, deviceName, subHotasQueueFactory.CreateHOTASQueue());
            hotasDevice.Capabilities = new Capabilities() { AxeCount = 0, ButtonCount = 2 };

            var deviceVm = new DeviceViewModel(subDispatcherFactory.CreateDispatcher(), subFileSystem, subMediaPlayerFactory, hotasDevice);
            return deviceVm;
        }

        private static DeviceViewModel CreateDeviceViewMode_LostConnection_old(out IHOTASQueue hotasQueue, out IHOTASDevice hotasDevice)
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subJoystickFactory = Substitute.For<JoystickFactory>();

            var hotasQueueFactory = new HOTASQueueFactory(Substitute.For<IKeyboard>());
            var hotasDeviceFactory = new HOTASDeviceFactory();

            var deviceId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var directInput = subDirectInputFactory.CreateDirectInput();
            var testJoystick = new TestJoystick_LostConnection();
            subJoystickFactory.CreateJoystick(Arg.Any<IDirectInput>(), Arg.Any<Guid>()).Returns(j => testJoystick);

            hotasQueue = hotasQueueFactory.CreateHOTASQueue();
            hotasDevice = hotasDeviceFactory.CreateHOTASDevice(directInput, subJoystickFactory, productId, deviceId, "test", hotasQueue);
            hotasDevice.Capabilities = new Capabilities() { AxeCount = 0, ButtonCount = 2 };

            var deviceVm = new DeviceViewModel(subDispatcherFactory.CreateDispatcher(), subFileSystem, subMediaPlayerFactory, hotasDevice);
            return deviceVm;
        }

        private static DeviceViewModel CreateDeviceViewMode_LostConnection(out IHOTASQueue hotasQueue, out IHOTASDevice subHotasDevice)
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subJoystickFactory = Substitute.For<JoystickFactory>();

            var hotasQueueFactory = new HOTASQueueFactory(Substitute.For<IKeyboard>());
            var testJoystick = new TestJoystick_LostConnection();
            subJoystickFactory.CreateJoystick(Arg.Any<IDirectInput>(), Arg.Any<Guid>()).Returns(j => testJoystick);

            hotasQueue = hotasQueueFactory.CreateHOTASQueue();
            subHotasDevice = Substitute.For<IHOTASDevice>();
            subHotasDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());
            subHotasDevice.Capabilities.Returns(new Capabilities() { AxeCount = 0, ButtonCount = 2 });

            var deviceVm = new DeviceViewModel(subDispatcherFactory.CreateDispatcher(), subFileSystem, subMediaPlayerFactory, subHotasDevice);
            return deviceVm;
        }

        private static DeviceViewModel CreateDeviceViewMode_AxesChanged(out IHOTASQueue hotasQueue, out IHOTASDevice subHotasDevice)
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subJoystickFactory = Substitute.For<JoystickFactory>();

            var hotasQueueFactory = new HOTASQueueFactory(Substitute.For<IKeyboard>());
            var testJoystick = new TestJoystick_AxisChanged();
            subJoystickFactory.CreateJoystick(Arg.Any<IDirectInput>(), Arg.Any<Guid>()).Returns(j => testJoystick);

            IDispatcher testDispatcher = new TestDispatcher_AxisChanged();
            subDispatcherFactory.CreateDispatcher().Returns(d => testDispatcher);

            hotasQueue = hotasQueueFactory.CreateHOTASQueue();
            subHotasDevice = Substitute.For<IHOTASDevice>();

            var axisMap = Substitute.For<HOTASAxis>();
            axisMap.MapId = (int)JoystickOffset.Slider1;
            axisMap.Type = HOTASButton.ButtonType.AxisLinear;
            subHotasDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>()
            {
                axisMap
            });

            var deviceVm = new DeviceViewModel(subDispatcherFactory.CreateDispatcher(), subFileSystem, subMediaPlayerFactory, subHotasDevice);
            return deviceVm;
        }

        private static DeviceViewModel CreateDeviceViewMode(out IHOTASQueue hotasQueue, out IHOTASDevice hotasDevice, out IJoystick subJoystick)
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subDispatchFactory = Substitute.For<DispatcherFactory>();
            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subJoystickFactory = Substitute.For<JoystickFactory>();

            var hotasQueueFactory = new HOTASQueueFactory(Substitute.For<IKeyboard>());
            var hotasDeviceFactory = new HOTASDeviceFactory();

            var deviceId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var directInput = subDirectInputFactory.CreateDirectInput();
            subJoystick = subJoystickFactory.CreateJoystick(directInput, deviceId);
            subJoystick.Capabilities.Returns(new Capabilities());

            hotasQueue = hotasQueueFactory.CreateHOTASQueue();
            hotasDevice = hotasDeviceFactory.CreateHOTASDevice(directInput, subJoystickFactory, productId, deviceId, "test", hotasQueue);
            hotasDevice.Capabilities = new Capabilities() { AxeCount = 0, ButtonCount = 2 };

            var deviceVm = new DeviceViewModel(subDispatchFactory.CreateDispatcher(), subFileSystem, subMediaPlayerFactory, hotasDevice);
            return deviceVm;
        }

        [Fact]
        public void basic_constructor()
        {
            var deviceVm = CreateDeviceViewMode("Test Device", out var subHotasDevice);
            Assert.NotNull(deviceVm);
            Assert.Equal("Test Device", deviceVm.Name);
            Assert.NotNull(deviceVm.ButtonMap);
            Assert.Empty(deviceVm.ButtonMap);

            Assert.Contains(deviceVm.PID, subHotasDevice.ProductId.ToString().ToUpper());
            Assert.Contains(deviceVm.VID, subHotasDevice.ProductId.ToString().ToUpper());
        }

        [Fact]
        public void clear_button_map()
        {
            var deviceVm = CreateDeviceViewMode("Test Device", out var hotasDevice);
            var list = new ObservableCollection<IHotasBaseMap>();
            list.Add(new HOTASButton() { MapId = 48, Type = HOTASButton.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() } }, ActionName = "first" });
            var testButton = new HOTASButton()
            {
                MapId = 49,
                Type = HOTASButton.ButtonType.Button,
                ActionCatalogItem = new ActionCatalogItem()
                { Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() } },
                ActionName = ActionCatalogItem.NO_ACTION_TEXT
            };
            list.Add(testButton);

            hotasDevice.ApplyButtonMap(list);

            deviceVm.RebuildMap();
            Assert.NotEmpty(testButton.ActionCatalogItem.Actions);
            deviceVm.ClearButtonMap();
            Assert.Empty(testButton.ActionCatalogItem.Actions);
        }

        [Fact]
        public void rebuild_map_no_param()
        {
            var deviceVm = CreateDeviceViewMode("Test Device", out var hotasDevice);
            var list = new ObservableCollection<IHotasBaseMap>();
            list.Add(new HOTASButton() { MapId = 48, Type = HOTASButton.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() } }, ActionName = "first" });
            list.Add(new HOTASButton() { MapId = 49, Type = HOTASButton.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() } }, ActionName = "second" });

            hotasDevice.ApplyButtonMap(list);
            deviceVm.ButtonMap.Add(new ButtonMapViewModel() { ButtonId = 1 });
            Assert.Single(deviceVm.ButtonMap);

            deviceVm.RebuildMap();
            Assert.NotEmpty(deviceVm.ButtonMap);
            Assert.Equal(2, deviceVm.ButtonMap.Count);
            Assert.Equal(hotasDevice.ButtonMap[0].MapId, deviceVm.ButtonMap[0].ButtonId);
            Assert.Equal(hotasDevice.ButtonMap[1].MapId, deviceVm.ButtonMap[1].ButtonId);
        }

        [Fact]
        public void rebuild_map_with_new_map()
        {
            var deviceVm = CreateDeviceViewMode("Test Device", out _);
            var list = new ObservableCollection<IHotasBaseMap>();
            list.Add(new HOTASButton() { MapId = 48, Type = HOTASButton.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() } }, ActionName = "first" });
            list.Add(new HOTASAxis() { MapId = 49, Type = HOTASButton.ButtonType.AxisLinear, ButtonMap = new ObservableCollection<HOTASButton>(), Segments = new ObservableCollection<Segment>() });
            list.Add(new HOTASAxis() { MapId = 50, Type = HOTASButton.ButtonType.AxisRadial, ButtonMap = new ObservableCollection<HOTASButton>(), Segments = new ObservableCollection<Segment>() });

            deviceVm.ButtonMap.Add(new ButtonMapViewModel() { ButtonId = 1 });
            Assert.Single(deviceVm.ButtonMap);

            deviceVm.RebuildMap(list);

            Assert.NotEmpty(deviceVm.ButtonMap);
            Assert.Equal(3, deviceVm.ButtonMap.Count);
            Assert.Equal(list[0].MapId, deviceVm.ButtonMap[0].ButtonId);
            Assert.Equal(list[1].MapId, deviceVm.ButtonMap[1].ButtonId);
            Assert.Equal(list[2].MapId, deviceVm.ButtonMap[2].ButtonId);
        }

        [Fact]
        public void force_button_press()
        {
            var deviceVm = CreateDeviceViewMode(out var hotasQueue, out var hotasDevice, out var subJoystick);


            var activationList = new Dictionary<int, ModeActivationItem>();
            var modes = new Dictionary<int, ObservableCollection<IHotasBaseMap>>();

            var map = new ObservableCollection<IHotasBaseMap>();
            map.Add(new HOTASButton() { MapId = 48, Type = HOTASButton.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() } }, ActionName = "first" });
            modes.Add(1, map);

            deviceVm.RebuildMap(map);

            hotasQueue.Listen(subJoystick, modes, activationList);

            Assert.Raises<ButtonPressedEventArgs>(
                a => hotasQueue.ButtonPressed += a,
                a => hotasQueue.ButtonPressed -= a,
                () => deviceVm.ForceButtonPress(JoystickOffset.Button1, true));
        }

        [Fact]
        public void lost_connection_to_device()
        {
            var deviceVm = CreateDeviceViewMode_LostConnection(out _, out var hotasDevice);
            Assert.PropertyChanged(deviceVm, "IsDeviceLoaded", () => hotasDevice.LostConnectionToDevice += Raise.EventWith(new object(), new LostConnectionToDeviceEventArgs(hotasDevice as HOTASDevice)));
        }

        [Fact]
        public void is_device_loaded()
        {
            var deviceVm = CreateDeviceViewMode(out var hotasDevice);
            Assert.True(deviceVm.IsDeviceLoaded);
            hotasDevice.Capabilities = null;
            Assert.False(deviceVm.IsDeviceLoaded);
        }

        [Fact]
        public void axis_changed()
        {
            var deviceVm = CreateDeviceViewMode_AxesChanged(out _, out var hotasDevice);
            var axis = deviceVm.ButtonMap.First(m => m.ButtonId == (int)JoystickOffset.Slider1) as AxisMapViewModel;
            Assert.NotNull(axis);
            Assert.Raises<AxisChangedViewModelEventArgs>(a => axis.AxisValueChanged += a,
                a => axis.AxisValueChanged -= a,
                () => hotasDevice.AxisChanged += Raise.EventWith(hotasDevice,
                    new AxisChangedEventArgs()
                        {AxisId = axis.ButtonId, Value = 1000, Device = hotasDevice as HOTASDevice}));
        }

        [Fact]
        public void replace_device()
        {
            var deviceVm = CreateDeviceViewMode(out var originalHotasDevice);

            var expectedHotasDevice = new HOTASDevice();
            expectedHotasDevice.Capabilities = new Capabilities();
            var expectedDeviceId = Guid.NewGuid();
            var expectedProductId = Guid.NewGuid();

            expectedHotasDevice.Name = "expected device name";
            expectedHotasDevice.DeviceId = expectedDeviceId;
            expectedHotasDevice.ProductId = expectedProductId;

            var list = new ObservableCollection<IHotasBaseMap>();
            list.Add(new HOTASButton() { MapId = 48, Type = HOTASButton.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() } }, ActionName = "first" });
            expectedHotasDevice.ApplyButtonMap(list);

            Assert.Equal(originalHotasDevice.Name, deviceVm.Name);
            Assert.Equal(originalHotasDevice.DeviceId, deviceVm.InstanceId);
            Assert.Equal(deviceVm.PID, DeviceViewModel.GetPID(originalHotasDevice.ProductId));
            Assert.Equal(deviceVm.VID, DeviceViewModel.GetVID(originalHotasDevice.ProductId));

            Assert.PropertyChanged(deviceVm, "IsDeviceLoaded", () => deviceVm.ReplaceDevice(expectedHotasDevice));

            Assert.NotEqual(originalHotasDevice.Name, deviceVm.Name);
            Assert.Equal(expectedHotasDevice.Name, deviceVm.Name);
            Assert.Equal(deviceVm.PID, DeviceViewModel.GetPID(expectedHotasDevice.ProductId));
            Assert.Equal(deviceVm.VID, DeviceViewModel.GetVID(expectedHotasDevice.ProductId));
            Assert.True(deviceVm.IsDeviceLoaded);
        }

        [Fact]
        public void recording_started_command()
        {
            var deviceVm = CreateDeviceViewMode(out var hotasDevice);

            var list = new ObservableCollection<IHotasBaseMap>();
            list.Add(new HOTASButton() { MapId = 48, Type = HOTASButton.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() } }, ActionName = "first" });
            list.Add(new HOTASButton() { MapId = 49, Type = HOTASButton.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() } }, ActionName = "second" });
            deviceVm.RebuildMap(list);

            var mapVm1 = deviceVm.ButtonMap[0] as ButtonMapViewModel;
            var mapVm2 = deviceVm.ButtonMap[1] as ButtonMapViewModel;

            Assert.NotNull(mapVm1);
            Assert.NotNull(mapVm2);

            Assert.False(mapVm1.IsDisabledForced);
            Assert.False(mapVm2.IsDisabledForced);
            
            mapVm1.RecordMacroStartCommand.Execute(default);

            Assert.False(mapVm1.IsDisabledForced);
            Assert.True(mapVm2.IsDisabledForced);
        }

        [Fact]
        public void recording_cancelled_command()
        {
            var deviceVm = CreateDeviceViewMode(out var hotasDevice);

            var list = new ObservableCollection<IHotasBaseMap>();
            list.Add(new HOTASButton() { MapId = 48, Type = HOTASButton.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() } }, ActionName = "first" });
            list.Add(new HOTASButton() { MapId = 49, Type = HOTASButton.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() } }, ActionName = "second" });
            deviceVm.RebuildMap(list);

            var mapVm1 = deviceVm.ButtonMap[0] as ButtonMapViewModel;
            var mapVm2 = deviceVm.ButtonMap[1] as ButtonMapViewModel;

            Assert.NotNull(mapVm1);
            Assert.NotNull(mapVm2);

            Assert.False(mapVm1.IsDisabledForced);
            Assert.False(mapVm2.IsDisabledForced);
            
            mapVm1.RecordMacroStartCommand.Execute(default);
            Assert.False(mapVm1.IsDisabledForced);
            Assert.True(mapVm2.IsDisabledForced);
            mapVm1.RecordMacroCancelCommand.Execute(default);

            Assert.False(mapVm1.IsDisabledForced);
            Assert.False(mapVm2.IsDisabledForced);
        }

        [Fact]
        public void recording_stopped_command()
        {
            var deviceVm = CreateDeviceViewMode(out var hotasDevice);

            var list = new ObservableCollection<IHotasBaseMap>();
            list.Add(new HOTASButton() { MapId = 48, Type = HOTASButton.ButtonType.Button, ActionCatalogItem = new ActionCatalogItem() { Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() } }, ActionName = "first" });
            deviceVm.RebuildMap(list);

            var map =  deviceVm.ButtonMap[0] as ButtonMapViewModel;
            Assert.NotNull(map);

            isRecordingStopped_recording_stopped = false;
            deviceVm.RecordingStopped += DeviceVm_RecordingStopped_for_recording_stopped;

            map.RecordMacroStopCommand.Execute(default);
            Assert.True(isRecordingStopped_recording_stopped);
            deviceVm.RecordingStopped -= DeviceVm_RecordingStopped_for_recording_stopped;
        }

        private bool isRecordingStopped_recording_stopped = false;
        private void DeviceVm_RecordingStopped_for_recording_stopped(object sender, EventArgs e)
        {
            isRecordingStopped_recording_stopped = true;
        }

        [Fact]
        public void get_name_with_ids()
        {
            var deviceVm = CreateDeviceViewMode(out var hotasDevice);

            var vid = DeviceViewModel.GetVID(hotasDevice.ProductId);
            var pid = DeviceViewModel.GetPID(hotasDevice.ProductId);
            var deviceName = hotasDevice.Name;

            Assert.Contains(vid, deviceVm.NameWithIds);
            Assert.Contains(pid, deviceVm.NameWithIds);
            Assert.Contains(deviceName, deviceVm.NameWithIds);
            Assert.Contains("VID:", deviceVm.NameWithIds);
            Assert.Contains("PID:", deviceVm.NameWithIds);
        }
    }
}

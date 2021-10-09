using NSubstitute;
using SharpDX.DirectInput;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SierraHOTAS.Win32;
using Xunit;
using JoystickOffset = SierraHOTAS.Models.JoystickOffset;

namespace SierraHOTAS.Tests
{
    public class HOTASCollectionTests
    {
        [Fact]
        public void constructor_null()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new HOTASCollection(null, Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), Substitute.For<HOTASDeviceFactory>()));
            Assert.Equal("Value cannot be null.\r\nParameter name: directInputFactory", exception.Message);

            exception = Assert.Throws<ArgumentNullException>(() => new HOTASCollection(Substitute.For<DirectInputFactory>(), null, Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), Substitute.For<HOTASDeviceFactory>()));
            Assert.Equal("Value cannot be null.\r\nParameter name: joystickFactory", exception.Message);

            exception = Assert.Throws<ArgumentNullException>(() => new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), null, Substitute.For<HOTASDeviceFactory>()));
            Assert.Equal("Value cannot be null.\r\nParameter name: hotasQueueFactory", exception.Message);

            exception = Assert.Throws<ArgumentNullException>(() => new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), null));
            Assert.Equal("Value cannot be null.\r\nParameter name: hotasDeviceFactory", exception.Message);
        }

        [Fact]
        public void constructor()
        {
            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), Substitute.For<HOTASDeviceFactory>());
            Assert.Empty(list.Devices);
            Assert.Empty(list.ModeProfileActivationButtons);
            Assert.Equal(HOTASCollection.FileFormatVersion, list.JsonFormatVersion);
        }

        [Fact]
        public void add_hotas_device()
        {
            var deviceId = Guid.NewGuid();
            var newDevice = new HOTASDevice() { DeviceId = deviceId };
            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(newDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            var device = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.Empty, deviceId, "test device", Substitute.For<IHOTASQueue>());
            device.ButtonMap.Add(new HOTASButton() { MapId = 1, MapName = "first button", ActionName = "tes action", IsShift = true, ShiftModePage = 2, Type = HOTASButton.ButtonType.Button });
            list.AddDevice(device);
            var addedDevice = list.Devices.First(d => d.DeviceId == deviceId);
            Assert.Equal("first button", addedDevice.ButtonMap[0].MapName);
        }

        [Fact]
        public void add_hotas_device_with_mode_profile()
        {
            var deviceId = Guid.NewGuid();
            var newDevice = new HOTASDevice() { DeviceId = deviceId };
            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(newDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            var device = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.Empty, deviceId, "test device", Substitute.For<IHOTASQueue>());
            device.ButtonMap.Add(new HOTASButton() { MapId = 1, MapName = "first button", ActionName = "tes action", IsShift = true, ShiftModePage = 2, Type = HOTASButton.ButtonType.Button });
            device.ModeProfiles.Add(43, new ObservableCollection<IHotasBaseMap>() { { new HOTASButton() { MapName = "mode profile map" } } });
            list.AddDevice(device);
            var addedDevice = list.Devices.First(d => d.DeviceId == deviceId);
            Assert.Equal("mode profile map", addedDevice.ModeProfiles[43][0].MapName);
        }

        [Fact]
        public void replace_device()
        {
            var deviceId = Guid.NewGuid();
            var firstDevice = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.Empty, deviceId, "existing device", Substitute.For<IHOTASQueue>());

            var replaceDevice = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.Empty, deviceId, "replace device", Substitute.For<IHOTASQueue>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(firstDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            firstDevice.ButtonMap.Add(new HOTASButton());
            list.AddDevice(firstDevice);
            
            var currentDevice = list.Devices.First(d => d.DeviceId == deviceId);
            Assert.Equal(firstDevice.Name, currentDevice.Name);

            list.ReplaceDevice(replaceDevice);

            currentDevice = list.Devices.First(d => d.DeviceId == deviceId);
            
            Assert.Equal(replaceDevice.Name, currentDevice.Name);
        }

        [Fact]
        public void start()
        {
            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subDirectInput = Substitute.For<IDirectInput>();

            var deviceId = Guid.NewGuid();
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.DeviceId.Returns(deviceId);

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<JoystickFactory>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var deviceInstances = new List<DeviceInstance>();
            var subInstance = Substitute.For<DeviceInstance>();
            deviceInstances.Add(subInstance);

            subDirectInputFactory.CreateDirectInput().Returns(subDirectInput);
            subDirectInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly).Returns(deviceInstances);

            var list = new HOTASCollection(subDirectInputFactory, Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.Start();

            var addedDevice = list.Devices[0];
            Assert.Equal(deviceId, addedDevice.DeviceId);
            addedDevice.Received(1).ListenAsync();
        }

        [Fact]
        public void stop_null()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.SetupNewModeProfile().Returns(43);
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.Stop();
            subDevice.Received().Stop();
        }

        [Fact]
        public void clear_button_map()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.SetupNewModeProfile().Returns(43);
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.ClearButtonMap();
            subDevice.Received().ClearButtonMap();
        }

        [Fact]
        public void listen_to_all_devices()
        {
            var subDevice1 = Substitute.For<IHOTASDevice>();
            subDevice1.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());
            subDevice1.DeviceId = Guid.NewGuid();

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();

            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice1);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice1);

            var subDevice2 = Substitute.For<IHOTASDevice>();
            subDevice2.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());
            subDevice2.DeviceId = Guid.NewGuid();

            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice2);
            list.AddDevice(subDevice2);

            Assert.Equal(2, list.Devices.Count);

            list.ListenToAllDevices();
            subDevice1.Received().ListenAsync();
            subDevice2.Received().ListenAsync();
        }

        [Fact]
        public void listen_to_device()
        {
            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), Substitute.For<HOTASDeviceFactory>());
            var subDevice = Substitute.For<IHOTASDevice>();
            list.ListenToDevice(subDevice);
            subDevice.Received().ListenAsync();
        }

        [Fact]
        public void device_lost_connection_to_device()
        {
            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), Substitute.For<HOTASDeviceFactory>());
            var subDevice = Substitute.For<IHOTASDevice>();
            list.ListenToDevice(subDevice);

            Assert.Raises<LostConnectionToDeviceEventArgs>(a => list.LostConnectionToDevice += a,
                a => list.LostConnectionToDevice -= a,
                () => subDevice.LostConnectionToDevice += Raise.EventWith(subDevice, new LostConnectionToDeviceEventArgs(subDevice)));
        }

        [Fact]
        public void setup_new_mode_profile()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.SetupNewModeProfile().Returns(43);
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            var newMode = list.SetupNewModeProfile();
            Assert.Equal(43, newMode);
            subDevice.Received().SetupNewModeProfile();
        }

        [Fact]
        public void copy_mode_profile_from_template()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.CopyModeProfileFromTemplate(4, 5);
            subDevice.Received().CopyModeProfileFromTemplate(4, 5);
        }

        [Fact]
        public void mode_profile_selected()
        {
            //this test does not test isShiftStateActive/PreviousMode logic in the method. the ShiftReleased test will do that
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.ListenToDevice(subDevice);

            Assert.Raises<ModeProfileChangedEventArgs>(a => list.ModeProfileChanged += a, a => list.ModeProfileChanged -= a,
                () => subDevice.ModeProfileSelected += Raise.EventWith(subDevice,
                    new ModeProfileSelectedEventArgs() { IsShift = true, Mode = 43 }));
        }

        [Fact]
        public void shift_released()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.ListenToDevice(subDevice);

            list.Mode = 1;
            subDevice.ModeProfileSelected += Raise.EventWith(subDevice, new ModeProfileSelectedEventArgs() { IsShift = true, Mode = 43 });

            //just proving Mode has changed since this test is verifying we get back to Mode = 1
            Assert.Equal(43, list.Mode);

            subDevice.ShiftReleased += Raise.EventWith(subDevice, new EventArgs());
            Assert.Equal(1, list.Mode);
        }

        [Fact]
        public void keystroke_down_sent()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.ListenToDevice(subDevice);

            Assert.Raises<KeystrokeSentEventArgs>(a => list.KeystrokeDownSent += a, a => list.KeystrokeDownSent -= a,
                () => subDevice.KeystrokeDownSent += Raise.EventWith(subDevice, new KeystrokeSentEventArgs(1, 1, 1, false, true)));
        }

        [Fact]
        public void keystroke_up_sent()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.ListenToDevice(subDevice);

            Assert.Raises<KeystrokeSentEventArgs>(a => list.KeystrokeUpSent += a, a => list.KeystrokeUpSent -= a,
                () => subDevice.KeystrokeUpSent += Raise.EventWith(subDevice, new KeystrokeSentEventArgs(1, 1, 1, false, true)));
        }

        [Fact]
        public void force_button_press()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.ListenToDevice(subDevice);
            list.ForceButtonPress(subDevice, JoystickOffset.Button1, true);
            subDevice.Received().ForceButtonPress(JoystickOffset.Button1, true);
        }

        [Fact]
        public void button_axis_changed()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.ListenToDevice(subDevice);

            Assert.Raises<AxisChangedEventArgs>(a => list.AxisChanged += a, a => list.AxisChanged -= a,
                () => subDevice.AxisChanged += Raise.EventWith(subDevice, new AxisChangedEventArgs() { AxisId = 1, Device = subDevice }));
        }

        [Fact]
        public void button_pressed()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.ListenToDevice(subDevice);

            Assert.Raises<ButtonPressedEventArgs>(a => list.ButtonPressed += a, a => list.ButtonPressed -= a,
                () => subDevice.ButtonPressed += Raise.EventWith(subDevice, new ButtonPressedEventArgs() { ButtonId = 1, Device = subDevice }));
        }

        [Fact]
        public void clear_unassigned_actions()
        {
            var subDevice1 = Substitute.For<IHOTASDevice>();
            var subDevice2 = Substitute.For<IHOTASDevice>();
            subDevice1.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice1);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice1);

            subDevice2.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice2);
            list.AddDevice(subDevice2);

            list.ListenToAllDevices();

            list.ClearUnassignedActions();
            subDevice1.Received().ClearUnassignedActions();
            subDevice2.Received().ClearUnassignedActions();
        }

        [Fact]
        public void get_hotas_devices_no_scanned_devices()
        {
            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subDirectInput = Substitute.For<IDirectInput>();

            subDirectInputFactory.CreateDirectInput().Returns(subDirectInput);
            subDirectInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly).Returns(x => null);

            var subHotasDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            var subHotasQueueFactory = Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>());
            var list = new HOTASCollection(subDirectInputFactory, Substitute.For<JoystickFactory>(), subHotasQueueFactory, subHotasDeviceFactory);
            var devices = list.RefreshMissingDevices();

            Assert.NotNull(devices);
            Assert.Empty(devices);
            subHotasDeviceFactory.DidNotReceive().CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>());
            subHotasQueueFactory.DidNotReceive().CreateHOTASQueue();
        }

        [Fact]
        public void get_hotas_devices_no_existing_device_capabilities_not_null()
        {
            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subDirectInput = Substitute.For<IDirectInput>();

            var deviceId = Guid.NewGuid();
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.DeviceId.Returns(deviceId);
            subDevice.Capabilities.Returns(new Capabilities());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<JoystickFactory>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var deviceInstances = new List<DeviceInstance>();
            var instance = new DeviceInstance { InstanceGuid = deviceId };
            deviceInstances.Add(instance);

            subDirectInputFactory.CreateDirectInput().Returns(subDirectInput);
            subDirectInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly).Returns(deviceInstances);

            var list = new HOTASCollection(subDirectInputFactory, Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.Start();

            var addedDevice = list.Devices[0];

            var deviceList = list.RefreshMissingDevices();
            Assert.Empty(deviceList);
        }

        [Fact]
        public void get_hotas_devices_no_existing_device_capabilities_null()
        {
            var subDirectInputFactory = Substitute.For<DirectInputFactory>();
            var subDirectInput = Substitute.For<IDirectInput>();

            var deviceId = Guid.NewGuid();
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.DeviceId.Returns(deviceId);
            subDevice.Capabilities.Returns(x => null);

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<JoystickFactory>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var deviceInstances = new List<DeviceInstance>();
            var instance = new DeviceInstance { InstanceGuid = deviceId };
            deviceInstances.Add(instance);

            subDirectInputFactory.CreateDirectInput().Returns(subDirectInput);
            subDirectInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly).Returns(deviceInstances);

            var subHotasDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            var subHotasQueueFactory = Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>());
            var list = new HOTASCollection(subDirectInputFactory, Substitute.For<JoystickFactory>(), subHotasQueueFactory, subHotasDeviceFactory);
            list.Start();

            var existingDevice = list.Devices[0];
            var deviceList = list.RefreshMissingDevices();
            Assert.Same(deviceList[0], existingDevice);
            subHotasDeviceFactory.Received().CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<JoystickFactory>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>());
            subHotasQueueFactory.Received().CreateHOTASQueue();
        }

        [Fact]
        public void set_mode_same_mode()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.Mode = 1;
            list.AddDevice(subDevice);
            list.ListenToAllDevices();

            list.SetMode(43);

            subDevice.DidNotReceive().SetMode(1);
        }

        [Fact]
        public void set_mode()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.Mode = 1;
            list.AddDevice(subDevice);
            list.ListenToAllDevices();


            Assert.Raises<ModeProfileChangedEventArgs>(a => list.ModeProfileChanged += a, a => list.ModeProfileChanged -= a,
                () => list.SetMode(43));
            Assert.Equal(43, list.Mode);
            subDevice.Received().SetMode(43);
        }

        [Fact]
        public void auto_set_mode()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            var buttonMap = new ObservableCollection<IHotasBaseMap>();
            buttonMap.Add(new HOTASButton() { ShiftModePage = 43 });
            subDevice.IsDeviceLoaded.Returns(true);
            subDevice.ButtonMap.Returns(buttonMap);
            subDevice.GetButtonState(Arg.Any<int>()).Returns(true);

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.Mode = 1;
            list.AddDevice(subDevice);
            list.ListenToAllDevices();

            list.AutoSetMode();
            Assert.Equal(43, list.Mode);
        }

        [Fact]
        public void remove_mode_profile_not_exist()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            subDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.ListenToAllDevices();

            var item = new ModeActivationItem();
            list.ModeProfileActivationButtons.Add(1, item);
            var isRemoved = list.RemoveModeProfile(new ModeActivationItem());

            Assert.Same(item, list.ModeProfileActivationButtons[1]);
            Assert.False(isRemoved);
        }

        [Fact]
        public void remove_mode_profile_only_one_profile()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            var buttonMap = new ObservableCollection<IHotasBaseMap>();
            var map = new HOTASButton() { MapId = 1, IsShift = true, ShiftModePage = 1 };
            buttonMap.Add(map);
            subDevice.ButtonMap.Returns(buttonMap);

            var deviceId = Guid.NewGuid();
            subDevice.DeviceId = deviceId;

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(),
                Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var profile = new Dictionary<int, ObservableCollection<IHotasBaseMap>>()
            {
                {
                    1, new ObservableCollection<IHotasBaseMap> {buttonMap[0]}
                }
            };

            subDevice.ModeProfiles.Returns(profile);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.ListenToAllDevices();

            var item = new ModeActivationItem()
            {
                DeviceId = deviceId,
                ButtonId = 1,
                Mode = 1
            };
            list.ModeProfileActivationButtons.Add(1, item);

            Assert.True(map.IsShift);
            Assert.Equal(1, map.ShiftModePage);

            var isRemoved = list.RemoveModeProfile(item);

            Assert.Empty(list.ModeProfileActivationButtons);
            Assert.True(isRemoved);
            Assert.False(map.IsShift);
            Assert.Equal(0, map.ShiftModePage);
            Assert.Equal(1, list.Mode);
        }

        [Fact]
        public void remove_mode_profile_more_than_one_profile()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            var buttonMap = new ObservableCollection<IHotasBaseMap>();
            var map1 = new HOTASButton() { MapId = 1, IsShift = true, ShiftModePage = 1 };
            var map2 = new HOTASButton() { MapId = 2, IsShift = false, ShiftModePage = 2 };
            buttonMap.Add(map1);
            buttonMap.Add(map2);
            subDevice.ButtonMap.Returns(buttonMap);

            var deviceId = Guid.NewGuid();
            subDevice.DeviceId = deviceId;

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(),
                Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var profile = new Dictionary<int, ObservableCollection<IHotasBaseMap>>()
            {
                {
                    1, new ObservableCollection<IHotasBaseMap> {buttonMap[0]}
                },
                {
                    2, new ObservableCollection<IHotasBaseMap> {buttonMap[1]}
                }
            };

            subDevice.ModeProfiles.Returns(profile);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.ListenToAllDevices();

            var item1 = new ModeActivationItem()
            {
                DeviceId = deviceId,
                ButtonId = 1,
                Mode = 1
            };

            var item2 = new ModeActivationItem()
            {
                DeviceId = deviceId,
                ButtonId = 2,
                Mode = 2
            };

            list.ModeProfileActivationButtons.Add(1, item1);
            list.ModeProfileActivationButtons.Add(2, item2);

            var isRemoved = list.RemoveModeProfile(item1);

            Assert.Single(list.ModeProfileActivationButtons);
            Assert.Same(item2, list.ModeProfileActivationButtons[2]);
            Assert.True(isRemoved);
            Assert.False(map1.IsShift);
            Assert.Equal(0, map1.ShiftModePage);
            Assert.Equal(2, list.Mode);
        }

        //todo in progress
        [Fact]
        public void apply_activation_button_to_all_profiles()
        {
            var subDevice = Substitute.For<IHOTASDevice>();
            var buttonMapProfile1 = new ObservableCollection<IHotasBaseMap>();
            var button_1_1 = new HOTASButton() { MapId = 1, IsShift = false, ShiftModePage = 1 };
            var button_1_2 = new HOTASButton() { MapId = 2, IsShift = false, ShiftModePage = 2 };
            var button_1_3 = new HOTASAxis() { MapId = 3, ButtonMap = new ObservableCollection<HOTASButton>() { new HOTASButton() { MapId = 3, ShiftModePage = 3 } } };

            buttonMapProfile1.Add(button_1_1);
            buttonMapProfile1.Add(button_1_2);
            buttonMapProfile1.Add(button_1_3);

            var buttonMapProfile2 = new ObservableCollection<IHotasBaseMap>();
            var button_2_1 = new HOTASButton() { MapId = 1, IsShift = false, ShiftModePage = 0 }; //simulate a new profile that does not have links back to previous profiles
            var button_2_2 = new HOTASButton() { MapId = 2, IsShift = false, ShiftModePage = 0 }; //simulate a new profile that does not have links back to previous profiles
            var button_2_3 = new HOTASAxis() { MapId = 3, ButtonMap = new ObservableCollection<HOTASButton>() { new HOTASButton() { MapId = 3, ShiftModePage = 0 } } };

            buttonMapProfile2.Add(button_2_1);
            buttonMapProfile2.Add(button_2_2);
            buttonMapProfile2.Add(button_2_3);

            subDevice.ButtonMap.Returns(buttonMapProfile1);


            var deviceId = Guid.NewGuid();
            subDevice.DeviceId = deviceId;

            var subDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subDeviceFactory.CreateHOTASDevice(Arg.Any<IDirectInput>(), Arg.Any<Guid>(), Arg.Any<Guid>(),
                Arg.Any<string>(), Arg.Any<IHOTASQueue>()).Returns(subDevice);

            var profiles = new Dictionary<int, ObservableCollection<IHotasBaseMap>>()
            {
                {
                    1, buttonMapProfile1
                },
                {
                    2, buttonMapProfile2
                },
                {
                    3, buttonMapProfile2
                }
            };

            subDevice.ModeProfiles.Returns(profiles);

            var list = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), subDeviceFactory);
            list.AddDevice(subDevice);
            list.ListenToAllDevices();

            var item1 = new ModeActivationItem()
            {
                DeviceId = deviceId,
                ButtonId = 1,
                Mode = 1
            };

            var item2 = new ModeActivationItem()
            {
                DeviceId = deviceId,
                ButtonId = 2,
                Mode = 2
            };

            var item3 = new ModeActivationItem()
            {
                DeviceId = deviceId,
                ButtonId = 3,
                Mode = 3
            };

            list.ModeProfileActivationButtons.Add(1, item1);
            list.ModeProfileActivationButtons.Add(2, item2);
            list.ModeProfileActivationButtons.Add(3, item3);

            Assert.Equal(0, button_2_1.ShiftModePage);
            Assert.Equal(0, button_2_2.ShiftModePage);
            Assert.Equal(0, button_2_3.ButtonMap[0].ShiftModePage);

            list.ApplyActivationButtonToAllProfiles();

            Assert.Equal(1, button_2_1.ShiftModePage);
            Assert.Equal(2, button_2_2.ShiftModePage);
            Assert.Equal(3, button_2_3.ButtonMap[0].ShiftModePage);
        }
    }
}

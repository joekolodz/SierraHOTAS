using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NSubstitute;
using SharpDX.DirectInput;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using Xunit;
using JoystickOffset = SierraHOTAS.Models.JoystickOffset;

namespace SierraHOTAS.Tests
{
    public class HOTASDeviceTests
    {
        private class TestJoystick_GetButtonState : IJoystick
        {
            public TestJoystick_GetButtonState()
            {
                Capabilities = new Capabilities() { AxeCount = 2, ButtonCount = 1 };
            }

            public int BufferSize { get; set; }
            public void Acquire()
            {

            }

            public Capabilities Capabilities { get; }

            public void GetCurrentState(ref JoystickState joystickState)
            {
                joystickState.Buttons[0] = true;
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

        private static HOTASDevice CreateHotasDevice()
        {
            return CreateHotasDevice(out _, out _, out _);
        }

        private static HOTASDevice CreateHotasDevice_GetButtonState()
        {
            var directInput = Substitute.For<IDirectInput>();
            var hotasQueue = Substitute.For<IHOTASQueue>();
            var productId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            const string name = "test device name";

            var subJoystick = new TestJoystick_GetButtonState();

            var joystickFactory = Substitute.For<JoystickFactory>();
            joystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);

            return new HOTASDevice(directInput, joystickFactory, productId, deviceId, name, hotasQueue);
        }

        private static HOTASDevice CreateHotasDevice(out IDirectInput directInput, out JoystickFactory joystickFactory, out IHOTASQueue hotasQueue)
        {
            directInput = Substitute.For<IDirectInput>();
            hotasQueue = Substitute.For<IHOTASQueue>();
            var productId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            const string name = "test device name";

            var subJoystick = Substitute.For<IJoystick>();
            subJoystick.Capabilities.Returns(new Capabilities() { AxeCount = 2, ButtonCount = 1 });
            subJoystick.IsAxisPresent(Arg.Any<string>()).Returns(true);

            joystickFactory = Substitute.For<JoystickFactory>();
            joystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);

            return new HOTASDevice(directInput, joystickFactory, productId, deviceId, name, hotasQueue);
        }

        [Fact]
        public void basic_constructor_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new HOTASDevice(null, Guid.NewGuid(), Guid.NewGuid(), "not empty", Substitute.For<IHOTASQueue>()));
            Assert.Throws<ArgumentNullException>(() => new HOTASDevice(Substitute.For<IDirectInput>(), Guid.NewGuid(),
                Guid.NewGuid(), string.Empty, Substitute.For<IHOTASQueue>()));
            Assert.Throws<ArgumentNullException>(() =>
                new HOTASDevice(Substitute.For<IDirectInput>(), Guid.NewGuid(), Guid.NewGuid(), "not empty", null));
            //empty device id is valid
            var device = new HOTASDevice(Substitute.For<IDirectInput>(), Guid.NewGuid(), Guid.Empty, "not empty",
                Substitute.For<IHOTASQueue>());
            Assert.Null(device.Name);
        }

        [Fact]
        public void basic_constructor()
        {
            var productId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            var device = new HOTASDevice(Substitute.For<IDirectInput>(), productId, deviceId, "test 1",
                Substitute.For<IHOTASQueue>());
            Assert.Equal("test 1", device.Name);
            Assert.Equal(productId, device.ProductId);
            Assert.Equal(deviceId, device.DeviceId);
            Assert.NotNull(device.ModeProfiles);
        }

        [Fact]
        public void initialize_device_default_constructor()
        {
            var test = new HOTASDevice();
            Assert.NotNull(test.ModeProfiles);
            Assert.NotNull(test.ButtonMap);
        }

        [Fact]
        public void initialize_device_partial_constructor()
        {
            var di = Substitute.For<IDirectInput>();
            var queue = Substitute.For<IHOTASQueue>();
            var productId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            const string name = "test device name";

            var device = new HOTASDevice(di, productId, deviceId, name, queue);

            Assert.Equal(deviceId, device.DeviceId);
            Assert.Equal(name, device.Name);
            Assert.Single(device.ModeProfiles);
            Assert.NotNull(device.ModeProfiles[1]);
            Assert.Same(device.ModeProfiles[1], device.ButtonMap);
        }

        [Fact]
        public void initialize_device_partial_constructor_null_parameters()
        {
            var di = Substitute.For<IDirectInput>();
            var queue = Substitute.For<IHOTASQueue>();
            var productId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            const string name = "test";

            Assert.NotNull(new HOTASDevice(di, productId, Guid.Empty, name, queue));
            Assert.Throws<ArgumentNullException>(() => new HOTASDevice(di, productId, deviceId, string.Empty, queue));
            Assert.Throws<ArgumentNullException>(() => new HOTASDevice(di, productId, deviceId, null, queue));
            Assert.Throws<ArgumentNullException>(() => new HOTASDevice(null, productId, deviceId, name, queue));
            Assert.Throws<ArgumentNullException>(() => new HOTASDevice(di, productId, deviceId, name, null));
        }

        [Fact]
        public void initialize_device_partial_constructor_empty_guid_ok()
        {
            var di = Substitute.For<IDirectInput>();
            var queue = Substitute.For<IHOTASQueue>();
            const string name = "test";

            var obj = new HOTASDevice(di, Guid.NewGuid(), Guid.Empty, name, queue);
            Assert.NotNull(obj);
            Assert.Equal(obj.DeviceId, Guid.Empty);
        }

        [Fact]
        public void initialize_device_full_constructor()
        {
            var di = Substitute.For<IDirectInput>();
            var queue = Substitute.For<IHOTASQueue>();
            var productId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            const string name = "test device name";

            var subJoystick = Substitute.For<IJoystick>();
            subJoystick.Capabilities.Returns(new Capabilities());
            var subJoystickFactory = Substitute.For<JoystickFactory>();
            subJoystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);

            var device = new HOTASDevice(di, subJoystickFactory, productId, deviceId, name, queue);


            Assert.Equal(deviceId, device.DeviceId);
            Assert.Equal(name, device.Name);
            Assert.Single(device.ModeProfiles);
            Assert.NotNull(device.ModeProfiles[1]);
            Assert.Same(device.ModeProfiles[1], device.ButtonMap);
            Assert.Equal(4096, subJoystick.BufferSize);
        }

        [Fact]
        public void set_mode_profile_empty()
        {
            var di = Substitute.For<IDirectInput>();
            var queue = Substitute.For<IHOTASQueue>();
            var productId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            const string name = "test device name";

            var subJoystick = Substitute.For<IJoystick>();
            subJoystick.Capabilities.Returns(new Capabilities());
            var subJoystickFactory = Substitute.For<JoystickFactory>();
            subJoystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);

            var device = new HOTASDevice(di, subJoystickFactory, productId, deviceId, name, queue);

            var newProfile = new Dictionary<int, ObservableCollection<IHotasBaseMap>>();
            var previousButtonMap = device.ButtonMap;

            Assert.Single(device.ModeProfiles);
            Assert.Empty(device.ModeProfiles[1]);

            device.SetModeProfile(newProfile);

            Assert.Empty(device.ModeProfiles);
            Assert.Same(previousButtonMap, device.ButtonMap);
        }

        [Fact]
        public void set_mode_profile()
        {
            var device = CreateHotasDevice();

            var newProfile = new Dictionary<int, ObservableCollection<IHotasBaseMap>>()
            {
                {
                    1,
                    new ObservableCollection<IHotasBaseMap>() {new HOTASButton() {MapName = "test map 1,1", MapId = 48}}
                },
                {
                    2,
                    new ObservableCollection<IHotasBaseMap>()
                    {
                        new HOTASButton() {MapName = "test map 2,1", MapId = 0},
                        new HOTASButton() {MapName = "test map 2,2", MapId = 4}
                    }
                }
            };


            Assert.Single(device.ModeProfiles);
            Assert.Equal(3, device.ModeProfiles[1].Count);
            Assert.Equal("Y", device.ModeProfiles[1][1].MapName);

            device.SetModeProfile(newProfile);

            Assert.Equal(2, device.ModeProfiles.Count);
            Assert.Single(device.ModeProfiles[1]);
            Assert.Equal(2, device.ModeProfiles[2].Count);
            Assert.Equal("test map 2,2", device.ModeProfiles[2][1].MapName);

            //assert that SetButtonMap is called
            Assert.NotNull(device.ModeProfiles);
            Assert.Equal(3, device.ButtonMap.Count);
            Assert.Equal("test map 1,1", device.ButtonMap[2].MapName);
        }

        [Fact]
        public void copy_mode_profile_from_template()
        {
            var device = CreateHotasDevice();

            var newProfile = new Dictionary<int, ObservableCollection<IHotasBaseMap>>()
            {
                {1, new ObservableCollection<IHotasBaseMap>() {new HOTASButton() {MapName = "test map 1,1", MapId = 0}, new HOTASButton() {MapName = "test map 1,2", MapId = 4}}},
                {2, new ObservableCollection<IHotasBaseMap>() {new HOTASButton() {MapName = "test map 2,1", MapId = 0} }},
            };

            device.SetModeProfile(newProfile);

            Assert.Single(device.ModeProfiles[2]);
            Assert.Equal("test map 2,1", device.ModeProfiles[2][0].MapName);
            device.CopyModeProfileFromTemplate(1, 2);

            Assert.Equal(2, device.ModeProfiles[2].Count);
            Assert.Equal("test map 1,1", device.ModeProfiles[2][0].MapName);
            Assert.Equal("test map 1,2", device.ModeProfiles[2][1].MapName);
        }

        [Fact]
        public void copy_mode_profile_from_template_no_destination_map()
        {
            var device = CreateHotasDevice();

            var newProfile = new Dictionary<int, ObservableCollection<IHotasBaseMap>>()
            {
                {1, new ObservableCollection<IHotasBaseMap>() {new HOTASButton() {MapName = "test map 1,1", MapId = 0}, new HOTASButton() {MapName = "test map 1,2", MapId = 4}}},
            };

            device.SetModeProfile(newProfile);

            Assert.Single(device.ModeProfiles);

            device.CopyModeProfileFromTemplate(1, 2);

            Assert.Equal(2, device.ModeProfiles[2].Count);
            Assert.Equal("test map 1,1", device.ModeProfiles[2][0].MapName);
            Assert.Equal("test map 1,2", device.ModeProfiles[2][1].MapName);
        }

        [Fact]
        public void copy_button_map_profile()
        {
            var sourceProfile = new ObservableCollection<IHotasBaseMap>()
            {
                new HOTASButton() {MapName = "button map 1,1", MapId = 0},
                new HOTASAxis()
                {
                    MapName = "axis map 1,2", MapId = 4,
                    ButtonMap = new ObservableCollection<HOTASButton>() {new HOTASButton() {MapName = "forward"}},
                    ReverseButtonMap = new ObservableCollection<HOTASButton>() {new HOTASButton() {MapName = "reverse"}}
                }
            };

            var destinationProfile = new ObservableCollection<IHotasBaseMap>();

            HOTASDevice.CopyButtonMapProfile(sourceProfile, destinationProfile);

            Assert.Equal(2, destinationProfile.Count);
            Assert.Equal("button map 1,1", destinationProfile[0].MapName);
            Assert.Equal("axis map 1,2", destinationProfile[1].MapName);
            Assert.Equal("forward", ((HOTASAxis)destinationProfile[1]).ButtonMap[0].MapName);
            Assert.Equal("reverse", ((HOTASAxis)destinationProfile[1]).ReverseButtonMap[0].MapName);
        }

        [Fact]
        public void list_async()
        {
            var device = CreateHotasDevice(out _, out _, out var hotasQueue);

            device.ListenAsync();

            Assert.Raises<KeystrokeSentEventArgs>(
                a => device.KeystrokeDownSent += a,
                a => device.KeystrokeDownSent -= a,
                () => hotasQueue.KeystrokeDownSent += Raise.EventWith(hotasQueue, new KeystrokeSentEventArgs(0, 0, 0, false, false)));

            Assert.Raises<KeystrokeSentEventArgs>(
                a => device.KeystrokeUpSent += a,
                a => device.KeystrokeUpSent -= a,
                () => hotasQueue.KeystrokeUpSent += Raise.EventWith(hotasQueue, new KeystrokeSentEventArgs(0, 0, 0, false, false)));

            Assert.Raises<MacroStartedEventArgs>(
                a => device.MacroStarted += a,
                a => device.MacroStarted -= a,
                () => hotasQueue.MacroStarted += Raise.EventWith(hotasQueue, new MacroStartedEventArgs(0, 0)));

            Assert.Raises<MacroCancelledEventArgs>(
                a => device.MacroCancelled += a,
                a => device.MacroCancelled -= a,
                () => hotasQueue.MacroCancelled += Raise.EventWith(hotasQueue, new MacroCancelledEventArgs(0, 0)));

            Assert.Raises<ButtonPressedEventArgs>(
                a => device.ButtonPressed += a,
                a => device.ButtonPressed -= a,
                () => hotasQueue.ButtonPressed += Raise.EventWith(hotasQueue, new ButtonPressedEventArgs()));

            Assert.Raises<AxisChangedEventArgs>(
                a => device.AxisChanged += a,
                a => device.AxisChanged -= a,
                () => hotasQueue.AxisChanged += Raise.EventWith(hotasQueue, new AxisChangedEventArgs()));

            Assert.Raises<ModeProfileSelectedEventArgs>(
                a => device.ModeProfileSelected += a,
                a => device.ModeProfileSelected -= a,
                () => hotasQueue.ModeProfileSelected += Raise.EventWith(hotasQueue, new ModeProfileSelectedEventArgs()));

            Assert.Raises<EventArgs>(
                a => device.ShiftReleased += a,
                a => device.ShiftReleased -= a,
                () => hotasQueue.ShiftReleased += Raise.EventWith(hotasQueue, new EventArgs()));

            Assert.Raises<LostConnectionToDeviceEventArgs>(
                a => device.LostConnectionToDevice += a,
                a => device.LostConnectionToDevice -= a,
                () => hotasQueue.LostConnectionToDevice += Raise.EventWith(hotasQueue, new EventArgs()));
        }

        [Fact]
        public void set_button_map_device_not_loaded()
        {
            var device = CreateHotasDevice();
            device.Capabilities = null;

            var newMap = new ObservableCollection<IHotasBaseMap>();

            Assert.Equal(3, device.ButtonMap.Count);
            device.SetButtonMap(newMap);
            Assert.Empty(device.ButtonMap);
        }

        [Fact]
        public void set_button_map()
        {
            var device = CreateHotasDevice();

            var newMap = new ObservableCollection<IHotasBaseMap>()
            {
                new HOTASAxis(){MapName = "X", MapId = 0, IsDirectional = false},
                new HOTASButton(){MapName = "Button1", MapId = 48, ActionName = "existing action"},
            };

            Assert.Equal(3, device.ButtonMap.Count);
            Assert.True(((HOTASAxis)device.ButtonMap[0]).IsDirectional);
            Assert.Empty(((HOTASButton)device.ButtonMap[2]).ActionName);
            device.SetButtonMap(newMap);
            Assert.Equal(3, device.ButtonMap.Count);
            Assert.False(((HOTASAxis)device.ButtonMap[0]).IsDirectional);
            Assert.Equal("existing action", ((HOTASButton)device.ButtonMap[2]).ActionName);
        }

        [Fact]
        public void test_button_seeds()
        {
            const string name = "test device name";

            var subJoystick = Substitute.For<IJoystick>();
            subJoystick.Capabilities.Returns(new Capabilities() { PovCount = 1, AxeCount = 8, ButtonCount = 1 });
            subJoystick.IsAxisPresent(Arg.Any<string>()).Returns(true);

            var joystickFactory = Substitute.For<JoystickFactory>();
            joystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);

            var device = new HOTASDevice(Substitute.For<IDirectInput>(), joystickFactory, Guid.NewGuid(), Guid.NewGuid(), name, Substitute.For<IHOTASQueue>());

            Assert.Equal(17, device.ButtonMap.Count);



            Assert.Equal(1, device.ButtonMap.Count(b => b.Type == HOTASButton.ButtonType.Button));
            Assert.Equal(5, device.ButtonMap.Count(b => b.Type == HOTASButton.ButtonType.AxisLinear));
            Assert.Equal(3, device.ButtonMap.Count(b => b.Type == HOTASButton.ButtonType.AxisRadial));
            Assert.Equal(8, device.ButtonMap.Count(b => b.Type == HOTASButton.ButtonType.POV));

            Assert.Equal(32, device.ButtonMap[9].MapId);
            Assert.Equal(HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 4500 * 1), device.ButtonMap[10].MapId);
            Assert.Equal(HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 4500 * 2), device.ButtonMap[11].MapId);
            Assert.Equal(HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 4500 * 3), device.ButtonMap[12].MapId);
            Assert.Equal(HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 4500 * 4), device.ButtonMap[13].MapId);
            Assert.Equal(HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 4500 * 5), device.ButtonMap[14].MapId);
            Assert.Equal(HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 4500 * 6), device.ButtonMap[15].MapId);
            Assert.Equal(HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, 4500 * 7), device.ButtonMap[16].MapId);

        }

        [Fact]
        public void set_mode()
        {
            var device = CreateHotasDevice();

            var newProfile = new Dictionary<int, ObservableCollection<IHotasBaseMap>>()
            {
                {
                    1,
                    new ObservableCollection<IHotasBaseMap>() {new HOTASButton() {MapName = "test map 1,1", MapId = 48}}
                },
                {
                    2,
                    new ObservableCollection<IHotasBaseMap>()
                    {
                        new HOTASButton() {MapName = "test map 2,1", MapId = 0},
                        new HOTASButton() {MapName = "test map 2,2", MapId = 4}
                    }
                }
            };


            device.SetModeProfile(newProfile);

            device.SetMode(1);

            Assert.Single(device.ButtonMap);
            Assert.Equal("test map 1,1", device.ButtonMap[0].MapName);

            device.SetMode(2);

            Assert.Equal(2, device.ButtonMap.Count);
            Assert.Equal("test map 2,1", device.ButtonMap[0].MapName);
            Assert.Equal("test map 2,2", device.ButtonMap[1].MapName);

            device.SetMode(3);

            Assert.Equal(2, device.ButtonMap.Count);
        }

        [Fact]
        public void force_button_press()
        {
            var device = CreateHotasDevice(out _, out _, out var hotasQueue);
            device.ForceButtonPress(JoystickOffset.Button1, true);
            hotasQueue.Received().ForceButtonPress(JoystickOffset.Button1, true);
        }

        [Fact]
        public void clear_button_map()
        {
            var device = CreateHotasDevice(out _, out _, out var hotasQueue);

            var newProfile = new Dictionary<int, ObservableCollection<IHotasBaseMap>>()
            {
                {
                    1,
                    new ObservableCollection<IHotasBaseMap>() {new HOTASButton() {MapName = "test map 1,1", MapId = 48}}
                },
                {
                    2,
                    new ObservableCollection<IHotasBaseMap>()
                    {
                        new HOTASButton() {MapName = "test map 2,1", MapId = 0},
                        new HOTASButton() {MapName = "test map 2,2", MapId = 4}
                    }
                }
            };


            device.SetModeProfile(newProfile);

            //verify this is the state we want to get back
            Assert.Equal(3, device.ButtonMap.Count);
            Assert.Equal(0, device.ButtonMap[0].MapId);

            device.SetMode(1);

            //baseline - verify we actually still have different values. set_mode test should also fail if this fails
            Assert.Single(device.ButtonMap);
            Assert.Equal(48, device.ButtonMap[0].MapId);

            device.ClearButtonMap();

            Assert.Equal(3, device.ButtonMap.Count);
            Assert.Equal(0, device.ButtonMap[0].MapId);
        }

        [Fact]
        public void get_button_state()
        {
            var device = CreateHotasDevice_GetButtonState();
            var state = device.GetButtonState((int)JoystickOffset.Button1);
            Assert.True(state);
        }
    }
}
using System;
using NSubstitute;
using SharpDX.DirectInput;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class HOTASDeviceTests
    {
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
            Assert.Throws<ArgumentNullException>(()=>new HOTASDevice(di, productId, deviceId, string.Empty, queue));
            Assert.Throws<ArgumentNullException>(()=>new HOTASDevice(di, productId, deviceId, null, queue));
            Assert.Throws<ArgumentNullException>(()=>new HOTASDevice(null, productId, deviceId, name, queue));
            Assert.Throws<ArgumentNullException>(()=>new HOTASDevice(di, productId, deviceId, name, null));
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
        }

    }
}

﻿using System;
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
            var deviceId = Guid.NewGuid();
            const string name = "test device name";

            var device = new HOTASDevice(di, deviceId, name, queue);

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
            var deviceId = Guid.NewGuid();
            const string name = "test";

            Assert.Throws<ArgumentNullException>(()=>new HOTASDevice(di, Guid.Empty, name, queue));
            Assert.Throws<ArgumentNullException>(()=>new HOTASDevice(di, deviceId, string.Empty, queue));
            Assert.Throws<ArgumentNullException>(()=>new HOTASDevice(di, deviceId, null, queue));
            Assert.Throws<ArgumentNullException>(()=>new HOTASDevice(null, deviceId, name, queue));
            Assert.Throws<ArgumentNullException>(()=>new HOTASDevice(di, deviceId, name, null));
        }

        [Fact]
        public void initialize_device_full_constructor()
        {
            var di = Substitute.For<IDirectInput>();
            var queue = Substitute.For<IHOTASQueue>();
            var deviceId = Guid.NewGuid();
            const string name = "test device name";

            var subJoystick = Substitute.For<IJoystick>();
            subJoystick.Capabilities.Returns(new Capabilities());
            var subJoystickFactory = Substitute.For<JoystickFactory>();
            subJoystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);

            var device = new HOTASDevice(di, subJoystickFactory, deviceId, name, queue);


            Assert.Equal(deviceId, device.DeviceId);
            Assert.Equal(name, device.Name);
            Assert.Single(device.ModeProfiles);
            Assert.NotNull(device.ModeProfiles[1]);
            Assert.Same(device.ModeProfiles[1], device.ButtonMap);
        }

    }
}

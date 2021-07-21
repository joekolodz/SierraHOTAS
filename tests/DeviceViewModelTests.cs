using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using NSubstitute;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class DeviceViewModelTests
    {
        private static DeviceViewModel CreateDeviceViewMode(string deviceName, out DirectInputFactory subDirectInputFactory, out IFileSystem subFileSystem, out MediaPlayerFactory subMediaPlayerFactory, out HOTASDeviceFactory subHotasDeviceFactory, out HOTASQueueFactory subHotasQueueFactory, out IHOTASDevice subHotasDevice)
        {
            subFileSystem = Substitute.For<IFileSystem>();
            subDirectInputFactory = Substitute.For<DirectInputFactory>();
            subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subHotasDeviceFactory = Substitute.For<HOTASDeviceFactory>();
            subHotasQueueFactory = Substitute.For<HOTASQueueFactory>();

            subHotasDevice = subHotasDeviceFactory.CreateHOTASDevice(subDirectInputFactory.CreateDirectInput(),Guid.Empty, Guid.Empty, deviceName,subHotasQueueFactory.CreateHOTASQueue());
            subHotasDevice.ButtonMap.Returns(new ObservableCollection<IHotasBaseMap>());
            subHotasDevice.Name.Returns(deviceName);
            subHotasDevice.ProductId.Returns(Guid.NewGuid());
            
            var deviceVm = new DeviceViewModel(Dispatcher.CurrentDispatcher, subFileSystem, subMediaPlayerFactory, subHotasDevice);
            return deviceVm;
        }

        [Fact]
        public void basic_constructor()
        {
            
            var deviceVm = CreateDeviceViewMode("Test Device", out _, out _, out _, out _, out _, out var subHotasDevice);
            Assert.NotNull(deviceVm);
            Assert.Equal("Test Device", deviceVm.Name);
            Assert.NotNull(deviceVm.ButtonMap);
            Assert.Empty(deviceVm.ButtonMap);

            Assert.Contains(deviceVm.PID, subHotasDevice.ProductId.ToString().ToUpper());
            Assert.Contains(deviceVm.VID, subHotasDevice.ProductId.ToString().ToUpper());
        }
    }
}

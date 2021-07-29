using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;

namespace SierraHOTAS.Factories
{
    public class DeviceViewModelFactory
    {
        public virtual DeviceViewModel CreateDeviceViewModel(IDispatcher dispatcher, IFileSystem fileSystem, MediaPlayerFactory mediaPlayerFactory, IHOTASDevice device)
        {
            return new DeviceViewModel(dispatcher, fileSystem, mediaPlayerFactory, device);
        }
    }
}

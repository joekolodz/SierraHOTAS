using SierraHOTAS.Models;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class HOTASDeviceTests
    {
        [Fact]
        public void initialize_device()
        {
            var test = new HOTASDevice();
            Assert.Single(test.ModeProfiles);
            Assert.NotNull(test.ModeProfiles[1]);
        }
    }
}

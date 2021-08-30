using SierraHOTAS.Models;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class ActivityItemTests
    {
        [Fact]
        public void get_scan_code_display()
        {
            var item = new ActivityItem();
            
            //normal
            item.ScanCode = (int)Win32Structures.ScanCodeShort.CAPITAL;
            item.IsKeyUp = true;
            item.IsExtended = false;
            Assert.Equal("CAPS", item.Key);
            
            item.ScanCode = (int)Win32Structures.ScanCodeShort.ESCAPE;
            item.IsKeyUp = true;
            item.IsExtended = false;
            Assert.Equal(Win32Structures.ScanCodeShort.ESCAPE.ToString(), item.Key);

            //extended
            item.ScanCode = (int)Win32Structures.ScanCodeShort.LWIN;
            item.IsKeyUp = false;
            item.IsExtended = true;
            Assert.Equal(Win32Structures.ScanCodeShort.LWIN.ToString(), item.Key);
            
            item.ScanCode = (int)Win32Structures.ScanCodeShort.NEXT;
            item.IsKeyUp = false;
            item.IsExtended = true;
            Assert.Equal("PG DN", item.Key);
        }

        [Fact]
        public void is_macro()
        {
            //code coverage!
            var item = new ActivityItem();
            
            item.IsMacro = true;
            Assert.True(item.IsMacro);
        }
    }
}

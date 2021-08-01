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
            item.Flags = 128;
            Assert.Equal("CAPS", item.Key);
            
            item.ScanCode = (int)Win32Structures.ScanCodeShort.ESCAPE;
            item.Flags = 128;
            Assert.Equal(Win32Structures.ScanCodeShort.ESCAPE.ToString(), item.Key);

            //extended
            item.ScanCode = (int)Win32Structures.ScanCodeShort.LWIN;
            item.Flags = 1;
            Assert.Equal(Win32Structures.ScanCodeShort.LWIN.ToString(), item.Key);
            
            item.ScanCode = (int)Win32Structures.ScanCodeShort.NEXT;
            item.Flags = 1;
            Assert.Equal("PG DN", item.Key);
        }

        [Fact]
        public void is_key_up()
        {
            var item = new ActivityItem();
            
            item.Flags = 0x80;
            Assert.True(item.IsKeyUp);

            item.Flags = 0x0;
            Assert.False(item.IsKeyUp);
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

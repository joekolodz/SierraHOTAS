using SierraHOTAS.Factories;

namespace SierraHOTAS.Tests
{
    using Newtonsoft.Json;
    using NSubstitute;
    using SierraHOTAS.Models;
    using SierraHOTAS.ViewModels;
    using Xunit;

    public class AxisMapViewModelTests
    {
        [Fact]
        public void constructor_test_valid_map()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);

            var map = new HOTASAxisMap();
            map.ButtonMap.Add(new HOTASButtonMap() { MapId = 1, MapName = "test 1", Type = HOTASButtonMap.ButtonType.AxisLinear });
            map.ButtonMap.Add(new HOTASButtonMap() { MapId = 2, MapName = "test 2", Type = HOTASButtonMap.ButtonType.AxisRadial });

            map.ReverseButtonMap.Add(new HOTASButtonMap() { MapId = 3, MapName = "test 3", Type = HOTASButtonMap.ButtonType.AxisLinear });
            map.ReverseButtonMap.Add(new HOTASButtonMap() { MapId = 4, MapName = "test 4", Type = HOTASButtonMap.ButtonType.AxisRadial });

            var mapVm = new AxisMapViewModel(subMediaPlayerFactory, subFileSystem, map);


            Assert.Equal(2, mapVm.ButtonMap.Count);
            Assert.Equal(2, mapVm.ReverseButtonMap.Count);

            Assert.Equal(mapVm.ButtonMap[0].ButtonId, map.ButtonMap[0].MapId);
            Assert.Equal(mapVm.ButtonMap[1].ButtonId, map.ButtonMap[1].MapId);

            Assert.Equal(mapVm.ReverseButtonMap[0].ButtonId, map.ReverseButtonMap[0].MapId);
            Assert.Equal(mapVm.ReverseButtonMap[1].ButtonId, map.ReverseButtonMap[1].MapId);

            Assert.True(subMediaPlayer.IsMuted);
            Assert.Equal(0, subMediaPlayer.Volume);
        }

        [Fact]
        public void constructor_test_no_sound_file()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            

            var map = new HOTASAxisMap();
            _ = new AxisMapViewModel(subMediaPlayerFactory, subFileSystem, map);

            Assert.True(subMediaPlayer.IsMuted);
            Assert.Equal(0, subMediaPlayer.Volume);
        }

        [Fact]
        public void constructor_test_valid_sound_file_from_deserialization()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);

            var map = new HOTASAxisMap();
            map.SoundFileName = "file name";
            var mapVm = new AxisMapViewModel(subMediaPlayerFactory, subFileSystem, map) { SoundFileName = "bob" };


            Assert.False(subMediaPlayer.IsMuted);
            Assert.Equal(0, subMediaPlayer.Volume);
        }

        [Fact]
        public void load_new_sound_command_no_file_name()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns(string.Empty);

            var map = new HOTASAxisMap();
            var mapVm = new AxisMapViewModel(subMediaPlayerFactory, subFileSystem, map);

            mapVm.OpenFileCommand.Execute(default);

            Assert.Null(mapVm.SoundFileName);
        }

        [Fact]
        public void load_new_sound_command_valid_file_name()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            var map = new HOTASAxisMap();

            var mapVm = new AxisMapViewModel(subMediaPlayerFactory, subFileSystem, map);

            mapVm.OpenFileCommand.Execute(default);

            Assert.NotNull(mapVm.SoundFileName);
        }

        [Fact]
        public void assign_segment_filter()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            var map = new HOTASAxisMap();
            var mapVm = new AxisMapViewModel(subMediaPlayerFactory, subFileSystem, map);

            var segmentLess = new Segment(1, 4000);
            var segmentEqual = new Segment(1, ushort.MaxValue);
            
            Assert.True(mapVm.SegmentFilter(segmentLess));
            Assert.False(mapVm.SegmentFilter(segmentEqual));
        }

        [Fact]
        public void segment_count_changed()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            var map = new HOTASAxisMap();
            var mapVm = new AxisMapViewModel(subMediaPlayerFactory, subFileSystem, map);

            mapVm.SegmentCount = 1;

        }

    }
}

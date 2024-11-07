namespace SierraHOTAS.Tests
{
    using NSubstitute;
    using SierraHOTAS.Factories;
    using SierraHOTAS.Models;
    using SierraHOTAS.ViewModels;
    using System;
    using System.Collections.ObjectModel;
    using Xunit;

    public class AxisMapViewModelTests
    {
        private AxisMapViewModel CreateAxisMapViewModel()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subDispatcherFactory = new DispatcherFactory();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var map = new HOTASAxis();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);

            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);
            return mapVm;
        }

        //private AxisMapViewModel CreateAxisMapViewModel(out HOTASAxis map)
        //{
        //    var subFileSystem = Substitute.For<IFileSystem>();
        //    var subMediaPlayer = Substitute.For<IMediaPlayer>();
        //    var subDispatcherFactory = Substitute.For<DispatcherFactory>();
        //    var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
        //    subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);

        //    map = new HOTASAxis();
        //    var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);
        //    return mapVm;
        //}

        private AxisMapViewModel CreateAxisMapViewModel(out IMediaPlayer subMediaPlayer)
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var map = new HOTASAxis();
            subMediaPlayer = Substitute.For<IMediaPlayer>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);

            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);
            return mapVm;
        }

        private AxisMapViewModel CreateAxisMapViewModel(out IFileSystem subFileSystem, out IMediaPlayer subMediaPlayer)
        {
            subFileSystem = Substitute.For<IFileSystem>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var map = new HOTASAxis();
            subMediaPlayer = Substitute.For<IMediaPlayer>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);

            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);
            return mapVm;
        }

        private AxisMapViewModel CreateAxisMapViewModel(out IFileSystem subFileSystem, out IMediaPlayer subMediaPlayer, out HOTASAxis map)
        {
            subFileSystem = Substitute.For<IFileSystem>();
            var subDispatcherFactory = new DispatcherFactory();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            map = new HOTASAxis();
            subMediaPlayer = Substitute.For<IMediaPlayer>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);

            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);
            return mapVm;
        }

        private AxisMapViewModel CreateAxisMapViewModel(out IMediaPlayer subMediaPlayer, HOTASAxis map)
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayer = Substitute.For<IMediaPlayer>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);

            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);
            return mapVm;
        }

        //private AxisMapViewModel CreateAxisMapViewModel(out IFileSystem subFileSystem, out IMediaPlayer subMediaPlayer, out DispatcherFactory subDispatcherFactory, out MediaPlayerFactory subMediaPlayerFactory, out HOTASAxis map)
        //{
        //    subFileSystem = Substitute.For<IFileSystem>();
        //    subMediaPlayer = Substitute.For<IMediaPlayer>();
        //    subDispatcherFactory = Substitute.For<DispatcherFactory>();
        //    subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
        //    subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);

        //    map = new HOTASAxis();
        //    var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);
        //    return mapVm;
        //}

        //private AxisMapViewModel CreateAxisMapViewModel(out IFileSystem subFileSystem, out IMediaPlayer subMediaPlayer, out DispatcherFactory subDispatcherFactory, out MediaPlayerFactory subMediaPlayerFactory, out HOTASAxis map)
        //{
        //    subFileSystem = Substitute.For<IFileSystem>();
        //    subMediaPlayer = Substitute.For<IMediaPlayer>();
        //    subDispatcherFactory = Substitute.For<DispatcherFactory>();
        //    subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
        //    subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);

        //    map = new HOTASAxis();
        //    var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);
        //    return mapVm;
        //}

        [Fact]
        public void constructor_test_valid_map()
        {
            var map = new HOTASAxis();
            map.ButtonMap.Add(new HOTASButton() { MapId = 1, MapName = "test 1", Type = HOTASButton.ButtonType.AxisLinear });
            map.ButtonMap.Add(new HOTASButton() { MapId = 2, MapName = "test 2", Type = HOTASButton.ButtonType.AxisRadial });

            map.ReverseButtonMap.Add(new HOTASButton() { MapId = 3, MapName = "test 3", Type = HOTASButton.ButtonType.AxisLinear });
            map.ReverseButtonMap.Add(new HOTASButton() { MapId = 4, MapName = "test 4", Type = HOTASButton.ButtonType.AxisRadial });

            var mapVm = CreateAxisMapViewModel(out var subMediaPlayer, map);

            mapVm.ButtonName = "new name";
            Assert.NotEmpty(map.MapName);

            map.Type = HOTASButton.ButtonType.Button;
            Assert.Equal(mapVm.Type, map.Type);

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
            CreateAxisMapViewModel(out IMediaPlayer subMediaPlayer);

            Assert.True(subMediaPlayer.IsMuted);
            Assert.Equal(0, subMediaPlayer.Volume);
        }

        [Fact]
        public void constructor_test_valid_sound_file_from_deserialization()
        {
            var map = new HOTASAxis();
            map.SoundFileName = "D:\\Development\\SierraHOTAS\\src\\Sounds\\click05.mp3";
            CreateAxisMapViewModel(out var subMediaPlayer, map);
            map.SoundFileName = "bob";

            Assert.False(subMediaPlayer.IsMuted);
            Assert.Equal(0, subMediaPlayer.Volume);
        }

        [Fact]
        public void load_new_sound_command_no_file_name()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns(string.Empty);

            var map = new HOTASAxis();
            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);

            mapVm.OpenFileCommand.Execute(default);

            Assert.Null(mapVm.SoundFileName);
        }

        [Fact]
        public void load_new_sound_command_valid_file_name()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            var map = new HOTASAxis();

            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);

            mapVm.OpenFileCommand.Execute(default);

            Assert.NotNull(mapVm.SoundFileName);
        }

        [Fact]
        public void assign_segment_filter()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            var map = new HOTASAxis();
            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);

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
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            var map = new HOTASAxis();
            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);

            mapVm.IsDirectional = true;
            mapVm.IsMultiAction = false;

            mapVm.SegmentCount = 1;
            Assert.Empty(mapVm.ButtonMap);

            mapVm.SegmentCount = 2;
            Assert.Single(mapVm.ButtonMap);
            Assert.Single(mapVm.ReverseButtonMap);
            Assert.Equal("Axis Button 1", mapVm.ButtonMap[0].ButtonName);
            Assert.Equal("Reverse Axis Button 1", mapVm.ReverseButtonMap[0].ButtonName);
            Assert.Equal(2, mapVm.Segments.Count);
            Assert.Equal(32767, mapVm.Segments[0].Value);
            Assert.Equal(ushort.MaxValue, mapVm.Segments[1].Value);

            mapVm.SegmentCount = 4;
            var boundaryIncrement = ushort.MaxValue / mapVm.SegmentCount;
            Assert.Equal(4, mapVm.Segments.Count);
            Assert.Equal(boundaryIncrement, mapVm.Segments[0].Value);
            Assert.Equal(boundaryIncrement * 2, mapVm.Segments[1].Value);
            Assert.Equal(boundaryIncrement * 3, mapVm.Segments[2].Value);
            Assert.Equal(ushort.MaxValue, mapVm.Segments[3].Value);
            Assert.Equal(4, mapVm.Segments.Count);

            mapVm.SegmentCount = 0;
            Assert.Empty(mapVm.Segments);
            Assert.Empty(mapVm.ButtonMap);
            Assert.Empty(mapVm.ReverseButtonMap);
            Assert.Empty(mapVm.Segments);

            //todo test property changed?
            //todo test boundary changed?
        }

        [Fact]
        public void reset_segment_count()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            var map = new HOTASAxis();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);

            mapVm.SegmentCount = 4;
            Assert.Equal(4, mapVm.Segments.Count);
            mapVm.ResetSegments();
            Assert.Empty(mapVm.Segments);
        }

        [Fact]
        public void segment_directional_changed()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            var map = new HOTASAxis();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);

            mapVm.IsDirectional = true;
            mapVm.IsMultiAction = false;

            mapVm.SegmentCount = 2;
            Assert.Single(mapVm.ButtonMap);
            Assert.Single(mapVm.ReverseButtonMap);
            Assert.Equal(2, mapVm.Segments.Count);

            mapVm.IsDirectional = false;
            mapVm.SegmentCount = 2;
            Assert.Single(mapVm.ButtonMap);
            Assert.Empty(mapVm.ReverseButtonMap);
            Assert.Equal("Axis Button 1", mapVm.ButtonMap[0].ButtonName);
            Assert.Equal(2, mapVm.Segments.Count);

            mapVm.SegmentCount = 4;
            Assert.Equal(4, mapVm.Segments.Count);
            Assert.Single(mapVm.ButtonMap);
            Assert.Empty(mapVm.ReverseButtonMap);
            Assert.Equal("Axis Button 1", mapVm.ButtonMap[0].ButtonName);
            Assert.Equal(4, mapVm.Segments.Count);
        }

        [Fact]
        public void segment_multiaction_changed()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            var map = new HOTASAxis();
            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);

            mapVm.IsDirectional = false;
            mapVm.IsMultiAction = true;

            mapVm.SegmentCount = 4;
            Assert.Equal(4, mapVm.ButtonMap.Count);
            Assert.Empty(mapVm.ReverseButtonMap);
            Assert.Equal("Axis Button 1", mapVm.ButtonMap[0].ButtonName);
            Assert.Equal("Axis Button 2", mapVm.ButtonMap[1].ButtonName);
            Assert.Equal("Axis Button 3", mapVm.ButtonMap[2].ButtonName);
            Assert.Equal("Axis Button 4", mapVm.ButtonMap[3].ButtonName);
        }

        [Fact]
        public void segment_directional_and_multiaction_changed()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            var map = new HOTASAxis();
            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);

            mapVm.IsDirectional = true;
            mapVm.IsMultiAction = true;

            mapVm.SegmentCount = 4;
            Assert.Equal(4, mapVm.Segments.Count);
            Assert.Equal(4, mapVm.ButtonMap.Count);
            Assert.Equal(4, mapVm.ReverseButtonMap.Count);
            Assert.Equal("Axis Button 1", mapVm.ButtonMap[0].ButtonName);
            Assert.Equal("Axis Button 2", mapVm.ButtonMap[1].ButtonName);
            Assert.Equal("Axis Button 3", mapVm.ButtonMap[2].ButtonName);
            Assert.Equal("Axis Button 4", mapVm.ButtonMap[3].ButtonName);
            Assert.Equal("Reverse Axis Button 1", mapVm.ReverseButtonMap[0].ButtonName);
            Assert.Equal("Reverse Axis Button 2", mapVm.ReverseButtonMap[1].ButtonName);
            Assert.Equal("Reverse Axis Button 3", mapVm.ReverseButtonMap[2].ButtonName);
            Assert.Equal("Reverse Axis Button 4", mapVm.ReverseButtonMap[3].ButtonName);

            mapVm.IsMultiAction = false;
            Assert.Equal(4, mapVm.Segments.Count);
            Assert.Single(mapVm.ButtonMap);
            Assert.Single(mapVm.ReverseButtonMap);
            Assert.Equal("Axis Button 1", mapVm.ButtonMap[0].ButtonName);
            Assert.Equal("Reverse Axis Button 1", mapVm.ReverseButtonMap[0].ButtonName);

            mapVm.IsMultiAction = true;
            Assert.Equal(4, mapVm.Segments.Count);
            Assert.Equal(4, mapVm.ButtonMap.Count);
            Assert.Equal(4, mapVm.ReverseButtonMap.Count);
            Assert.Equal("Axis Button 1", mapVm.ButtonMap[0].ButtonName);
            Assert.Equal("Axis Button 2", mapVm.ButtonMap[1].ButtonName);
            Assert.Equal("Axis Button 3", mapVm.ButtonMap[2].ButtonName);
            Assert.Equal("Axis Button 4", mapVm.ButtonMap[3].ButtonName);
            Assert.Equal("Reverse Axis Button 1", mapVm.ReverseButtonMap[0].ButtonName);
            Assert.Equal("Reverse Axis Button 2", mapVm.ReverseButtonMap[1].ButtonName);
            Assert.Equal("Reverse Axis Button 3", mapVm.ReverseButtonMap[2].ButtonName);
            Assert.Equal("Reverse Axis Button 4", mapVm.ReverseButtonMap[3].ButtonName);

            mapVm.IsDirectional = false;
            Assert.Equal(4, mapVm.Segments.Count);
            Assert.Equal(4, mapVm.ButtonMap.Count);
            Assert.Empty(mapVm.ReverseButtonMap);
            Assert.Equal("Axis Button 1", mapVm.ButtonMap[0].ButtonName);
            Assert.Equal("Axis Button 2", mapVm.ButtonMap[1].ButtonName);
            Assert.Equal("Axis Button 3", mapVm.ButtonMap[2].ButtonName);
            Assert.Equal("Axis Button 4", mapVm.ButtonMap[3].ButtonName);

            mapVm.IsMultiAction = false;
            Assert.Equal(4, mapVm.Segments.Count);
            Assert.Single(mapVm.ButtonMap);
            Assert.Empty(mapVm.ReverseButtonMap);
            Assert.Equal("Axis Button 1", mapVm.ButtonMap[0].ButtonName);
        }

        [Fact]
        public void sound_volume_tolerance()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            var map = new HOTASAxis();
            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);

            Assert.Equal(1.0d, mapVm.SoundVolume);
            mapVm.SoundVolume = 0.15d;
            Assert.Equal(0.15d, mapVm.SoundVolume);
            mapVm.SoundVolume = 0.1501d;
            Assert.Equal(0.15d, mapVm.SoundVolume);
            mapVm.SoundVolume = 0.16d;
            Assert.Equal(0.15d, mapVm.SoundVolume);
            mapVm.SoundVolume = 0.199d;
            Assert.Equal(0.15d, mapVm.SoundVolume);
            mapVm.SoundVolume = 0.20d;
            Assert.Equal(0.20d, mapVm.SoundVolume);
        }

        [Fact]
        public void direction_changed()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var subMediaPlayer = Substitute.For<IMediaPlayer>();
            var subDispatcherFactory = Substitute.For<DispatcherFactory>();
            var subMediaPlayerFactory = Substitute.For<MediaPlayerFactory>();
            subMediaPlayerFactory.CreateMediaPlayer().Returns(subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            var map = new HOTASAxis();
            var mapVm = new AxisMapViewModel(subDispatcherFactory.CreateDispatcher(), subMediaPlayerFactory, subFileSystem, map);

            Assert.True(mapVm.Direction == AxisDirection.Forward);
            map.SetAxis(800);
            map.SetAxis(800);
            map.SetAxis(800);
            map.SetAxis(800);
            map.SetAxis(800);
            map.SetAxis(800);
            map.SetAxis(800);
            map.SetAxis(800);
            map.SetAxis(800);
            map.SetAxis(800);
            Assert.True(mapVm.Direction == AxisDirection.Forward);
            map.SetAxis(700);
            Assert.True(mapVm.Direction == AxisDirection.Backward);
        }

        [Fact]
        public void media_player_played()
        {
            var mapVm = CreateAxisMapViewModel(out var subFileSystem, out var subMediaPlayer, out var map);
            subFileSystem.GetSoundFileName().Returns("some file");


            mapVm.SoundFileName = "not empty";
            mapVm.SegmentCount = 4;
            var boundaryIncrement = ushort.MaxValue / mapVm.SegmentCount;

            Assert.Equal(4, mapVm.Segments.Count);
            Assert.True(mapVm.Direction == AxisDirection.Forward);

            map.SetAxis(0);
            map.SetAxis(boundaryIncrement);

            subMediaPlayer.Received().Play();
        }

        [Fact]
        public void set_axis()
        {
            var mapVm = CreateAxisMapViewModel();

            Assert.Raises<AxisChangedViewModelEventArgs>(a => mapVm.OnAxisValueChanged += a, a => mapVm.OnAxisValueChanged -= a, () => mapVm.SetAxis(0));
        }

        [Fact]
        public void record_macro_start_command_execute()
        {
            var mapVm = CreateAxisMapViewModel();

            mapVm.IsMultiAction = true;
            mapVm.IsDirectional = true;
            mapVm.SegmentCount = 4;
            mapVm.ButtonMap[0].RecordMacroStartCommand.Execute(default);
            Assert.True(mapVm.ButtonMap[0].IsRecording);
            Assert.True(mapVm.ButtonMap[1].IsDisabledForced);
            Assert.True(mapVm.ButtonMap[2].IsDisabledForced);

            Assert.False(mapVm.ButtonMap[0].IsDisabledForced);
            Assert.False(mapVm.ButtonMap[1].IsRecording);
            Assert.False(mapVm.ButtonMap[2].IsRecording);

            Assert.True(mapVm.ReverseButtonMap[0].IsDisabledForced);
            Assert.True(mapVm.ReverseButtonMap[1].IsDisabledForced);
            Assert.True(mapVm.ReverseButtonMap[2].IsDisabledForced);

            Assert.False(mapVm.ReverseButtonMap[0].IsRecording);
            Assert.False(mapVm.ReverseButtonMap[1].IsRecording);
            Assert.False(mapVm.ReverseButtonMap[2].IsRecording);
        }

        [Fact]
        public void record_macro_stop_command_execute()
        {
            var mapVm = CreateAxisMapViewModel();

            mapVm.IsMultiAction = true;
            mapVm.IsDirectional = true;
            mapVm.SegmentCount = 4;
            mapVm.ButtonMap[0].RecordMacroStartCommand.Execute(default);

            mapVm.ButtonMap[0].AssignActions(new ActionCatalogItem() { ActionName = "fire", Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() { ScanCode = 1 } } });
            Assert.NotEmpty(mapVm.ButtonMap[0].Actions);

            mapVm.ButtonMap[0].RecordMacroStopCommand.Execute(default);
            Assert.NotEmpty(mapVm.ButtonMap[0].Actions);

            Assert.False(mapVm.ButtonMap[0].IsDisabledForced);
            Assert.False(mapVm.ButtonMap[1].IsDisabledForced);
            Assert.False(mapVm.ButtonMap[2].IsDisabledForced);

            Assert.False(mapVm.ButtonMap[0].IsRecording);
            Assert.False(mapVm.ButtonMap[1].IsRecording);
            Assert.False(mapVm.ButtonMap[2].IsRecording);

            Assert.False(mapVm.ReverseButtonMap[0].IsDisabledForced);
            Assert.False(mapVm.ReverseButtonMap[1].IsDisabledForced);
            Assert.False(mapVm.ReverseButtonMap[2].IsDisabledForced);

            Assert.False(mapVm.ReverseButtonMap[0].IsRecording);
            Assert.False(mapVm.ReverseButtonMap[1].IsRecording);
            Assert.False(mapVm.ReverseButtonMap[2].IsRecording);

        }
        [Fact]
        public void record_macro_cancel_command_execute()
        {
            var mapVm = CreateAxisMapViewModel();

            mapVm.IsMultiAction = true;
            mapVm.IsDirectional = true;
            mapVm.SegmentCount = 4;
            mapVm.ButtonMap[0].RecordMacroStartCommand.Execute(default);

            mapVm.ButtonMap[0].AssignActions(new ActionCatalogItem() { ActionName = "fire", Actions = new ObservableCollection<ButtonAction>() { new ButtonAction() { ScanCode = 1 } } });
            Assert.NotEmpty(mapVm.ButtonMap[0].Actions);

            mapVm.ButtonMap[0].RecordMacroCancelCommand.Execute(default);

            Assert.Empty(mapVm.ButtonMap[0].Actions);

            Assert.False(mapVm.ButtonMap[0].IsDisabledForced);
            Assert.False(mapVm.ButtonMap[1].IsDisabledForced);
            Assert.False(mapVm.ButtonMap[2].IsDisabledForced);

            Assert.False(mapVm.ButtonMap[0].IsRecording);
            Assert.False(mapVm.ButtonMap[1].IsRecording);
            Assert.False(mapVm.ButtonMap[2].IsRecording);

            Assert.False(mapVm.ReverseButtonMap[0].IsDisabledForced);
            Assert.False(mapVm.ReverseButtonMap[1].IsDisabledForced);
            Assert.False(mapVm.ReverseButtonMap[2].IsDisabledForced);

            Assert.False(mapVm.ReverseButtonMap[0].IsRecording);
            Assert.False(mapVm.ReverseButtonMap[1].IsRecording);
            Assert.False(mapVm.ReverseButtonMap[2].IsRecording);
        }

        [Fact]
        public void open_file_command()
        {
            var mapVm = CreateAxisMapViewModel(out var subFileSystem, out var subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            mapVm.OpenFileCommand.Execute(default);
            Assert.NotEmpty(mapVm.SoundFileName);
            Assert.False(subMediaPlayer.IsMuted);
        }

        [Fact]
        public void close_file_command()
        {
            var mapVm = CreateAxisMapViewModel(out var subFileSystem, out var subMediaPlayer);
            subFileSystem.GetSoundFileName().Returns("some file");

            mapVm.OpenFileCommand.Execute(default);
            mapVm.RemoveSoundCommand.Execute(default);
            Assert.Empty(mapVm.SoundFileName);
            Assert.True(subMediaPlayer.IsMuted);
        }

        [Fact]
        public void segments_property_changed()
        {
            var mapVm = CreateAxisMapViewModel();
            mapVm.SegmentCount = 4;
            Assert.Raises<EventArgs>(a => mapVm.SegmentBoundaryChanged += a, a => mapVm.SegmentBoundaryChanged -= a, () => mapVm.Segments[0].Value = 1);

        }
    }
}
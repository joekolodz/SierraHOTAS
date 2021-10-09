using NSubstitute;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;
using SierraHOTAS.Win32;
using System;
using System.Collections.Generic;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class QuickProfilePanelViewModelTests
    {
        [Fact]
        public void basic_constructor()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new QuickProfilePanelViewModel(Substitute.For<IEventAggregator>(), null));
            Assert.Equal("Value cannot be null.\r\nParameter name: fileSystem", exception.Message);

            exception = Assert.Throws<ArgumentNullException>(() => new QuickProfilePanelViewModel(null, Substitute.For<IFileSystem>()));
            Assert.Equal("Value cannot be null.\r\nParameter name: eventAggregator", exception.Message);
        }

        [Fact]
        public void addicted_to_code_coverage()
        {
            var _ = new QuickProfilePanelViewModel();
        }

        [Fact]
        public void setup_quick_profiles()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var list = new Dictionary<int, QuickProfileItem> { { 1, new QuickProfileItem() { AutoLoad = true, Path = "test path" } } };
            subFileSystem.LoadQuickProfilesList(Arg.Any<string>()).Returns(list);
            var panelVm = new QuickProfilePanelViewModel(Substitute.For<IEventAggregator>(), subFileSystem);
            Assert.Empty(panelVm.QuickProfilesList);
            panelVm.SetupQuickProfiles();
            Assert.Single(panelVm.QuickProfilesList);
            Assert.True(panelVm.QuickProfilesList[1].AutoLoad);
            Assert.Equal("test path", panelVm.QuickProfilesList[1].Path);
        }

        [Fact]
        public void quick_profile_requested_command_no_path()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            subFileSystem.ChooseHotasProfileForQuickLoad().Returns("");

            var panelVm = new QuickProfilePanelViewModel(Substitute.For<IEventAggregator>(), subFileSystem);
            panelVm.QuickProfileRequestedCommand.Execute(1);
            subFileSystem.DidNotReceive().FileOpen(default);
        }

        [Fact]
        public void quick_profile_requested_command_no_hotas()
        {
            var subEventAggregator = Substitute.For<IEventAggregator>();
            var subFileSystem = Substitute.For<IFileSystem>();
            subFileSystem.ChooseHotasProfileForQuickLoad().Returns("test path");
            subFileSystem.FileOpen(Arg.Any<string>()).Returns(e => null);

            var panelVm = new QuickProfilePanelViewModel(subEventAggregator, subFileSystem);
            panelVm.QuickProfileRequestedCommand.Execute(1);
            subFileSystem.Received().FileOpen(Arg.Any<string>());
            subEventAggregator.Received().Publish(Arg.Any<ShowMessageWindowEvent>());
        }

        [Fact]
        public void quick_profile_requested_command()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            subFileSystem.ChooseHotasProfileForQuickLoad().Returns("test path");

            var hotas = new HOTASCollection(Substitute.For<DirectInputFactory>(), Substitute.For<JoystickFactory>(), Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), Substitute.For<HOTASDeviceFactory>());
            subFileSystem.FileOpen(Arg.Any<string>()).Returns(hotas);

            var panelVm = new QuickProfilePanelViewModel(Substitute.For<IEventAggregator>(), subFileSystem);

            Assert.PropertyChanged(panelVm, "QuickProfilesList", () => panelVm.QuickProfileRequestedCommand.Execute(1));
            subFileSystem.Received().SaveQuickProfilesList(Arg.Any<Dictionary<int, QuickProfileItem>>(), "quick-profile-list.json");
            Assert.Equal("test path", panelVm.QuickProfilesList[1].Path);
        }

        [Fact]
        public void quick_profile_selected_command_not_found()
        {
            var subEventAggregator = Substitute.For<IEventAggregator>();
            var panelVm = new QuickProfilePanelViewModel(subEventAggregator, Substitute.For<IFileSystem>());
            panelVm.QuickProfileSelectedCommand.Execute(1);
            subEventAggregator.DidNotReceive().Publish(Arg.Any<QuickProfileSelectedEvent>());
        }

        [Fact]
        public void quick_profile_selected_command_found()
        {
            var expectedProfileId = 43;
            var expectedPath = "test path";
            var list = new Dictionary<int, QuickProfileItem> { { expectedProfileId, new QuickProfileItem() { AutoLoad = true, Path = expectedPath } } };
            var eventAggregator = new EventAggregator();
            eventAggregator.Subscribe<QuickProfileSelectedEvent>(quick_profile_selected_event_handler);
            var panelVm = new QuickProfilePanelViewModel(eventAggregator, Substitute.For<IFileSystem>());
            panelVm.QuickProfilesList = list;

            profileId = 0;
            profilePath = string.Empty;
            panelVm.QuickProfileSelectedCommand.Execute(43);

            Assert.Equal(expectedProfileId, profileId);
            Assert.Equal(expectedPath, profilePath);
        }

        private int profileId;
        private string profilePath;
        private void quick_profile_selected_event_handler(QuickProfileSelectedEvent arg)
        {
            profileId = arg.Id;
            profilePath = arg.Path;
        }

        [Fact]
        public void quick_profile_cleared_command_not_found()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var list = new Dictionary<int, QuickProfileItem> { { 1, new QuickProfileItem() { AutoLoad = true, Path = "test path" } } };
            var panelVm = new QuickProfilePanelViewModel(Substitute.For<IEventAggregator>(), subFileSystem);
            panelVm.QuickProfilesList = list;
            panelVm.QuickProfileClearedCommand.Execute(43);
            subFileSystem.DidNotReceiveWithAnyArgs().SaveQuickProfilesList(default, default);
            Assert.Single(panelVm.QuickProfilesList);
        }

        [Fact]
        public void quick_profile_cleared_command_found()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var list = new Dictionary<int, QuickProfileItem> { { 1, new QuickProfileItem() { AutoLoad = true, Path = "test path" } } };
            var panelVm = new QuickProfilePanelViewModel(Substitute.For<IEventAggregator>(), subFileSystem);
            panelVm.QuickProfilesList = list;

            Assert.PropertyChanged(panelVm, "QuickProfilesList", () => panelVm.QuickProfileClearedCommand.Execute(1));

            subFileSystem.Received().SaveQuickProfilesList(panelVm.QuickProfilesList, "quick-profile-list.json");
            Assert.Empty(panelVm.QuickProfilesList);
        }

        [Fact]
        public void quick_profile_auto_load_selected_command_not_found()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var list = new Dictionary<int, QuickProfileItem>
            {
                { 1, new QuickProfileItem() { AutoLoad = false, Path = "test path 1" } },
                { 2, new QuickProfileItem() { AutoLoad = true, Path = "test path 2" } }
            };
            var panelVm = new QuickProfilePanelViewModel(Substitute.For<IEventAggregator>(), subFileSystem);

            panelVm.QuickProfilesList = list;
            panelVm.AutoLoadSelectedCommand.Execute(43);
            subFileSystem.DidNotReceiveWithAnyArgs().SaveQuickProfilesList(default, default);
            Assert.Equal(2, panelVm.QuickProfilesList.Count);
            Assert.False(list[1].AutoLoad);
            Assert.True(list[2].AutoLoad);
        }

        [Fact]
        public void quick_profile_auto_load_selected_command_found()
        {
            var subFileSystem = Substitute.For<IFileSystem>();
            var list = new Dictionary<int, QuickProfileItem>
            {
                { 1, new QuickProfileItem() { AutoLoad = false, Path = "test path 1" } },
                { 2, new QuickProfileItem() { AutoLoad = true, Path = "test path 2" } }
            };
            var panelVm = new QuickProfilePanelViewModel(Substitute.For<IEventAggregator>(), subFileSystem);

            panelVm.QuickProfilesList = list;
            panelVm.AutoLoadSelectedCommand.Execute(1);
            subFileSystem.Received().SaveQuickProfilesList(panelVm.QuickProfilesList, "quick-profile-list.json");
            Assert.True(list[1].AutoLoad);
            Assert.False(list[2].AutoLoad);
        }

        [Fact]
        public void get_auto_load_path_null()
        {
            var panelVm = new QuickProfilePanelViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IFileSystem>());
            Assert.NotNull(panelVm.QuickProfilesList);
            Assert.Equal(string.Empty, panelVm.GetAutoLoadPath());

            panelVm.QuickProfilesList = null;
            Assert.Equal(string.Empty, panelVm.GetAutoLoadPath());
        }

        [Fact]
        public void get_auto_load_path()
        {
            var list = new Dictionary<int, QuickProfileItem>
            {
                { 1, new QuickProfileItem() { AutoLoad = false, Path = "test path 1" } },
                { 2, new QuickProfileItem() { AutoLoad = true, Path = "auto load path" } }
            };
            var panelVm = new QuickProfilePanelViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IFileSystem>());
            panelVm.QuickProfilesList = list;
            Assert.Equal("auto load path", panelVm.GetAutoLoadPath());
        }

        [Fact]
        public void close_app()
        {
            var panelVm = new QuickProfilePanelViewModel(Substitute.For<IEventAggregator>(), Substitute.For<IFileSystem>());
            Assert.Raises<EventArgs>(a => panelVm.Close += a, a => panelVm.Close -= a, () => panelVm.CloseApp());
        }
    }
}

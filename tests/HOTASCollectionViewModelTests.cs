using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Mime;
using System.Security.Permissions;
using System.Windows.Threading;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NSubstitute.Routing.Handlers;
using SierraHOTAS.Models;
using Xunit;
using Pose;
using SierraHOTAS.ModeProfileWindow.ViewModels;
using SierraHOTAS.ViewModels;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SierraHOTAS.Tests
{
    public class HOTASCollectionViewModelTests
    {
        private ITestOutputHelper _output;
        public HOTASCollectionViewModelTests(ITestOutputHelper output)
        {
            _output = output;
        }

        //public static class DispatcherUtil
        //{
        //    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //    public static void DoEvents()
        //    {
        //        var frame = new DispatcherFrame();
        //        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
        //            new DispatcherOperationCallback(ExitFrame), frame);
        //        Dispatcher.PushFrame(frame);
        //    }

        //    private static object ExitFrame(object frame)
        //    {
        //        ((DispatcherFrame)frame).Continue = false;
        //        return null;
        //    }
        //}

        private static HOTASCollectionViewModel CreateHotasCollectionViewModel(out IHOTASCollection subHotasCollection, out IFileSystem subFileSystem, out Dictionary<int, ModeActivationItem> subModeProfileButtons, out ActionCatalogViewModel subActionVm)
        {
            subFileSystem = Substitute.For<IFileSystem>();

            subHotasCollection = Substitute.For<IHOTASCollection>();
            subHotasCollection.Devices = new ObservableCollection<HOTASDevice>() { new HOTASDevice() };

            subModeProfileButtons = new Dictionary<int, ModeActivationItem>();
            subHotasCollection.ModeProfileActivationButtons.Returns(subModeProfileButtons);

            subActionVm = Substitute.For<ActionCatalogViewModel>();

            var hotasVm = new HOTASCollectionViewModel(Dispatcher.CurrentDispatcher, subFileSystem, subHotasCollection, subActionVm);
            return hotasVm;
        }

        [Fact]
        public void construction()
        {
            var shimPath = Shim.Replace(() => Path.GetFileNameWithoutExtension(Is.A<string>())).With((string s) => "Test");
            PoseContext.Isolate(() =>
            {
                Assert.Equal("Test", Path.GetFileNameWithoutExtension("This parameter doesn't matter"));
            }, shimPath);
        }

        [Fact]
        public void set_mode()
        {
            const int expectedMode = 43;

            var hotasVm = CreateHotasCollectionViewModel(out var subHotasCollection, out _, out _, out _);
            subHotasCollection.Mode = expectedMode;

            var receivedEvents = new List<int>();

            hotasVm.ModeProfileChanged += delegate (object sender, ModeProfileChangedEventArgs e)
            {
                receivedEvents.Add(e.Mode);
            };

            //Test
            hotasVm.Initialize();
            hotasVm.SetMode(expectedMode);

            Assert.Equal(expectedMode, receivedEvents[0]);
            subHotasCollection.Received().SetMode(expectedMode);
            //Assert that this should have been called
            //hotasVm.Devices[0].RebuildMap();
        }

        [Fact]
        public void selection_changed_command()
        {
            var hotasVm = CreateHotasCollectionViewModel(out _, out _, out _, out _);
            var model = new DeviceViewModel(){Name = "Test"};
            
            //Test
            hotasVm.Initialize();
            hotasVm.SelectionChangedCommand.Execute(model);

            Assert.Same(model, hotasVm.SelectedDevice);
        }

        [Fact]
        public void file_saved_command()
        {
            var expectedProfileSetFileName = "Test File Name";


            var hotasVm = CreateHotasCollectionViewModel(out var subHotasCollection, out var subFileSystem, out _, out _);
            subFileSystem.LastSavedFileName = expectedProfileSetFileName;
            hotasVm.Initialize();
            hotasVm.SaveFileCommand.Execute(0);

            subFileSystem.ReceivedWithAnyArgs().FileSave(default);
            subHotasCollection.Received().ClearUnassignedActions();
            Assert.Equal(expectedProfileSetFileName, hotasVm.ProfileSetFileName);
        }
    }
}

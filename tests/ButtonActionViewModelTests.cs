using NSubstitute;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class ButtonActionViewModelTests
    {
        [Fact]
        public void constructor()
        {
            var actionVm = new ButtonActionViewModel();
            Assert.Equal("LBUTTON", actionVm.ScanCode);
        }

        [Fact]
        public void constructor_with_action()
        {
            var button = new ButtonAction() {Flags = 0, ScanCode = 48, TimeInMilliseconds = 0};
            var actionVm = new ButtonActionViewModel(button);
            Assert.Equal("B", actionVm.ScanCode);
        }

        [Fact]
        public void is_key_up_false()
        {
            var button = new ButtonAction() {Flags = 0, ScanCode = 48, TimeInMilliseconds = 0};
            var actionVm = new ButtonActionViewModel(button);
            Assert.False(actionVm.IsKeyUp);
        }

        [Fact]
        public void is_key_up_true()
        {
            var button = new ButtonAction() {Flags = 128, ScanCode = 48, TimeInMilliseconds = 0};
            var actionVm = new ButtonActionViewModel(button);
            Assert.True(actionVm.IsKeyUp);
        }

        [Fact]
        public void time_getter()
        {
            const int time = 4000;
            var button = new ButtonAction() {Flags = 0, ScanCode = 48, TimeInMilliseconds = time};
            var actionVm = new ButtonActionViewModel(button);
            Assert.Equal(time,actionVm.TimeInMilliseconds);
        }

        [Fact]
        public void time_setter()
        {
            const int time = 4000;
            var button = new ButtonAction() {Flags = 0, ScanCode = 48, TimeInMilliseconds = 0};
            var actionVm = new ButtonActionViewModel(button);
            Assert.NotEqual(time, actionVm.TimeInMilliseconds);
            actionVm.TimeInMilliseconds = time;
            Assert.Equal(time,actionVm.TimeInMilliseconds);
        }

    }
}

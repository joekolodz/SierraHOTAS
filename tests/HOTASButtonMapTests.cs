using SierraHOTAS.Models;
using System.Linq;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class HOTASButtonMapTests
    {
        [Fact]
        public void is_macro_false_when_empty()
        {
            var map = new HOTASButton();
            Assert.False(map.IsMacro);
        }

        [Fact]
        public void is_macro_false_with_one_item_time_zero()
        {
            var map = new HOTASButton();
            map.ActionCatalogItem.Actions.Add(new ButtonAction() { TimeInMilliseconds = 0 });
            Assert.False(map.IsMacro);
        }

        [Fact]
        public void is_macro_true_with_one_item_time_not_zero()
        {
            var map = new HOTASButton();
            map.ActionCatalogItem.Actions.Add(new ButtonAction() { TimeInMilliseconds = 1 });
            Assert.True(map.IsMacro);
        }

        [Fact]
        public void record_clears_actions()
        {
            var map = new HOTASButton();
            map.ActionCatalogItem.Actions.Add(new ButtonAction() { ScanCode = 1, Flags = 1, TimeInMilliseconds = 1 });
            map.Record();
            Assert.Empty(map.ActionCatalogItem.Actions);
        }

        [Fact]
        public void record_sets_history_empty_when_no_items()
        {
            var map = new HOTASButton();
            map.Record();
            map.Cancel();
            Assert.Empty(map.ActionCatalogItem.Actions);
        }

        [Fact]
        public void record_sets_history_with_one_item()
        {
            var map = new HOTASButton();
            var item = new ButtonAction() { ScanCode = 1, Flags = 1, TimeInMilliseconds = 1 };
            map.ActionCatalogItem.Actions.Add(item);
            map.Record();
            map.Cancel();
            Assert.NotEmpty(map.ActionCatalogItem.Actions);
            Assert.Same(item, map.ActionCatalogItem.Actions.First());
        }

        [Fact]
        public void stop_keeps_only_new_actions()
        {
            var map = new HOTASButton();
            var item = new ButtonAction() { ScanCode = 1, Flags = 1, TimeInMilliseconds = 1 };
            map.ActionCatalogItem.Actions.Add(item);
            map.Record();

            var newItem = new ButtonAction() { ScanCode = 1, Flags = 1, TimeInMilliseconds = 1 };
            map.ActionCatalogItem.Actions.Add(newItem);

            map.Stop();
            Assert.NotSame(item, map.ActionCatalogItem.Actions.First());
            Assert.Same(newItem, map.ActionCatalogItem.Actions.First());
        }

        [Fact]
        public void record_sets_keyboard_state_true()
        {
            Keyboard.IsKeySuppressionActive = false;
            var map = new HOTASButton();
            map.Record();
            Assert.True(Keyboard.IsKeySuppressionActive);
        }

        [Fact]
        public void stop_sets_keyboard_state_false()
        {
            Keyboard.IsKeySuppressionActive = false;
            var map = new HOTASButton();
            map.Record();
            map.Stop();
            Assert.False(Keyboard.IsKeySuppressionActive);
        }

        [Fact]
        public void cancel_sets_keyboard_state_false()
        {
            Keyboard.IsKeySuppressionActive = false;
            var map = new HOTASButton();
            map.Record();
            map.Cancel();
            Assert.False(Keyboard.IsKeySuppressionActive);
        }

        [Fact]
        public void record_adds_action()
        {
            var map = new HOTASButton();
            map.Record();

            Keyboard.SimulateKeyPressTest(1, 0);
            
            Assert.Single(map.ActionCatalogItem.Actions);
        }

        [Fact]
        public void reset_shift()
        {
            var map = new HOTASButton {IsShift = true, ShiftModePage = 1};
            map.ResetShift();
            Assert.False(map.IsShift);
            Assert.Equal(0, map.ShiftModePage);
        }

        [Fact]
        public void to_string()
        {
            const int LLKHF_UP = 0x80;
            var map = new HOTASButton();
            map.ActionCatalogItem.Actions.Add(new ButtonAction() { ScanCode = 48, Flags = LLKHF_UP });
            Assert.Equal("[KEY_B^]", map.ToString());
            map.ActionCatalogItem.Actions.Add(new ButtonAction() { ScanCode = 49});
            Assert.Equal("[KEY_B^][KEY_Nv]", map.ToString());
        }
    }
}
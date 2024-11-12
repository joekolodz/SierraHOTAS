using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class HOTASAxisMapTests
    {
        [Fact]
        public void constructor()
        {
            var map = new HOTASAxis();
            Assert.NotNull(map.Segments);
            Assert.NotNull(map.ButtonMap);
            Assert.NotNull(map.ReverseButtonMap);
            Assert.Equal(1d, map.SoundVolume);
        }

        [Fact]
        public void set_value_direction_changed_when_is_directional_true()
        {
            var map = new HOTASAxis();
            map.IsDirectional = true;
            Assert.True(map.Direction == AxisDirection.Forward);
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
            Assert.True(map.Direction == AxisDirection.Forward);
            Assert.Raises<AxisDirectionChangedEventArgs>(a => map.AxisDirectionChanged += a, a => map.AxisDirectionChanged -= a, () => map.SetAxis(700));
            Assert.True(map.Direction == AxisDirection.Backward);
        }

        [Fact]
        public void set_value_direction_not_changed_when_is_directional_false()
        {
            var map = new HOTASAxis();
            map.IsDirectional = false;
            Assert.True(map.Direction == AxisDirection.Forward);
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
            Assert.True(map.Direction == AxisDirection.Forward);
            map.SetAxis(700);
            Assert.True(map.Direction == AxisDirection.Forward);
        }

        [Fact]
        public void set_value_detect_segment_changed_not_enough_segments()
        {
            var map = new HOTASAxis();
            map.Segments.Clear();
            map.SetAxis(1000);
            Assert.False(map.IsSegmentChanged);

            map.Segments.Add(new Segment(1, 1));
            map.SetAxis(1000);
            Assert.False(map.IsSegmentChanged);
        }

        [Fact]
        public void set_value_detect_segment_changed()
        {
            var map = new HOTASAxis();
            map.Segments.Clear();
            map.Segments.Add(new Segment(1, 0));
            map.Segments.Add(new Segment(2, 10));
            map.Segments.Add(new Segment(3, 20));
            Assert.Raises<AxisSegmentChangedEventArgs>(a => map.AxisSegmentChanged += a, a => map.AxisSegmentChanged -= a, () => map.SetAxis(5));
            Assert.True(map.IsSegmentChanged);
        }

        [Fact]
        public void set_value_same_segment()
        {
            var map = new HOTASAxis();
            map.Segments.Clear();
            map.Segments.Add(new Segment(1, 0));
            map.Segments.Add(new Segment(2, 10));
            map.Segments.Add(new Segment(3, 20));
            map.SetAxis(5);
            Assert.True(map.IsSegmentChanged);
            map.SetAxis(5);
            Assert.False(map.IsSegmentChanged);
        }

        [Fact]
        public void calculate_segment_range_no_segments()
        {
            var map = new HOTASAxis();
            map.ButtonMap.Add(new HOTASButton());
            map.ReverseButtonMap.Add(new HOTASButton());
            map.CalculateSegmentRange(0);
            Assert.Empty(map.Segments);
            Assert.Empty(map.ButtonMap);
            Assert.Empty(map.ReverseButtonMap);
        }

        [Fact]
        public void calculate_segment_not_directional_not_multi_action()
        {
            var map = new HOTASAxis();
            map.IsDirectional = false;
            map.IsMultiAction = false;//should create single action button map list

            map.ButtonMap.Clear();
            map.ReverseButtonMap.Clear();

            map.CalculateSegmentRange(2);
            Assert.NotEmpty(map.Segments);
            Assert.Equal(2, map.Segments.Count);
            Assert.NotEmpty(map.ButtonMap);
            Assert.Equal("Axis Button 1", map.ButtonMap[0].MapName);
            Assert.Empty(map.ReverseButtonMap);
        }

        [Fact]
        public void calculate_segment_is_directional_not_multi_action()
        {
            var map = new HOTASAxis();
            map.IsDirectional = true;
            map.IsMultiAction = false;//should create single action button map list

            map.ButtonMap.Clear();
            map.ReverseButtonMap.Clear();

            map.CalculateSegmentRange(2);
            Assert.NotEmpty(map.Segments);
            Assert.Equal(2, map.Segments.Count);
            Assert.NotEmpty(map.ButtonMap);
            Assert.Equal("Axis Button 1", map.ButtonMap[0].MapName);
            Assert.NotEmpty(map.ReverseButtonMap);
            Assert.Equal("Reverse Axis Button 1", map.ReverseButtonMap[0].MapName);
        }

        [Fact]
        public void calculate_segment_is_directional_not_multi_action_existing_buttons()
        {
            var map = new HOTASAxis();
            map.IsDirectional = true;
            map.IsMultiAction = false;//should leave a single action button map list

            map.ButtonMap.Add(new HOTASButton() { MapName = "kept" });
            map.ButtonMap.Add(new HOTASButton() { MapName = "removed" });
            map.ReverseButtonMap.Add(new HOTASButton() { MapName = "kept" });
            map.ReverseButtonMap.Add(new HOTASButton() { MapName = "removed" });

            map.CalculateSegmentRange(2);
            Assert.NotEmpty(map.Segments);
            Assert.Equal(2, map.Segments.Count);
            Assert.Single(map.ButtonMap);
            Assert.Equal("kept", map.ButtonMap[0].MapName);
            Assert.Single(map.ReverseButtonMap);
            Assert.Equal("kept", map.ReverseButtonMap[0].MapName);
        }

        [Fact]
        public void calculate_segment_not_directional_is_multi_action_less_segments_than_buttons()
        {
            var map = new HOTASAxis();
            map.IsDirectional = false;
            map.IsMultiAction = true;//should create action button map for each segment

            map.ButtonMap.Clear();
            map.ButtonMap.Add(new HOTASButton() { MapName = "kept" });
            map.ButtonMap.Add(new HOTASButton() { MapName = "removed 1" });
            map.ButtonMap.Add(new HOTASButton() { MapName = "removed 2" });

            map.ReverseButtonMap.Clear();

            map.CalculateSegmentRange(1);
            Assert.NotEmpty(map.Segments);
            Assert.Single(map.Segments);
            Assert.NotEmpty(map.ButtonMap);
            Assert.Equal("kept", map.ButtonMap[0].MapName);
            Assert.Empty(map.ReverseButtonMap);
        }

        [Fact]
        public void calculate_segment_is_directional_is_multi_action_less_segments_than_buttons()
        {
            var map = new HOTASAxis();
            map.IsDirectional = true;
            map.IsMultiAction = true;//should create action button map for each segment

            map.ButtonMap.Clear();

            map.ReverseButtonMap.Clear();
            map.ReverseButtonMap.Add(new HOTASButton() { MapName = "kept" });
            map.ReverseButtonMap.Add(new HOTASButton() { MapName = "removed 1" });
            map.ReverseButtonMap.Add(new HOTASButton() { MapName = "removed 2" });

            map.CalculateSegmentRange(1);
            Assert.NotEmpty(map.Segments);
            Assert.Single(map.Segments);
            Assert.NotEmpty(map.ButtonMap);
            Assert.NotEmpty(map.ReverseButtonMap);
            Assert.Equal("kept", map.ReverseButtonMap[0].MapName);
        }

        [Fact]
        public void calculate_segment_not_directional_is_multi_action()
        {
            var map = new HOTASAxis();
            map.IsDirectional = false;
            map.IsMultiAction = true;//should create action button map for each segment

            map.ButtonMap.Clear();
            map.ReverseButtonMap.Clear();

            map.CalculateSegmentRange(3);
            Assert.NotEmpty(map.Segments);
            Assert.Equal(3, map.Segments.Count);
            Assert.NotEmpty(map.ButtonMap);
            Assert.Equal("Axis Button 1", map.ButtonMap[0].MapName);
            Assert.Equal("Axis Button 2", map.ButtonMap[1].MapName);
            Assert.Equal("Axis Button 3", map.ButtonMap[2].MapName);
            Assert.Empty(map.ReverseButtonMap);

            map.CalculateSegmentRange(4);
            Assert.Equal(4, map.Segments.Count);
            Assert.Equal("Axis Button 4", map.ButtonMap[3].MapName);
        }

        [Fact]
        public void calculate_segment_is_directional_Is_multi_action()
        {
            var map = new HOTASAxis();
            map.IsDirectional = true;
            map.IsMultiAction = true;//should create action button map for each segment

            map.ButtonMap.Clear();
            map.ReverseButtonMap.Clear();

            map.CalculateSegmentRange(3);
            Assert.NotEmpty(map.Segments);
            Assert.Equal(3, map.Segments.Count);

            Assert.NotEmpty(map.ButtonMap);
            Assert.Equal("Axis Button 1", map.ButtonMap[0].MapName);
            Assert.Equal("Axis Button 2", map.ButtonMap[1].MapName);
            Assert.Equal("Axis Button 3", map.ButtonMap[2].MapName);

            Assert.NotEmpty(map.ReverseButtonMap);
            Assert.Equal("Reverse Axis Button 1", map.ReverseButtonMap[0].MapName);
            Assert.Equal("Reverse Axis Button 2", map.ReverseButtonMap[1].MapName);
            Assert.Equal("Reverse Axis Button 3", map.ReverseButtonMap[2].MapName);


            map.CalculateSegmentRange(4);
            Assert.Equal(4, map.Segments.Count);
            Assert.Equal("Axis Button 4", map.ButtonMap[3].MapName);
            Assert.Equal(4, map.Segments.Count);
            Assert.Equal("Reverse Axis Button 4", map.ReverseButtonMap[3].MapName);
        }

        [Fact]
        public void clear_segments()
        {
            var map = new HOTASAxis();
            map.CalculateSegmentRange(4);
            Assert.Equal(4, map.Segments.Count);
            map.ClearSegments();
            Assert.Empty(map.Segments);
        }

        [Fact]
        public void get_segment_from_raw_value_not_found()
        {
            var map = new HOTASAxis();
            map.CalculateSegmentRange(4);
            Assert.Equal(4, map.Segments.Count);
            Assert.Equal(0, map.GetSegmentFromRawValue(65536));
        }

        [Fact]
        public void get_segment_from_raw_value()
        {
            var map = new HOTASAxis();
            map.CalculateSegmentRange(4);
            Assert.Equal(4, map.Segments.Count);
            Assert.Equal(1, map.GetSegmentFromRawValue(0));
            Assert.Equal(1, map.GetSegmentFromRawValue(16383));
            Assert.Equal(2, map.GetSegmentFromRawValue(16384));
            Assert.Equal(2, map.GetSegmentFromRawValue(32766));
            Assert.Equal(3, map.GetSegmentFromRawValue(32767));
            Assert.Equal(3, map.GetSegmentFromRawValue(49149));
            Assert.Equal(4, map.GetSegmentFromRawValue(49150));
            Assert.Equal(4, map.GetSegmentFromRawValue(65535));
        }

        [Fact]
        public void segment_filter()
        {
            var map = new HOTASAxis();
            Assert.False(map.SegmentFilter(new Segment(1, ushort.MaxValue)));
            Assert.True(map.SegmentFilter(new Segment(1, ushort.MinValue)));
            Assert.True(map.SegmentFilter(new Segment(1, ushort.MaxValue - 1)));
        }

        [Fact]
        public void get_button_from_raw_value_not_directional_not_multi_action()
        {
            var map = new HOTASAxis();
            map.IsDirectional = false;
            map.IsMultiAction = false;

            map.CalculateSegmentRange(4);
            Assert.Equal("Axis Button 1", map.GetButtonMapFromRawValue(16383).MapName);
            Assert.Equal("Axis Button 1", map.GetButtonMapFromRawValue(32766).MapName);
            Assert.Equal("Axis Button 1", map.GetButtonMapFromRawValue(49149).MapName);
            Assert.Equal("Axis Button 1", map.GetButtonMapFromRawValue(65535).MapName);

        }

        [Fact]
        public void get_button_from_raw_value_forward_directional_not_multi_action()
        {
            var map = new HOTASAxis();
            map.IsDirectional = true;
            map.IsMultiAction = false;

            map.CalculateSegmentRange(4);
            Assert.Equal("Axis Button 1", map.GetButtonMapFromRawValue(16383).MapName);
            Assert.Equal("Axis Button 1", map.GetButtonMapFromRawValue(32766).MapName);
            Assert.Equal("Axis Button 1", map.GetButtonMapFromRawValue(49149).MapName);
            Assert.Equal("Axis Button 1", map.GetButtonMapFromRawValue(65535).MapName);
        }


        [Fact]
        public void get_button_from_raw_value_not_directional_is_multi_action()
        {
            var map = new HOTASAxis();
            map.IsDirectional = false;
            map.IsMultiAction = true;

            map.CalculateSegmentRange(4);
            Assert.Equal("Axis Button 1", map.GetButtonMapFromRawValue(16383).MapName);
            Assert.Equal("Axis Button 2", map.GetButtonMapFromRawValue(32766).MapName);
            Assert.Equal("Axis Button 3", map.GetButtonMapFromRawValue(49149).MapName);
            Assert.Equal("Axis Button 4", map.GetButtonMapFromRawValue(65535).MapName);
        }

        [Fact]
        public void get_button_from_raw_value_forward_directional_is_multi_action()
        {
            var map = new HOTASAxis();
            map.IsDirectional = true;
            map.IsMultiAction = true;

            map.CalculateSegmentRange(4);
            Assert.Equal("Axis Button 1", map.GetButtonMapFromRawValue(16383).MapName);
            Assert.Equal("Axis Button 2", map.GetButtonMapFromRawValue(32766).MapName);
            Assert.Equal("Axis Button 3", map.GetButtonMapFromRawValue(49149).MapName);
            Assert.Equal("Axis Button 4", map.GetButtonMapFromRawValue(65535).MapName);
        }

        [Fact]
        public void get_button_from_raw_value_backward_directional_is_multi_action()
        {
            var map = new HOTASAxis();
            map.IsDirectional = true;
            map.IsMultiAction = true;

            map.CalculateSegmentRange(4);

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
            map.SetAxis(700);//set direction backward

            Assert.Equal("Reverse Axis Button 1", map.GetButtonMapFromRawValue(16383).MapName);
            Assert.Equal("Reverse Axis Button 2", map.GetButtonMapFromRawValue(32766).MapName);
            Assert.Equal("Reverse Axis Button 3", map.GetButtonMapFromRawValue(49149).MapName);
            Assert.Equal("Reverse Axis Button 4", map.GetButtonMapFromRawValue(65535).MapName);
        }

        [Fact]
        public void get_button_from_raw_value_backward_directional_not_multi_action()
        {
            var map = new HOTASAxis();
            map.IsDirectional = true;
            map.IsMultiAction = false;

            map.CalculateSegmentRange(4);

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
            map.SetAxis(700);//set direction backward


            Assert.Equal("Reverse Axis Button 1", map.GetButtonMapFromRawValue(16383).MapName);
            Assert.Equal("Reverse Axis Button 1", map.GetButtonMapFromRawValue(32766).MapName);
            Assert.Equal("Reverse Axis Button 1", map.GetButtonMapFromRawValue(49149).MapName);
            Assert.Equal("Reverse Axis Button 1", map.GetButtonMapFromRawValue(65535).MapName);
        }

        [Fact]
        public void clear_unassigned_actions()
        {
            var map = new HOTASAxis();
            map.ButtonMap.Add(new HOTASButton() { ActionCatalogItem = new ActionCatalogItem() { ActionName = ActionCatalogItem.NO_ACTION_TEXT, Actions = new ObservableCollection<ButtonAction>() { new ButtonAction(), new ButtonAction() } } });
            map.ButtonMap.Add(new HOTASButton() { ActionCatalogItem = new ActionCatalogItem() { ActionName = "item 1", Actions = new ObservableCollection<ButtonAction>() { new ButtonAction(), new ButtonAction() } } });
            map.ButtonMap.Add(new HOTASButton() { ActionCatalogItem = new ActionCatalogItem() { ActionName = "item 2", Actions = new ObservableCollection<ButtonAction>() { new ButtonAction(), new ButtonAction() } } });

            map.ReverseButtonMap.Add(new HOTASButton() { ActionCatalogItem = new ActionCatalogItem() { ActionName = ActionCatalogItem.NO_ACTION_TEXT, Actions = new ObservableCollection<ButtonAction>() { new ButtonAction(), new ButtonAction() } } });
            map.ReverseButtonMap.Add(new HOTASButton() { ActionCatalogItem = new ActionCatalogItem() { ActionName = "item 1", Actions = new ObservableCollection<ButtonAction>() { new ButtonAction(), new ButtonAction() } } });
            map.ReverseButtonMap.Add(new HOTASButton() { ActionCatalogItem = new ActionCatalogItem() { ActionName = "item 2", Actions = new ObservableCollection<ButtonAction>() { new ButtonAction(), new ButtonAction() } } });

            map.ClearUnassignedActions();
            Assert.Empty(map.ButtonMap[0].ActionCatalogItem.Actions);
            Assert.Equal(2, map.ButtonMap[1].ActionCatalogItem.Actions.Count);
            Assert.Equal(2, map.ButtonMap[2].ActionCatalogItem.Actions.Count);

            Assert.Empty(map.ReverseButtonMap[0].ActionCatalogItem.Actions);
            Assert.Equal(2, map.ReverseButtonMap[1].ActionCatalogItem.Actions.Count);
            Assert.Equal(2, map.ReverseButtonMap[2].ActionCatalogItem.Actions.Count);
        }

        [Fact]
        public void segment_boundary_test()
        {
            var map = new HOTASAxis();
            map.CalculateSegmentRange(4);
            map.Segments[0].Value = 34000;//adjusting the first segment passed the second segment will pump the second segment up by 655 passed the new value of the first segment
            Assert.Equal(34655, map.Segments[1].Value);

            map.Segments[3].Value = ushort.MaxValue+100;//can't push the last segment any further
            Assert.Equal(64880, map.Segments[3].Value);
        }
    }
}

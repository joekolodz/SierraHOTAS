using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Castle.Core.Smtp;
using NSubstitute;
using SharpDX.DirectInput;
using SierraHOTAS.Models;
using SierraHOTAS.Win32;
using Xunit;
using Xunit.Abstractions;
using JoystickOffset = SierraHOTAS.Models.JoystickOffset;

namespace SierraHOTAS.Tests
{
    public class HOTASQueueTests
    {
        private readonly ITestOutputHelper _output;

        public HOTASQueueTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class TestJoystick_BasicQueue : IJoystick
        {
            public JoystickUpdate[] TestData { get; set; }

            public TestJoystick_BasicQueue(int dataBufferSize)
            {
                TestData = new JoystickUpdate[dataBufferSize];
                Capabilities = new Capabilities() { AxeCount = 2, ButtonCount = 128, PovCount = 4 };
            }

            public int BufferSize { get; set; }
            public void Acquire()
            {

            }

            public Capabilities Capabilities { get; }

            public void GetCurrentState(ref JoystickState joystickState)
            {
                joystickState.Buttons[0] = true;
            }

            public bool IsAxisPresent(string axisName)
            {
                return true;
            }

            public void Poll()
            {

            }

            public JoystickUpdate[] GetBufferedData()
            {
                if (TestData == null)
                {
                    throw new Exception("forced exception");
                }

                var returnData = new JoystickUpdate[TestData.Length];
                TestData.CopyTo(returnData, 0);
                TestData = new JoystickUpdate[0];
                return returnData;
            }

            public void Unacquire()
            {

            }

            public void Dispose()
            {

            }
        }

        [Fact]
        public void translate_point_of_view_offset_same_as_button()
        {
            const int expected = -224;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.Released);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void translate_point_of_view_offset_north()
        {
            const int expected = 32;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorth);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void translate_point_of_view_offset_north_east()
        {
            const int expected = 1152032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthEast);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void translate_point_of_view_offset_east()
        {
            const int expected = 2304032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVEast);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void translate_point_of_view_offset_south_east()
        {
            const int expected = 3456032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVSouthEast);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void translate_point_of_view_offset_south()
        {
            const int expected = 4608032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVSouth);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void translate_point_of_view_offset_south_west()
        {
            const int expected = 5760032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVSouthWest);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void translate_point_of_view_offset_west()
        {
            const int expected = 6912032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVWest);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void translate_point_of_view_offset_north_west()
        {
            const int expected = 8064032;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthWest);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void verify_translate_works_for_other_pov_buttons()
        {
            var expected = 36;
            var actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV2, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorth);
            Assert.Equal(expected, actual);

            expected = 8064036;
            actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV2, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthWest);
            Assert.Equal(expected, actual);

            expected = 40;
            actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV3, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorth);
            Assert.Equal(expected, actual);

            expected = 8064040;
            actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV3, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthWest);
            Assert.Equal(expected, actual);


            expected = 44;
            actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV4, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorth);
            Assert.Equal(expected, actual);

            expected = 8064044;
            actual = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV4, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthWest);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void keystroke_up_sent()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 2);
            var map = CreateTestHotasTestMapWithButton();

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);

            queue.KeystrokeUpSent += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonPressed };
            joystick.TestData[1] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonReleased };
            
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(5);
            }

            Assert.True(isEventCalled);
        }

        [Fact]
        public void keystroke_down_sent()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 1);
            var map = CreateTestHotasTestMapWithButton();

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);
            queue.KeystrokeDownSent += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonPressed };

            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        [Fact]
        public void macro_start()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 1);
            var map = CreateTestHotasTestMapWithButton(timeInMilliseconds: 1);

            var subKeyboard = Substitute.For<IKeyboard>();
            subKeyboard.KeyDownRepeatDelay.Returns(35);
            var queue = new HOTASQueue(subKeyboard);
            queue.Listen(joystick, map);

            queue.MacroStarted += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonPressed };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        [Fact]
        public void macro_cancelled()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 2);
            var map = CreateTestHotasTestMapWithButton(timeInMilliseconds: 1);

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);

            queue.MacroCancelled += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonPressed };
            joystick.TestData[1] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonPressed };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        [Fact]
        public void button_pressed()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 1);
            var map = CreateTestHotasTestMapWithButton();

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);

            queue.ButtonPressed += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonPressed };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        [Fact]
        public void button_released()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 2);
            var map = CreateTestHotasTestMapWithButton();

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);

            queue.ButtonReleased += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonPressed };
            joystick.TestData[1] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonReleased };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        [Fact]
        public void axis_changed()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 1);
            var map = CreateTestHotasTestMapWithAxis();

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);

            queue.AxisChanged += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.X, Sequence = 0, Timestamp = 0, Value = 43000 };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        [Fact]
        public void axis_changed_causes_keypress_down()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 2);
            var map = CreateTestHotasTestMapWithAxis();

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);

            queue.KeystrokeDownSent += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.X, Sequence = 0, Timestamp = 0, Value = 4000 };
            joystick.TestData[1] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.X, Sequence = 0, Timestamp = 0, Value = 6000 };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        [Fact]
        public void axis_changed_causes_keypress_up()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 2);
            var map = CreateTestHotasTestMapWithAxis();

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);

            queue.KeystrokeUpSent += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.X, Sequence = 0, Timestamp = 0, Value = 4000 };
            joystick.TestData[1] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.X, Sequence = 0, Timestamp = 0, Value = 6000 };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        [Fact]
        public void mode_profile_selected()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 1);
            var map = CreateTestHotasTestMapWithShiftMode();

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);

            queue.ModeProfileSelected += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonPressed };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        [Fact]
        public void shift_released()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 1);
            var map = CreateTestHotasTestMapWithShiftMode();

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);

            queue.ShiftReleased += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonReleased };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        [Fact]
        public void lost_connection_to_device()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 1);
            var map = CreateTestHotasTestMapWithButton();

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);

            queue.LostConnectionToDevice += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData = null;
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        [Fact]
        public void force_button_press()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 1);
            var map = CreateTestHotasTestMapWithButton();

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);
            Assert.False(isEventCalled);
            
            queue.ButtonPressed += (sender, e) => { isEventCalled = true; };
            queue.ForceButtonPress(JoystickOffset.Button1, true);
            
            Assert.True(isEventCalled);
        }

        [Fact]
        public void stop_loop()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 1);
            var map = CreateTestHotasTestMapWithButton();

            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);
            queue.KeystrokeDownSent += (sender, e) => { isEventCalled = true; };

            //setup baseline test to prove it's actually running
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonPressed };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);

            //test that we stop the loop and no events come through
            isEventCalled = false;
            queue.Stop();

            joystick.TestData = new JoystickUpdate[1];
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.Button1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.ButtonState.ButtonPressed };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.False(isEventCalled);
        }

        [Fact]
        public void handle_pov_button_press()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 1);
            var map = CreateTestHotasTestMapWithPOV();
            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);
            queue.ButtonPressed += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.POV1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthEast };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        [Fact]
        public void handle_pov_button_release()
        {
            var isEventCalled = false;
            var timeOut = 80;

            var joystick = new TestJoystick_BasicQueue(dataBufferSize: 2);
            var map = CreateTestHotasTestMapWithPOV();
            var queue = new HOTASQueue(Substitute.For<IKeyboard>());
            queue.Listen(joystick, map);
            queue.ButtonReleased += (sender, e) => { isEventCalled = true; };
            Assert.False(isEventCalled);
            joystick.TestData[0] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.POV1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthEast };
            joystick.TestData[1] = new JoystickUpdate() { RawOffset = (int)JoystickOffset.POV1, Sequence = 0, Timestamp = 0, Value = (int)JoystickOffsetValues.PointOfViewPositionValues.Released };
            while (!isEventCalled && --timeOut > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
            Assert.True(isEventCalled);
        }

        private static ObservableCollection<IHotasBaseMap> CreateTestHotasTestMapWithButton(int timeInMilliseconds = 0)
        {
            var map = new ObservableCollection<IHotasBaseMap>()
            {
                new HOTASButton()
                {
                    MapId = (int) JoystickOffset.Button1, MapName = "trigger", Type = HOTASButton.ButtonType.Button,
                    ActionName = "fire", ActionCatalogItem = new ActionCatalogItem()
                    {
                        ActionName = "fire", Actions = new ObservableCollection<ButtonAction>()
                        {
                            new ButtonAction() {ScanCode = 28, TimeInMilliseconds = timeInMilliseconds},
                            new ButtonAction() {ScanCode = 28, IsKeyUp = true}
                        }
                    }
                }
            };
            return map;
        }

        private static ObservableCollection<IHotasBaseMap> CreateTestHotasTestMapWithAxis()
        {
            var map = new ObservableCollection<IHotasBaseMap>()
            {
                new HOTASAxis()
                {
                    MapId = (int) JoystickOffset.X, MapName = "axis 1", Type = HOTASButton.ButtonType.AxisLinear,
                    Segments = new ObservableCollection<Segment>()
                    {
                        new Segment(1, 0),
                        new Segment(2, 5000),
                        new Segment(3, 10000),
                    },
                    ButtonMap = new ObservableCollection<HOTASButton>()
                    {
                        new HOTASButton()
                        {
                            MapId = 1, MapName = "axis triggered button 1", Type = HOTASButton.ButtonType.Button,
                            ActionName = "click 1!", ActionCatalogItem = new ActionCatalogItem()
                            {
                                ActionName = "click 1!", Actions = new ObservableCollection<ButtonAction>()
                                {
                                    new ButtonAction() {ScanCode = 43},
                                    new ButtonAction() {ScanCode = 43, IsKeyUp = true}
                                }
                            }
                        }
                    }
                }
            };
            return map;
        }

        private static ObservableCollection<IHotasBaseMap> CreateTestHotasTestMapWithShiftMode()
        {
            var map = new ObservableCollection<IHotasBaseMap>()
            {
                new HOTASButton()
                {
                    MapId = (int) JoystickOffset.Button1, MapName = "trigger", Type = HOTASButton.ButtonType.Button, IsShift = true, ShiftModePage = 1,
                    ActionName = "change modes", ActionCatalogItem = new ActionCatalogItem()
                }
            };
            return map;
        }

        private static ObservableCollection<IHotasBaseMap> CreateTestHotasTestMapWithPOV()
        {
            var map = new ObservableCollection<IHotasBaseMap>()
            {
                new HOTASButton()
                {
                    MapId = HOTASQueue.TranslatePointOfViewOffset(JoystickOffset.POV1, (int)JoystickOffsetValues.PointOfViewPositionValues.POVNorthEast), MapName = "pov1NE", Type = HOTASButton.ButtonType.POV,
                    ActionName = "pov1NE action", ActionCatalogItem = new ActionCatalogItem()
                    {
                        ActionName = "pov1NE action", Actions = new ObservableCollection<ButtonAction>()
                        {
                            new ButtonAction() {ScanCode = 17},
                            new ButtonAction() {ScanCode = 17, IsKeyUp = true}
                        }
                    }
                }
            };
            return map;
        }
    }
}

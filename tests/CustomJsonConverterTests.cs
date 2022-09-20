using Newtonsoft.Json;
using NSubstitute;
using SharpDX.DirectInput;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using SierraHOTAS.Win32;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class CustomJsonConverterTests
    {
        private static HOTASCollection CreateHotasCollection(out IDirectInput directInput, out JoystickFactory joystickFactory, out IHOTASQueue hotasQueue)
        {
            directInput = Substitute.For<IDirectInput>();
            hotasQueue = Substitute.For<IHOTASQueue>();
            var productId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            const string name = "test device name";

            var subJoystick = Substitute.For<IJoystick>();
            subJoystick.Capabilities.Returns(new Capabilities() { PovCount = 1, AxeCount = 4, ButtonCount = 20 });
            subJoystick.IsAxisPresent(Arg.Any<string>()).Returns(true);

            joystickFactory = Substitute.For<JoystickFactory>();
            joystickFactory.CreateJoystick(default, default).ReturnsForAnyArgs(subJoystick);
            var device = new HOTASDevice(directInput, joystickFactory, productId, deviceId, name, hotasQueue);

            var hotasCollection = new HOTASCollection(Substitute.For<DirectInputFactory>(), joystickFactory, Substitute.For<HOTASQueueFactory>(Substitute.For<IKeyboard>()), Substitute.For<HOTASDeviceFactory>(), Substitute.For<ActionCatalog>());
            hotasCollection.Devices = new ObservableCollection<IHOTASDevice>() { device };

            hotasCollection.ModeActivationButtons.Add(1, new ModeActivationItem() { DeviceId = device.DeviceId, DeviceName = device.Name });
            return hotasCollection;
        }

        //TODO - refactor to support action catalog persistence
        //[Fact]
        //public void write_converter()
        //{
        //    var converter = new CustomJsonConverter();
        //    var list = CreateHotasCollection(out _, out _, out _);
        //    var axisMap = list.Devices[0].Modes[1][1] as HOTASAxis;

        //    axisMap.MapName = "X axis";
        //    axisMap.Segments = new ObservableCollection<Segment>() { new Segment(1, 50), new Segment(2, 500) };
        //    axisMap.IsDirectional = true;
        //    axisMap.IsDirectional = true;
        //    axisMap.IsMultiAction = true;
        //    axisMap.SoundVolume = 0.4d;
        //    axisMap.SoundFileName = "some file";
        //    axisMap.ButtonMap.Add(new HOTASButton()
        //    {
        //        MapName = "X axis forward",
        //        MapId = 0,
        //        ActionName = "forward",
        //        ActionCatalogItem = new ActionCatalogItem()
        //        {
        //            ActionName = "forward",
        //            Actions = new ObservableCollection<ButtonAction>()
        //            {
        //                new ButtonAction(){ScanCode = 30},
        //                new ButtonAction(){ScanCode = 30, IsKeyUp = true}
        //            }
        //        }
        //    });

        //    axisMap.ReverseButtonMap.Add(new HOTASButton()
        //    {
        //        MapName = "X axis reverse",
        //        MapId = 0,
        //        ActionName = "reverse",
        //        ActionCatalogItem = new ActionCatalogItem()
        //        {
        //            ActionName = "reverse",
        //            Actions = new ObservableCollection<ButtonAction>()
        //            {
        //                new ButtonAction(){ScanCode = 35, IsExtended = true},
        //                new ButtonAction(){ScanCode = 35, IsKeyUp = true, IsExtended = true},
        //            }
        //        }
        //    });


        //    var povMap = list.Devices[0].Modes[1][28] as HOTASButton;
        //    povMap.MapName = "POVSouth";
        //    povMap.MapId = 4608032;
        //    povMap.Type = HOTASButton.ButtonType.POV;
        //    povMap.ActionName = "test pov action";
        //    povMap.IsShift = true;
        //    povMap.ActionCatalogItem = new ActionCatalogItem()
        //    {
        //        ActionName = "test pov action",
        //        Actions = new ObservableCollection<ButtonAction>()
        //            {
        //                new ButtonAction() {ScanCode = 40},
        //                new ButtonAction() {ScanCode = 40, IsKeyUp = true}
        //            }
        //    };

        //    var jsonResult = JsonConvert.SerializeObject(list, Formatting.Indented, converter);

        //    Debug.WriteLine(jsonResult);

        //    var newList = JsonConvert.DeserializeObject<HOTASCollection>(jsonResult, new JsonSerializerSettings() {Converters = new List<JsonConverter>() {new CustomJsonConverter()}});
            
        //    Assert.NotNull(newList);
        //    Assert.Equal(2, newList.Devices[0].Modes[1].Count);

        //    Assert.Equal("1.0.0", newList.JsonFormatVersion);
        //    Assert.Single(newList.Devices);
        //    Assert.Single(newList.ModeActivationButtons);
        //    Assert.Equal("test device name", newList.ModeActivationButtons[1].DeviceName);
        //    Assert.Contains(newList.Devices[0].Modes[1], b =>b.MapName == "X axis");
        //    Assert.Contains(newList.Devices[0].Modes[1], b =>b.MapName == "POVSouth");

        //    var axis = newList.Devices[0].Modes[1][0] as HOTASAxis;
        //    Assert.NotNull(axis);
        //    Assert.Equal(30, axis.ButtonMap[0].ActionCatalogItem.Actions[1].ScanCode);
        //    Assert.True(axis.ButtonMap[0].ActionCatalogItem.Actions[1].IsKeyUp);
        //    Assert.False(axis.ButtonMap[0].ActionCatalogItem.Actions[1].IsExtended);

        //    Assert.Equal(35, axis.ReverseButtonMap[0].ActionCatalogItem.Actions[1].ScanCode);
        //    Assert.True(axis.ReverseButtonMap[0].ActionCatalogItem.Actions[1].IsKeyUp);
        //    Assert.True(axis.ReverseButtonMap[0].ActionCatalogItem.Actions[1].IsExtended);

        //    var pov = newList.Devices[0].Modes[1][1] as HOTASButton;
        //    Assert.NotNull(axis);
        //    Assert.Equal("POVSouth", pov.MapName);
        //    Assert.Equal(4608032, pov.MapId);
        //    Assert.Equal(40, pov.ActionCatalogItem.Actions[1].ScanCode);
        //    Assert.True(pov.ActionCatalogItem.Actions[1].IsKeyUp);
        //}
    }
}

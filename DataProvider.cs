using SierraHOTAS.Models;
using System;
using System.Collections.ObjectModel;

namespace SierraHOTAS
{
    public static class DataProvider
    {
        /*
         * f0c05a72-8f66-4cdd-ab69-bca2ee7f3907, Throttle
d67c807f-98fd-4443-94ab-b2724e44f805, Stick
         */
        public static ObservableCollection<HOTASDevice> GetDeviceList()
        {
            Guid.TryParse("f0c05a72-8f66-4cdd-ab69-bca2ee7f3907", out var throttleGuid);
            Guid.TryParse("d67c807f-98fd-4443-94ab-b2724e44f805", out var stickGuid);
            var devices = new ObservableCollection<HOTASDevice>()
            {
                new HOTASDevice(throttleGuid, "Joe's Test Throttle")
                {
                    ButtonMap = new ObservableCollection<IHotasBaseMap>()
                    {
                        new HOTASButtonMap(){MapName = "Button 1", Type = HOTASButtonMap.ButtonType.Button, MapId = 54, Actions = new ObservableCollection<ButtonAction>()
                        {
                            new ButtonAction(){ ScanCode = (int) Win32Structures.ScanCodeShort.SPACE, TimeInMilliseconds = 0, Flags = 0},
                            new ButtonAction(){ ScanCode = (int) Win32Structures.ScanCodeShort.SPACE, TimeInMilliseconds = 0, Flags = (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP}
                        }},
                        new HOTASAxisMap(){MapName = "Axis 2", Type = HOTASButtonMap.ButtonType.AxisRadial, MapId = 55, IsDirectional =true }
                    }
                },
                new HOTASDevice(stickGuid, "Joe's Test Stick")
                {
                    ButtonMap = new ObservableCollection<IHotasBaseMap>()
                    {
                        new HOTASAxisMap(){MapName = "Axis 1", Type = HOTASButtonMap.ButtonType.AxisLinear, MapId = 42, IsDirectional =true },
                        new HOTASAxisMap(){MapName = "Axis 2", Type = HOTASButtonMap.ButtonType.AxisRadial, MapId = 43, IsDirectional =false },
                        new HOTASButtonMap(){MapName = "Button 0", Type = HOTASButtonMap.ButtonType.POV, Actions = new ObservableCollection<ButtonAction>()},
                        new HOTASButtonMap()
                        {
                            MapName = "Button 1", Type = HOTASButtonMap.ButtonType.Button, MapId = 55,
                            Actions = new ObservableCollection<ButtonAction>()
                            {
                                new ButtonAction() {ScanCode = (int) Win32Structures.ScanCodeShort.LMENU, TimeInMilliseconds = 0, Flags = (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED},
                                new ButtonAction() {ScanCode = (int) Win32Structures.ScanCodeShort.LMENU, TimeInMilliseconds = 0, Flags = (int) (Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP | Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED)}
                            }
                        },
                        new HOTASButtonMap()
                        {
                            MapName = "Button 2", Type = HOTASButtonMap.ButtonType.Button, MapId = 56,
                            Actions = new ObservableCollection<ButtonAction>()
                            {
                                new ButtonAction() {ScanCode = (int) Win32Structures.ScanCodeShort.RSHIFT, TimeInMilliseconds = 0, Flags = (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED},
                                new ButtonAction() {ScanCode = (int) Win32Structures.ScanCodeShort.KEY_B, TimeInMilliseconds = 0, Flags = 0},
                                new ButtonAction() {ScanCode = (int) Win32Structures.ScanCodeShort.KEY_B, TimeInMilliseconds = 0, Flags = (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP},
                                new ButtonAction() {ScanCode = (int) Win32Structures.ScanCodeShort.RSHIFT, TimeInMilliseconds = 0, Flags = (int) (Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP | Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED)}
                            }
                        }
                    }
                }
            };
            return devices;
        }
    }
}

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
                    ButtonMap = new ObservableCollection<HOTASMap>()
                    {
                        new HOTASMap(){ButtonName = "Button 1", Type = HOTASMap.ButtonType.Button, ButtonId = 54, Actions = new ObservableCollection<ButtonAction>()
                        {
                            new ButtonAction(){ ScanCode = (int) Win32Structures.ScanCodeShort.SPACE, TimeInMilliseconds = 0, Flags = 0},
                            new ButtonAction(){ ScanCode = (int) Win32Structures.ScanCodeShort.SPACE, TimeInMilliseconds = 0, Flags = (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP}
                        }}
                    }
                },
                new HOTASDevice(stickGuid, "Joe's Test Stick")
                {
                    ButtonMap = new ObservableCollection<HOTASMap>()
                    {
                        new HOTASMap(){ButtonName = "Button 0", Actions = new ObservableCollection<ButtonAction>()},
                        new HOTASMap()
                        {
                            ButtonName = "Button 1", Type = HOTASMap.ButtonType.Button, ButtonId = 55,
                            Actions = new ObservableCollection<ButtonAction>()
                            {
                                new ButtonAction() {ScanCode = (int) Win32Structures.ScanCodeShort.LMENU, TimeInMilliseconds = 0, Flags = (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED},
                                new ButtonAction() {ScanCode = (int) Win32Structures.ScanCodeShort.LMENU, TimeInMilliseconds = 0, Flags = (int) (Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP | Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED)}
                            }
                        },
                        new HOTASMap()
                        {
                            ButtonName = "Button 2", Type = HOTASMap.ButtonType.Button, ButtonId = 56,
                            Actions = new ObservableCollection<ButtonAction>()
                            {
                                new ButtonAction() {ScanCode = (int) Win32Structures.ScanCodeShort.RSHIFT, TimeInMilliseconds = 0, Flags = 0},
                                new ButtonAction() {ScanCode = (int) Win32Structures.ScanCodeShort.KEY_B, TimeInMilliseconds = 0, Flags = (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED},
                                new ButtonAction() {ScanCode = (int) Win32Structures.ScanCodeShort.KEY_B, TimeInMilliseconds = 0, Flags = (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP},
                                new ButtonAction() {ScanCode = (int) Win32Structures.ScanCodeShort.RSHIFT, TimeInMilliseconds = 0, Flags = (int) (Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP | Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED)}
                            }
                        },
                        new HOTASMap(){ButtonName = "Button 3", Actions = new ObservableCollection<ButtonAction>()},
                        new HOTASMap(){ButtonName = "Button 4", Actions = new ObservableCollection<ButtonAction>()},
                        new HOTASMap(){ButtonName = "Button 5", Actions = new ObservableCollection<ButtonAction>()},
                        new HOTASMap(){ButtonName = "Button 6", Actions = new ObservableCollection<ButtonAction>()},
                        new HOTASMap(){ButtonName = "Button 7", Actions = new ObservableCollection<ButtonAction>()},
                        new HOTASMap(){ButtonName = "Button 8", Actions = new ObservableCollection<ButtonAction>()},
                        new HOTASMap(){ButtonName = "Button 9", Actions = new ObservableCollection<ButtonAction>()},
                        new HOTASMap(){ButtonName = "Button 10", Actions = new ObservableCollection<ButtonAction>()},
                        new HOTASMap(){ButtonName = "Button 11", Actions = new ObservableCollection<ButtonAction>()},
                        new HOTASMap(){ButtonName = "Button 12", Actions = new ObservableCollection<ButtonAction>()}                    }
                }
            };
            return devices;
        }
    }
}

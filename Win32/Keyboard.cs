using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SierraHOTAS
{
    //https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-keyboard-hook-in-c/
    public static class Keyboard
    {
        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] Win32Structures.INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        internal static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        internal static event EventHandler<KeystrokeEventArgs> KeyDownEvent;
        internal static event EventHandler<KeystrokeEventArgs> KeyUpEvent;

        internal const int WH_KEYBOARD_LL = 13;
        internal const int WM_KEYDOWN = 0x0100;
        internal const int WM_KEYUP = 0x0101;
        internal const int WM_SYSKEYDOWN = 0x0104;
        internal const int WM_SYSKEYUP = 0x0105;
        internal const int WM_PREVIOUS_KEYSTATE = 0x40000000;

        internal const uint MAPVK_VK_TO_VSC = 0x00;
        internal const uint MAPVK_VSC_TO_VK = 0x01;
        internal const uint MAPVK_VK_TO_CHAR = 0x02;
        internal const uint MAPVK_VSC_TO_VK_EX = 0x03;
        internal const uint MAPVK_VK_TO_VSC_EX = 0x04;

        internal static LowLevelKeyboardProc _proc = HookCallback;
        internal static IntPtr _hookID = IntPtr.Zero;

        public static bool IsKeySuppressionActive { get; set; }
        internal const int KeyDownInitialDelay = 350;
        internal const int KeyDownRepeatDelay = 35;

        internal class KeystrokeEventArgs : EventArgs
        {
            public int Code { get; set; }
            public int Flags { get; set; }

            public KeystrokeEventArgs(uint code, Win32Structures.KBDLLHOOKSTRUCTFlags flags)
            {
                Code = (int)code;
                Flags = (int)flags;
            }
        }

        private static Dictionary<Win32Structures.ScanCodeShort, string> _displayKeyNames;
        private static Dictionary<Win32Structures.ScanCodeShort, string> _displayKeyNamesExtended;
        static Keyboard()
        {
            PopulateKeyDisplayNamesDictionary();
            PopulateKeyDisplayNamesExtendedDictionary();
        }

        private static void PopulateKeyDisplayNamesExtendedDictionary()
        {
            _displayKeyNamesExtended = new Dictionary<Win32Structures.ScanCodeShort, string>
            {
                {Win32Structures.ScanCodeShort.NUMLOCK, "NUM LCK"},
                {Win32Structures.ScanCodeShort.MULTIPLY, "PRINT"},
                {Win32Structures.ScanCodeShort.APPS, "APPS"},
                {Win32Structures.ScanCodeShort.LWIN, "LWIN"},
                {Win32Structures.ScanCodeShort.RWIN, "RWIN"},
                {Win32Structures.ScanCodeShort.LMENU, "RALT"},
                {Win32Structures.ScanCodeShort.LCONTROL, "RCTRL"},
                {Win32Structures.ScanCodeShort.OEM_2, "NUM /"},
                {Win32Structures.ScanCodeShort.RSHIFT, "RSHIFT"},
                {Win32Structures.ScanCodeShort.HOME, "HOME"},
                {Win32Structures.ScanCodeShort.END, "END"},
                {Win32Structures.ScanCodeShort.INSERT, "INSERT"},
                {Win32Structures.ScanCodeShort.DELETE, "DEL"},
                {Win32Structures.ScanCodeShort.PRIOR, "PG UP"},
                {Win32Structures.ScanCodeShort.NEXT, "PG DN"},
                {Win32Structures.ScanCodeShort.UP, "UP"},
                {Win32Structures.ScanCodeShort.RIGHT, "RIGHT"},
                {Win32Structures.ScanCodeShort.DOWN, "DN"},
                {Win32Structures.ScanCodeShort.LEFT, "LEFT"},
                {Win32Structures.ScanCodeShort.RETURN, "NUM ENTR"}
            };
        }

        private static void PopulateKeyDisplayNamesDictionary()
        {
            _displayKeyNames = new Dictionary<Win32Structures.ScanCodeShort, string>();
            foreach (short code in Enum.GetValues(typeof(Win32Structures.ScanCodeShort)))
            {
                var scanCode = (Win32Structures.ScanCodeShort) code;
                var displayName = Enum.GetName(typeof(Win32Structures.ScanCodeShort), code);
                displayName = displayName.Replace("KEY_", "");
                displayName = displayName.Replace("LCONTROL", "LCTRL");
                switch (displayName)
                {
                    case "LMENU":
                        displayName = "LALT";
                        break;
                    case "OEM_1":
                        displayName = ";";
                        break;
                    case "OEM_2":
                        displayName = "/";
                        break;
                    case "OEM_3":
                        displayName = "`";
                        break;
                    case "OEM_4":
                        displayName = "[";
                        break;
                    case "OEM_5":
                        displayName = "\\";
                        break;
                    case "OEM_6":
                        displayName = "]";
                        break;
                    case "OEM_7":
                        displayName = "/";
                        break;
                    case "OEM_PERIOD":
                        displayName = ".";
                        break;
                    case "OEM_COMMA":
                        displayName = ",";
                        break;
                    case "OEM_PLUS":
                        displayName = "=";
                        break;
                    case "OEM_MINUS":
                        displayName = "-";
                        break;
                    //case "PRIOR":
                    //    displayName = "PG UP";
                    //    break;
                    case "RETURN":
                        displayName = "ENTER";
                        break;
                    //case "NEXT":
                    //    displayName = "PG DN";
                    //    break;
                    case "CAPITAL":
                        displayName = "CAPS";
                        break;
                    case "MULTIPLY":
                        displayName = "NUM *";
                        break;
                    case "ADD":
                        displayName = "NUM +";
                        break;
                    case "SUBTRACT":
                        displayName = "NUM -";
                        break;
                    case "MULTIPLE":
                        displayName = "PRINT";
                        break;
                    case "NUMLOCK":
                        displayName = "BREAK";
                        break;
                }

                if (_displayKeyNames.ContainsKey(scanCode))
                {
                    Debug.WriteLine("Wait");
                }

                _displayKeyNames.Add(scanCode, displayName);
            }
        }

        public static string GetKeyDisplayName(Win32Structures.ScanCodeShort scanCode, int flags)
        {
            if((flags & (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED) == (int) Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED)
            {
                _displayKeyNamesExtended.TryGetValue(scanCode, out var name);
                return name;
            }
            else
            {
                _displayKeyNames.TryGetValue(scanCode, out var name);
                return name;
            }
        }

        public static void Start()
        {
            _lstKeyDownBuffer = new List<uint>();
            _hookID = SetHook(_proc);
        }

        public static void Stop()
        {
            UnhookWindowsHookEx(_hookID);
            _lstKeyDownBuffer.Clear();
        }

        public static void SendKeyPress(int scanCode, int flags)
        {
            var pInputs = new[]
            {
                BuildKeyboardInput((Win32Structures.ScanCodeShort)scanCode, flags),
            };
            //Debug.WriteLine($"SendInput:{scanCode} - {flags}");
            SendInput((uint)pInputs.Length, pInputs, Win32Structures.INPUT.Size);
        }

        public static void SendKeyPress(Win32Structures.ScanCodeShort scanCode, int flags)
        {
            var pInputs = new Win32Structures.INPUT[]
            {
                BuildKeyboardInput(scanCode, flags),
            };
            SendInput((uint)pInputs.Length, pInputs, Win32Structures.INPUT.Size);
        }

        private static Win32Structures.INPUT BuildKeyboardInput(Win32Structures.ScanCodeShort scanCode, int flags)
        {
            Win32Structures.KEYEVENTF keyEventFlags = Win32Structures.KEYEVENTF.SCANCODE;

            if ((flags & (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED) == (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED)
                keyEventFlags |= Win32Structures.KEYEVENTF.EXTENDEDKEY;

            if ((flags & (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP) == (int)Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_UP)
                keyEventFlags |= Win32Structures.KEYEVENTF.KEYUP;

            return new Win32Structures.INPUT()
            {
                type = Win32Structures.INPUT_KEYBOARD,
                U = new Win32Structures.InputUnion()
                {
                    ki = new Win32Structures.KEYBDINPUT()
                    {
                        scanCode = scanCode,
                        vkCode = 0,
                        flags = keyEventFlags,
                        time = 0,
                        dwExtraInfo = (UIntPtr)0
                    }
                }
            };
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                if (curModule != null)
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
            return (IntPtr)0;
        }

        private static List<uint> _lstKeyDownBuffer;
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) return CallNextHookEx(_hookID, nCode, wParam, lParam);

            var key = Marshal.PtrToStructure<Win32Structures.KBDLLHOOKSTRUCT>(lParam);

            //Debug.WriteLine($"HookCallback:{key.scanCode} - {key.flags}");

            //bufKey will hold both the scan code and the extended keyboard flag
            var bufKey = key.scanCode << 8;
            var ext = (uint)(key.flags & Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED);
            bufKey |= ext;

            if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                //suppress key repeating events from firing the event
                if (!_lstKeyDownBuffer.Contains(bufKey))
                {
                    _lstKeyDownBuffer.Add(bufKey);
                    KeyDownEvent?.Invoke(null, new KeystrokeEventArgs(key.scanCode, key.flags));
                }
            }

            if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
            {
                if (_lstKeyDownBuffer.Contains(bufKey))
                {
                    _lstKeyDownBuffer.Remove(bufKey);
                    KeyUpEvent?.Invoke(null, new KeystrokeEventArgs(key.scanCode, key.flags));
                }
            }

            var callbackValue = 0;//process this keypress as normal
            if (IsKeySuppressionActive) callbackValue = 1;//don't continue handling this keystroke. prevents ALT from opening menue, alt-tab from switching screens, etc. Only do this when recording keypresses.
            return (IntPtr)callbackValue;
        }
    }
}

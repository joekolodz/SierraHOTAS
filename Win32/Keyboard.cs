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
        static Keyboard()
        {
            var displayName = "";
            _displayKeyNames = new Dictionary<Win32Structures.ScanCodeShort, string>();
            foreach (short code in Enum.GetValues(typeof(Win32Structures.ScanCodeShort)))
            {
                
                var scanCode = (Win32Structures.ScanCodeShort)code;
                displayName = Enum.GetName(typeof(Win32Structures.ScanCodeShort), code);
                displayName = displayName.Replace("KEY_", "");
                displayName = displayName.Replace("CONTROL", "CTRL");
                switch (displayName)
                {
                    case "LMENU":
                        displayName = "LALT";
                        break;
                    case "OEM1":
                        displayName = ";";
                        break;
                    case "OEM2":
                        displayName = "/";
                        break;
                    case "OEMPERIOD":
                        displayName = ".";
                        break;
                    case "OEMCOMMA":
                        displayName = ",";
                        break;
                    case "PRIOR":
                        displayName = "PG UP";
                        break;
                    case "NEXT":
                        displayName = "PG DN";
                        break;
                    case "OEMPLUS":
                        displayName = "+";
                        break;
                    case "OEMMINUS":
                        displayName = "-";
                        break;
                }
                if (_displayKeyNames.ContainsKey(scanCode))
                {
                    Debug.WriteLine("Wait");
                }
                _displayKeyNames.Add(scanCode, displayName);
            }
        }
        public static string GetKeyDisplayName(Win32Structures.ScanCodeShort scanCode)
        {
            _displayKeyNames.TryGetValue(scanCode, out var name);
            return name;
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

            var bufKey = key.scanCode << 8;
            var ext = (uint)(key.flags & Win32Structures.KBDLLHOOKSTRUCTFlags.LLKHF_EXTENDED);
            bufKey |= ext;

            if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            {
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
                    var result = _lstKeyDownBuffer.Remove(bufKey);
                    KeyUpEvent?.Invoke(null, new KeystrokeEventArgs(key.scanCode, key.flags));
                }
            }

            var callbackValue = 0;//process this keypress as normal
            if (IsKeySuppressionActive) callbackValue = 1;//don't continue handling this keystroke. prevents ALT from opening menue, alt-tab from switching screens, etc. Only do this when recording keypresses.
            return (IntPtr)callbackValue;
        }
    }
}

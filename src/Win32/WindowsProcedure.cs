using System;
using System.Windows;
using System.Windows.Interop;

namespace SierraHOTAS.Win32
{
    public class WindowsProcedure
    {
        //https://stackoverflow.com/questions/624367/how-to-handle-wndproc-messages-in-wpf
        public static void Initialize(Views.MainWindow main)
        {
            if(!(PresentationSource.FromVisual(main) is HwndSource source))
                throw new ArgumentNullException(nameof(main), @"Can't found a handle to the main window. Application stopping.");

            source.AddHook(WndProc);
        }
        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //if (msg == Keyboard.WM_KEYDOWN)
            //{
            //    Debug.WriteLine("Close it down!");
            //}
            // Handle messages...
            //Debug.WriteLine($"WndProc: {DateTime.Now}");
            return IntPtr.Zero;
        }

    }
}

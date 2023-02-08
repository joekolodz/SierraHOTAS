using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace SierraHOTAS.Views
{
    /// <summary>
    /// Interaction logic for ModeOverlayWindow.xaml
    /// </summary>
    public partial class ModeOverlayWindow : Window
    {
        public const string WINDOW_NAME = "ModeOverlay";
        private readonly Action<EventHandler<ModeChangedEventArgs>> _removeModeChangedHandler;
        private readonly Dictionary<int, ModeActivationItem> _modeDictionary;
        private bool _isMouseDown = false;
        private Point _mouseOffset;
        private readonly IFileSystem _fileSystem;
        private string _settingFileName;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

        public ModeOverlayWindow(IFileSystem fileSystem, Dictionary<int, ModeActivationItem> modeDictionary, int mode, Action<EventHandler<ModeChangedEventArgs>> modeChangedHandler, Action<EventHandler<ModeChangedEventArgs>> removeModeChangedHandler)
        {
            InitializeComponent();

            _removeModeChangedHandler = removeModeChangedHandler;
            modeChangedHandler(ModeChangedHandler);
            _modeDictionary = modeDictionary;
            _fileSystem = fileSystem;

            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            Topmost = true;
            ShowInTaskbar = false;

            ReadSettings();

            var b = new SolidColorBrush(Colors.Red);
            b.Opacity = 0.0;
            Background = b;

            SetModeName(mode);

            StateChanged += ModeOverlayWindow_StateChanged;
            Closed += ModeOverlayWindow_Closed;
            MouseDown += ModeOverlayWindow_MouseDown;
            MouseUp += ModeOverlayWindow_MouseUp;
            MouseMove += ModeOverlayWindow_MouseMove;
        }

        private void SetSettingFileName()
        {
            var assembly = Assembly.GetEntryAssembly();
            var path = Path.Combine(Path.GetDirectoryName(assembly.Location), assembly.GetName().Name);
            _settingFileName = $"{path}.userSettings";
        }

        private void ReadSettings()
        {
            SetSettingFileName();

            Top = 0;
            Left = 0;

            var settings = _fileSystem.ReadModeOverlayScreenPosition(_settingFileName);

            if (string.IsNullOrWhiteSpace(settings)) return;
            var list = settings.Split(',');

            if (list.Length != 2) return;

            int.TryParse(list[0], out var x);
            int.TryParse(list[1], out var y);

            Top = y;
            Left = x;
        }

        private void ModeOverlayWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isMouseDown) return;

            var mouseLocation = PointToScreen(e.GetPosition(this));
            var x = mouseLocation.X - _mouseOffset.X;
            var y = mouseLocation.Y - _mouseOffset.Y;
            var helper = new WindowInteropHelper(this);
            MoveWindow(helper.Handle, (int)x, (int)y, (int)Width, (int)Height, true);
        }

        private void ModeOverlayWindow_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            _isMouseDown = false;
            _fileSystem.SaveModeOverlayScreenPosition(_settingFileName, (int)Left, (int)Top);
        }

        private void ModeOverlayWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CaptureMouse();
            _mouseOffset = e.GetPosition(this);
            _isMouseDown = true;
        }

        private void ModeOverlayWindow_Closed(object sender, EventArgs e)
        {
            _removeModeChangedHandler(ModeChangedHandler);
        }

        private void ModeChangedHandler(object sender, ModeChangedEventArgs e)
        {
            SetModeName(e.Mode);
        }

        private void ModeOverlayWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void SetModeName(int mode)
        {
            if (_modeDictionary.ContainsKey(mode))
            {
                txtMessage.Text = _modeDictionary[mode].ModeName;
                SetWidthFromContent();
            }
        }
        private void SetWidthFromContent()
        {
            var face = new Typeface(new FontFamily("Segoe UI"),
                                    new FontStyle(),
                                    FontWeights.Bold,
                                    FontStretches.Normal);

            var formattedText = new FormattedText(txtMessage.Text,
                                                  CultureInfo.GetCultureInfo("en-us"),
                                                  FlowDirection.LeftToRight,
                                                  face,
                                                  15,
                                                  Brushes.Gold,
                                                  1.25);

            Width = (int)formattedText.Width + 110;
        }
    }
}

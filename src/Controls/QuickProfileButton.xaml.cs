using System;
using System.Windows;
using System.Windows.Controls;

namespace SierraHOTAS.Controls
{
    public partial class QuickProfileButton : UserControl
    {
        private const string FILE_TOOL_TIP = "Click to select a profile to quick load";
        public event EventHandler<EventArgs> QuickLoadButtonClicked;
        public event EventHandler<EventArgs> QuickLoadButtonCleared;

        public static DependencyProperty ProfileIdProperty = DependencyProperty.Register(nameof(ProfileId), typeof(int), typeof(QuickProfileButton), new PropertyMetadata(0));
        public static DependencyProperty FileNameProperty = DependencyProperty.Register(nameof(FileName), typeof(string), typeof(QuickProfileButton));
        public static DependencyProperty NickNameProperty = DependencyProperty.Register(nameof(NickName), typeof(string), typeof(QuickProfileButton));
        public static DependencyProperty FileToolTipProperty = DependencyProperty.Register(nameof(FileToolTip), typeof(string), typeof(QuickProfileButton), new PropertyMetadata(FILE_TOOL_TIP));
        public static DependencyProperty IsQuickProfileSetProperty = DependencyProperty.Register(nameof(IsQuickProfileSet), typeof(bool), typeof(QuickProfileButton), new PropertyMetadata(false));

        public int ProfileId
        {
            get => (int)GetValue(ProfileIdProperty);
            set => SetValue(ProfileIdProperty, value);
        }

        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set
            {
                SetValue(FileNameProperty, value);
                IsQuickProfileSet = !string.IsNullOrWhiteSpace(FileName);
            }
        }

        public string FileToolTip
        {
            get => (string)GetValue(FileToolTipProperty);
            set => SetValue(FileToolTipProperty, value);
        }

        public string NickName
        {
            get => (string)GetValue(NickNameProperty);
            set => SetValue(NickNameProperty, value);
        }

        public bool IsQuickProfileSet
        {
            get => (bool)GetValue(IsQuickProfileSetProperty);
            set => SetValue(IsQuickProfileSetProperty, value);
        }

        public QuickProfileButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void Reset()
        {
            FileName = "";
            NickName = "";
            FileToolTip = FILE_TOOL_TIP;
        }

        public void SetFileName(string fileName)
        {
            FileName = fileName;
            NickName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            FileToolTip = fileName;
        }

        private void QuickLoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            QuickLoadButtonClicked?.Invoke(this, new EventArgs());
        }

        private void ClearQuickLoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            QuickLoadButtonCleared?.Invoke(this, new EventArgs());
        }
    }
}

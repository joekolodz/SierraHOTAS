using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using SierraHOTAS.Annotations;

namespace SierraHOTAS.Controls
{
    public partial class QuickProfileButton : UserControl, INotifyPropertyChanged
    {
        private const string FILE_TOOL_TIP = "Click to select a profile to quick load";
        public event EventHandler<EventArgs> QuickLoadButtonClicked;
        public event EventHandler<EventArgs> QuickLoadButtonCleared;

        public static DependencyProperty ProfileIdProperty = DependencyProperty.Register(nameof(ProfileId), typeof(int), typeof(QuickProfileButton), new PropertyMetadata(0));
        public static DependencyProperty FileNameProperty = DependencyProperty.Register(nameof(FileName), typeof(string), typeof(QuickProfileButton), new FrameworkPropertyMetadata(FILE_TOOL_TIP, OnFileNameChanged));
        public static DependencyProperty NickNameProperty = DependencyProperty.Register(nameof(NickName), typeof(string), typeof(QuickProfileButton));
        public static DependencyProperty FileToolTipProperty = DependencyProperty.Register(nameof(FileToolTip), typeof(string), typeof(QuickProfileButton), new PropertyMetadata(FILE_TOOL_TIP));

        private static void OnFileNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is QuickProfileButton prop)) return;
            prop.OnPropertyChanged(nameof(IsQuickProfileSet));
        }

        public static DependencyProperty IsQuickProfileSetProperty = DependencyProperty.Register(nameof(IsQuickProfileSet), typeof(bool), typeof(QuickProfileButton), new PropertyMetadata(false));
        public static DependencyProperty HideClearButtonProperty = DependencyProperty.Register(nameof(HideClearButton), typeof(bool), typeof(QuickProfileButton), new PropertyMetadata(false));

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

        public bool HideClearButton
        {
            get => (bool)GetValue(HideClearButtonProperty);
            set
            {
                SetValue(HideClearButtonProperty, value);
                ClearButton.Visibility = HideClearButton ? Visibility.Collapsed:Visibility.Visible;
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

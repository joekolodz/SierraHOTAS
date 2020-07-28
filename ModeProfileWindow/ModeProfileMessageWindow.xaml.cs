using System.Windows;

namespace SierraHOTAS.ModeProfileWindow
{
    /// <summary>
    /// Interaction logic for ModeProfileMessageWindow.xaml
    /// </summary>
    public partial class ModeProfileMessageWindow : Window
    {
        public ModeProfileMessageWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

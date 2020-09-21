using System.Windows;
using SierraHOTAS.Services;

namespace SierraHOTAS.ModeProfileWindow
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window, IDialogWindow
    {
        public MessageWindow(string message)
        {
            InitializeComponent();
            txtMessage.Text = message;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

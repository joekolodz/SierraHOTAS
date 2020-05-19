using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SierraHOTAS.Models;
using SierraHOTAS.ModeProfileWindow.ViewModels;

namespace SierraHOTAS.ModeProfileWindow
{
    /// <summary>
    /// Interaction logic for NewModeProfileWindow.xaml
    /// </summary>
    public partial class NewModeProfileWindow : Window
    {
        public NewModeProfileWindowViewModel ModeProfileViewModel { get; }
        public NewModeProfileWindow(int mode, Dictionary<int, (Guid, int)> activationButtonList, HOTASCollection devices)
        {
            InitializeComponent();
            ModeProfileViewModel = new NewModeProfileWindowViewModel(mode, activationButtonList, devices);
            ModeProfileViewModel.AppDispatcher = Dispatcher;
            ModeProfileViewModel.NewModeSavedEventHandler += NewModeSaved;
            DataContext = ModeProfileViewModel;

            Closed += NewModeProfileWindow_Closed;
        }

        private void NewModeSaved(object sender, EventArgs e)
        {
            Close();
        }

        private void NewModeProfileWindow_Closed(object sender, EventArgs e)
        {
            ModeProfileViewModel.AppDispatcher = null;
            ModeProfileViewModel.NewModeSavedEventHandler -= NewModeSaved;
        }
    }
}

using SierraHOTAS.ModeProfileWindow.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;

namespace SierraHOTAS.ModeProfileWindow
{
    /// <summary>
    /// Interaction logic for ModeProfileConfigWindow.xaml
    /// </summary>
    public partial class ModeProfileConfigWindow : Window
    {
        public ModeProfileConfigWindowViewModel ModeProfileConfigViewModel { get; }
        public ModeProfileConfigWindow(int mode, Dictionary<int, ModeActivationItem> activationButtonList)
        {
            InitializeComponent();
            ModeProfileConfigViewModel = new ModeProfileConfigWindowViewModel(mode, activationButtonList);
            ModeProfileConfigViewModel.AppDispatcher = Dispatcher;
            ModeProfileConfigViewModel.NewModeSavedEventHandler += NewModeSaved;
            DataContext = ModeProfileConfigViewModel;

            Closed += ModeProfileConfigWindow_Closed;
        }

        private void NewModeSaved(object sender, EventArgs e)
        {
            Close();
        }

        private void ModeProfileConfigWindow_Closed(object sender, EventArgs e)
        {
            ModeProfileConfigViewModel.AppDispatcher = null;
            ModeProfileConfigViewModel.NewModeSavedEventHandler -= NewModeSaved;
        }
    }
}

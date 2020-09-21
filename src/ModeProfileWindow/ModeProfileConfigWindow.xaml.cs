using SierraHOTAS.ModeProfileWindow.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

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
            ModeProfileConfigViewModel.SaveCancelled += SaveCancelled;
            ModeProfileConfigViewModel.NewProfileSaved += NewProfileSaved;
            DataContext = ModeProfileConfigViewModel;

            Closed += ModeProfileConfigWindow_Closed;
        }

        private void NewProfileSaved(object sender, EventArgs e)
        {
            DialogResult = true;
            CloseInternal();
        }

        private void SaveCancelled(object sender, EventArgs e)
        {
            DialogResult = false;
            CloseInternal();
        }

        private void ModeProfileConfigWindow_Closed(object sender, EventArgs e)
        {
            ModeProfileConfigViewModel.AppDispatcher = null;
            ModeProfileConfigViewModel.SaveCancelled -= SaveCancelled;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape) SaveCancelled(null, null);
        }

        private void CloseInternal()
        {
            Close();
        }
    }
}

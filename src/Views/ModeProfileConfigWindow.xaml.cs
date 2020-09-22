using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using SierraHOTAS.Models;
using SierraHOTAS.ModeProfileWindow.ViewModels;
using SierraHOTAS.ViewModels;

namespace SierraHOTAS.Views
{
    /// <summary>
    /// Interaction logic for ModeProfileConfigWindow.xaml
    /// </summary>
    public partial class ModeProfileConfigWindow : Window
    {
        public ModeProfileConfigWindowViewModel ModeProfileConfigViewModel { get; }

        private Action _cancelCallback;

        public ModeProfileConfigWindow(int mode, Dictionary<int, ModeActivationItem> activationButtonList, Action<EventHandler<ButtonPressedEventArgs>> pressedHandler, Action cancelCallback)
        {
            InitializeComponent();
            ModeProfileConfigViewModel = new ModeProfileConfigWindowViewModel(mode, activationButtonList);
            ModeProfileConfigViewModel.AppDispatcher = Dispatcher;
            ModeProfileConfigViewModel.SaveCancelled += SaveCancelled;
            ModeProfileConfigViewModel.NewProfileSaved += NewProfileSaved;
            DataContext = ModeProfileConfigViewModel;
            pressedHandler(PressedHandler);
            _cancelCallback = cancelCallback;
            Closed += ModeProfileConfigWindow_Closed;
        }

        private void PressedHandler(object sender, ButtonPressedEventArgs e)
        {
            ModeProfileConfigViewModel.DeviceList_ButtonPressed(sender, e);
        }

        private void NewProfileSaved(object sender, EventArgs e)
        {
            DialogResult = true;
            CloseInternal();
        }

        private void SaveCancelled(object sender, EventArgs e)
        {
            DialogResult = false;
            _cancelCallback.Invoke();
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

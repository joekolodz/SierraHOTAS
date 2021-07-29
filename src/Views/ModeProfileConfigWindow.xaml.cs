using SierraHOTAS.Models;
using SierraHOTAS.ModeProfileWindow.ViewModels;
using SierraHOTAS.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SierraHOTAS.Views
{
    /// <summary>
    /// Interaction logic for ModeProfileConfigWindow.xaml
    /// </summary>
    public partial class ModeProfileConfigWindow : Window
    {
        public ModeProfileConfigWindowViewModel ModeProfileConfigViewModel { get; }

        private readonly Action _cancelCallback;
        private readonly Action<EventHandler<ButtonPressedEventArgs>> _removePressedHandler;

        public ModeProfileConfigWindow(IEventAggregator eventAggregator, IDispatcher appDispatcher, int mode, Dictionary<int, ModeActivationItem> activationButtonList, Action<EventHandler<ButtonPressedEventArgs>> pressedHandler, Action<EventHandler<ButtonPressedEventArgs>> removePressedHandler, Action cancelCallback)
        {
            InitializeComponent();
            ModeProfileConfigViewModel = new ModeProfileConfigWindowViewModel(eventAggregator, appDispatcher, mode, activationButtonList);
            ModeProfileConfigViewModel.SaveCancelled += SaveCancelled;
            ModeProfileConfigViewModel.NewModeProfileSaved += NewModeProfileSaved;
            Closed += OnClosed;
            DataContext = ModeProfileConfigViewModel;
            pressedHandler(PressedHandler);
            _removePressedHandler = removePressedHandler;
            _cancelCallback = cancelCallback;
        }

        private void PressedHandler(object sender, ButtonPressedEventArgs e)
        {
            ModeProfileConfigViewModel.DeviceList_ButtonPressed(sender, e);
        }

        private void NewModeProfileSaved(object sender, EventArgs e)
        {
            DialogResult = true;
        }

        private void SaveCancelled(object sender, EventArgs e)
        {
            _cancelCallback.Invoke();
            DialogResult = false;
        }

        private void RemoveHandlers()
        {
            Closed -= SaveCancelled;
            ModeProfileConfigViewModel.SaveCancelled -= SaveCancelled;
            ModeProfileConfigViewModel.NewModeProfileSaved -= NewModeProfileSaved;
            _removePressedHandler(PressedHandler);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape) SaveCancelled(null, null);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            RemoveHandlers();
            Close();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModeProfileConfigViewModel.TemplateModeSelected(sender, e);
        }
    }
}

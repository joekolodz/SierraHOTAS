using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SierraHOTAS.Views
{
    /// <summary>
    /// Interaction logic for ModeConfigWindow.xaml
    /// </summary>
    public partial class ModeConfigWindow : Window
    {
        public ModeConfigWindowViewModel ModeConfigViewModel { get; }

        private readonly Action _cancelCallback;
        private readonly Action<EventHandler<ButtonPressedEventArgs>> _removePressedHandler;

        public ModeConfigWindow(IEventAggregator eventAggregator, IDispatcher appDispatcher, int mode, Dictionary<int, ModeActivationItem> activationButtonList, Action<EventHandler<ButtonPressedEventArgs>> pressedHandler, Action<EventHandler<ButtonPressedEventArgs>> removePressedHandler, Action cancelCallback)
        {
            InitializeComponent();
            ModeConfigViewModel = new ModeConfigWindowViewModel(eventAggregator, appDispatcher, mode, activationButtonList);
            ModeConfigViewModel.SaveCancelled += SaveCancelled;
            ModeConfigViewModel.NewModeSaved += newModeSaved;
            Closed += OnClosed;
            DataContext = ModeConfigViewModel;
            pressedHandler(PressedHandler);
            _removePressedHandler = removePressedHandler;
            _cancelCallback = cancelCallback;
        }

        private void PressedHandler(object sender, ButtonPressedEventArgs e)
        {
            ModeConfigViewModel.DeviceList_ButtonPressed(sender, e);
        }

        private void newModeSaved(object sender, EventArgs e)
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
            ModeConfigViewModel.SaveCancelled -= SaveCancelled;
            ModeConfigViewModel.NewModeSaved -= newModeSaved;
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
    }
}

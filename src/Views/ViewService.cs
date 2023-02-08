using System.Linq;
using SierraHOTAS.ViewModels;
using System.Windows;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;

namespace SierraHOTAS.Views
{
    public class ViewService
    {
        private readonly Window _mainWindow;
        private IEventAggregator _eventAggregator;
        private readonly IDispatcher _appDispatcher;
        private readonly IFileSystem _fileSystem;


        public ViewService(Window mainWindow, IEventAggregator eventAggregator, DispatcherFactory dispatcherFactory, IFileSystem fileSystem)
        {
            _mainWindow = mainWindow;
            _eventAggregator = eventAggregator;
            _appDispatcher = dispatcherFactory.CreateDispatcher();
            _fileSystem = fileSystem;

            _eventAggregator.Subscribe<ShowMessageWindowEvent>(ShowMessageWindow);
            _eventAggregator.Subscribe<ShowModeConfigWindowEvent>(ShowModeConfigWindow);
            _eventAggregator.Subscribe<ShowInputGraphWindowEvent>(ShowInputGraphWindow);
            _eventAggregator.Subscribe<ShowModeOverlayWindowEvent>(ShowModeOverlayWindow);
        }

        private void ShowMessageWindow(ShowMessageWindowEvent eventMessage)
        {
            var modeMessageWindow = new MessageWindow(eventMessage.Message) { Owner = _mainWindow };
            modeMessageWindow.ShowDialog();
        }

        private void ShowModeConfigWindow(ShowModeConfigWindowEvent eventMessage)
        {
            var modeMessageWindow = new ModeConfigWindow(_eventAggregator, _appDispatcher, eventMessage.Mode, eventMessage.ActivationButtonList, eventMessage.PressedHandler, eventMessage.RemovePressedHandler, eventMessage.CancelCallback);
            modeMessageWindow.Owner = _mainWindow;

            var result = modeMessageWindow.ShowDialog();
            if (result.HasValue && result.Value == false)
            {
                eventMessage.CancelCallback.Invoke();
            }
        }

        private void ShowInputGraphWindow(ShowInputGraphWindowEvent eventMessage)
        {
            InputGraphWindow.CreateWindow(_mainWindow, eventMessage.DeviceList, eventMessage.AxisChangedHandler, eventMessage.CancelCallbackRemoveHandler);
        }

        private void ShowModeOverlayWindow(ShowModeOverlayWindowEvent eventMessage)
        {
            var existingWindow = Application.Current.Windows.Cast<Window>().SingleOrDefault(w => w.Name == ModeOverlayWindow.WINDOW_NAME);

            if (existingWindow == null)
            {
                var profile = new ModeOverlayWindow(_fileSystem, eventMessage.ModeDictionary, eventMessage.Mode, eventMessage.ModeChangedHandler, eventMessage.RemoveModeChangedHandler);
                profile.Name = ModeOverlayWindow.WINDOW_NAME;
                profile.Show();
            }
            else
            {
                if (existingWindow.IsVisible)
                {
                    existingWindow.Hide();
                }
                else
                {
                    existingWindow.Show();
                }
            }
            _mainWindow.Focus();
        }
    }
}

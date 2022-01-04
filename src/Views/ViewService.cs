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


        public ViewService(Window mainWindow, IEventAggregator eventAggregator, DispatcherFactory dispatcherFactory)
        {
            _mainWindow = mainWindow;
            _eventAggregator = eventAggregator;
            _appDispatcher = dispatcherFactory.CreateDispatcher();

            _eventAggregator.Subscribe<ShowMessageWindowEvent>(ShowMessageWindow);
            _eventAggregator.Subscribe<ShowModeProfileConfigWindowEvent>(ShowModeProfileConfigWindow);
            _eventAggregator.Subscribe<ShowInputGraphWindowEvent>(ShowInputGraphWindow);
        }

        private void ShowMessageWindow(ShowMessageWindowEvent eventMessage)
        {
            var modeMessageWindow = new MessageWindow(eventMessage.Message) { Owner = _mainWindow };
            modeMessageWindow.ShowDialog();
        }

        private void ShowModeProfileConfigWindow(ShowModeProfileConfigWindowEvent eventMessage)
        {
            var modeMessageWindow = new ModeProfileConfigWindow(_eventAggregator, _appDispatcher, eventMessage.Mode, eventMessage.ActivationButtonList, eventMessage.PressedHandler, eventMessage.RemovePressedHandler, eventMessage.CancelCallback);
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
    }
}

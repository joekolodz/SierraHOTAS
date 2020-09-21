using SierraHOTAS.ViewModels;
using System.Windows;

namespace SierraHOTAS.Views
{
    public class ViewService
    {
        private Window _mainWindow;
        private IEventAggregator _eventAggregator;
        public ViewService(Window mainWindow, IEventAggregator eventAggregator)
        {
            _mainWindow = mainWindow;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe<ShowMessageWindowEvent>(ShowMessageWindow);
        }

        private void ShowMessageWindow(ShowMessageWindowEvent eventMessage)
        {
            var modeMessageWindow = new MessageWindow(eventMessage.Message) { Owner = _mainWindow };
            modeMessageWindow.ShowDialog();
        }
    }
}

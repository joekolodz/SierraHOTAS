using SierraHOTAS.ViewModels;
using System.Windows;

namespace SierraHOTAS.Views
{
    public class ViewService
    {
        private readonly Window _mainWindow;

        public ViewService(Window mainWindow, IEventAggregator eventAggregator)
        {
            _mainWindow = mainWindow;
            eventAggregator.Subscribe<ShowMessageWindowEvent>(ShowMessageWindow);
            eventAggregator.Subscribe<ShowModeProfileConfigWindowEvent>(ShowModeProfileConfigWindow);
            eventAggregator.Subscribe<ShowInputGraphWindowEvent>(ShowInputGraphWindow);
        }

        private void ShowMessageWindow(ShowMessageWindowEvent eventMessage)
        {
            var modeMessageWindow = new MessageWindow(eventMessage.Message) { Owner = _mainWindow };
            modeMessageWindow.ShowDialog();
        }

        private void ShowModeProfileConfigWindow(ShowModeProfileConfigWindowEvent eventMessage)
        {
            var modeMessageWindow = new ModeProfileConfigWindow(eventMessage.Mode, eventMessage.ActivationButtonList, eventMessage.PressedHandler, eventMessage.CancelCallback);
            modeMessageWindow.Owner = _mainWindow;
            
            var result = modeMessageWindow.ShowDialog();
            if (result.HasValue && result.Value == false)
            {
                eventMessage.CancelCallback.Invoke();
            }
        }

        private void ShowInputGraphWindow(ShowInputGraphWindowEvent eventMessage)
        {
            new InputGraphWindow(eventMessage.AxisChangedHandler, eventMessage.CancelCallbackRemoveHandler) { Owner = _mainWindow }.Show();
        }
    }
}

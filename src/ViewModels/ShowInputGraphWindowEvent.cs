using System;
using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModels
{
    public class ShowInputGraphWindowEvent
    {
        public Action<EventHandler<AxisChangedEventArgs>> AxisChangedHandler { get; set; }
        public Action<EventHandler<AxisChangedEventArgs>> CancelCallbackRemoveHandler { get; set; }
        public ShowInputGraphWindowEvent(Action<EventHandler<AxisChangedEventArgs>> axisChangedHandler, Action<EventHandler<AxisChangedEventArgs>> cancelCallbackRemoveHandler)
        {
            AxisChangedHandler = axisChangedHandler;
            CancelCallbackRemoveHandler = cancelCallbackRemoveHandler;
        }
    }
}

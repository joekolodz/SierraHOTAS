using System;
using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModels
{
    public class ShowInputGraphWindowEvent
    {
        public IHOTASCollection DeviceList { get; set; }
        public Action<EventHandler<AxisChangedEventArgs>> AxisChangedHandler { get; set; }
        public Action<EventHandler<AxisChangedEventArgs>> CancelCallbackRemoveHandler { get; set; }
        public ShowInputGraphWindowEvent(IHOTASCollection deviceList, Action<EventHandler<AxisChangedEventArgs>> axisChangedHandler, Action<EventHandler<AxisChangedEventArgs>> cancelCallbackRemoveHandler)
        {
            DeviceList = deviceList;
            AxisChangedHandler = axisChangedHandler;
            CancelCallbackRemoveHandler = cancelCallbackRemoveHandler;
        }
    }
}

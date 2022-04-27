using SierraHOTAS.Models;
using System;
using System.Collections.Generic;

namespace SierraHOTAS.ViewModels
{
    public class ShowModeConfigWindowEvent
    {
        public int Mode { get; set; }
        public Dictionary<int, ModeActivationItem> ActivationButtonList { get; set; }
        public Action<EventHandler<ButtonPressedEventArgs>> PressedHandler { get; set; }
        public Action<EventHandler<ButtonPressedEventArgs>> RemovePressedHandler { get; set; }
        public Action CancelCallback { get; set; }

        public ShowModeConfigWindowEvent(int mode, Dictionary<int, ModeActivationItem> activationButtonList, Action<EventHandler<ButtonPressedEventArgs>> pressedHandler, Action<EventHandler<ButtonPressedEventArgs>> removePressedHandler, Action cancelCallback)
        {
            Mode = mode;
            ActivationButtonList = activationButtonList;
            PressedHandler = pressedHandler;
            RemovePressedHandler = removePressedHandler;
            CancelCallback = cancelCallback;
        }
    }
}

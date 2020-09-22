using SierraHOTAS.Models;
using SierraHOTAS.ModeProfileWindow.ViewModels;
using System;
using System.Collections.Generic;

namespace SierraHOTAS.ViewModels
{
    public class ShowModeProfileConfigWindowEvent
    {
        public int Mode { get; set; }
        public Dictionary<int, ModeActivationItem> ActivationButtonList { get; set; }
        public Action<EventHandler<ButtonPressedEventArgs>> PressedHandler { get; set; }
        public Action CancelCallback { get; set; }

        public ShowModeProfileConfigWindowEvent(int mode, Dictionary<int, ModeActivationItem> activationButtonList, Action<EventHandler<ButtonPressedEventArgs>> pressedHandler, Action cancelCallback)
        {
            Mode = mode;
            ActivationButtonList = activationButtonList;
            PressedHandler = pressedHandler;
            CancelCallback = cancelCallback;
        }
    }
}

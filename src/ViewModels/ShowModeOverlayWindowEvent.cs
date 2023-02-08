using System;
using System.Collections.Generic;
using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModels
{
    public class ShowModeOverlayWindowEvent
    {
        public Action<EventHandler<ModeChangedEventArgs>> ModeChangedHandler { get; set; }
        public Action<EventHandler<ModeChangedEventArgs>> RemoveModeChangedHandler { get; set; }
        public Dictionary<int, ModeActivationItem> ModeDictionary { get; set; }
        public int Mode{ get; set; }

        public ShowModeOverlayWindowEvent(Dictionary<int, ModeActivationItem> modeDictionary, int mode, Action<EventHandler<ModeChangedEventArgs>> modeChangedHandler, Action<EventHandler<ModeChangedEventArgs>> removeModeChangedHandler)
        {
            ModeDictionary = modeDictionary;
            Mode = mode;
            ModeChangedHandler = modeChangedHandler;
            RemoveModeChangedHandler = removeModeChangedHandler;
        }
    }
}

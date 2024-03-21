using System;

namespace SierraHOTAS.Models
{
    public class ActionCatalogItemRemovedRequestedEventArgs : EventArgs
    {
        public ActionCatalogItem ActionCatalogItem { get; set; }
        public ActionCatalogItemRemovedRequestedEventArgs(ActionCatalogItem item)
        {
            ActionCatalogItem = item;
        }
    }
}

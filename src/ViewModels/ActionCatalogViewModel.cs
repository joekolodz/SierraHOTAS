using SierraHOTAS.ViewModels;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModels
{
    public class ActionCatalogViewModel
    {
        public ObservableCollection<ActionCatalogItem> Catalog { get; }

        private const string NO_ACTION_TEXT = "<No Action>";

        public ActionCatalogViewModel()
        {
            Catalog = new ObservableCollection<ActionCatalogItem>();
            AddEmptyItem();
        }

        public void Clear()
        {
            Catalog.Clear();
            AddEmptyItem();
        }

        public bool Contains(string actionName)
        {
            return Catalog.Any(i => i.ActionName == actionName);
        }

        public void Add(ActionCatalogItem item)
        {
            if (Catalog.Contains(item)) return;

            if (Contains(item.ActionName))
            {
                var i = Catalog.First(x => x.ActionName == item.ActionName);
                Catalog.Remove(i);
            }
            Catalog.Add(item);
        }

        public void Add(ButtonMapViewModel buttonMap)
        {
            var item = buttonMap.ActionItem;
            if (Catalog.Contains(item)) return;

            Logging.Log.Info("Add to actions catalog");
            if (string.IsNullOrWhiteSpace(item.ActionName) || item.ActionName == NO_ACTION_TEXT)
            {
                item.ActionName = $"Action for {buttonMap.ButtonName}";
            }

            Catalog.Add(item);
            Logging.Log.Info($"{item.ActionName} - {buttonMap.ButtonName} added to actions catalog");
        }

        private void AddEmptyItem()
        {
            Add(new ActionCatalogItem() { NoAction = true, ActionName = NO_ACTION_TEXT, Actions = new ObservableCollection<ButtonAction>() });
        }
    }
}

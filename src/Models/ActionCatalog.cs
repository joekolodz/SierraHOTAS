using System.Collections.ObjectModel;
using System.Linq;

namespace SierraHOTAS.Models
{
    public class ActionCatalog
    {
        public ObservableCollection<ActionCatalogItem> Catalog { get; }

        private const string NO_ACTION_TEXT = "<No Action>";

        public ActionCatalog()
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

        public void Add(ActionCatalogItem item, string buttonName)
        {
            if (Catalog.Contains(item)) return;

            if (string.IsNullOrWhiteSpace(item.ActionName) || item.ActionName == NO_ACTION_TEXT)
            {
                item.ActionName = $"Action for {buttonName}";
            }

            Catalog.Add(item);
            Logging.Log.Debug($"{item.ActionName} - {buttonName} added to actions catalog");
        }

        private void AddEmptyItem()
        {
            Add(new ActionCatalogItem() { NoAction = true, ActionName = NO_ACTION_TEXT, Actions = new ObservableCollection<ButtonAction>() });
        }
    }
}

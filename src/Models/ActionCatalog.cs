using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SierraHOTAS.Models
{
    public class ActionCatalog
    {
        public ObservableCollection<ActionCatalogItem> Catalog { get; }

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

        public ActionCatalogItem Get(Guid id)
        {
            return Catalog.FirstOrDefault(i => i.Id == id);
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

            if (string.IsNullOrWhiteSpace(item.ActionName) || item.Id == Guid.Empty)
            {
                item.ActionName = $"Action for {buttonName}";
            }

            Catalog.Add(item);
            Logging.Log.Debug($"{item.ActionName} - {buttonName} added to actions catalog");
        }

        public ActionCatalogItem Get(string actionName)
        {
            if (!Contains(actionName)) return null;
            return Catalog.First(x => x.ActionName == actionName);
        }

        private void AddEmptyItem()
        {
            Add(ActionCatalogItem.EmptyItem());
        }
    }
}

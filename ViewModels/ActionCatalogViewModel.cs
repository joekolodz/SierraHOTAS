using SierraHOTAS.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModels
{
    public class ActionCatalogViewModel
    {
        public ObservableCollection<ActionCatalogItem> Catalog { get; }

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

        public ActionCatalogItem Add(ActionCatalogItem item)
        {
            Catalog.Add(item);
            return item;
        }

        public ActionCatalogItem Add(ButtonMapViewModel buttonMap)
        {
            var item = buttonMap.ActionItem;
            Logging.Log.Info("Add to actions catalog");
            if (string.IsNullOrWhiteSpace(item.ActionName))
            {
                item.ActionName = $"Unassigned {buttonMap.ButtonName}";
                item.Actions = buttonMap.GetHotasActions().ToObservableCollection();
            }

            if (Catalog.Contains(item))
            {
                Logging.Log.Info($"{item.ActionName} - {buttonMap.ButtonName} already exists in actions catalog. Removing first.");
                Catalog.Remove(item);
            }
            Catalog.Add(item);
            Logging.Log.Info($"{item.ActionName} - {buttonMap.ButtonName} added to actions catalog");
            return item;
        }

        private void AddEmptyItem()
        {
            Add(new ActionCatalogItem() { ActionName = "<No Action>", Actions = new ObservableCollection<ButtonAction>() });
        }
    }
}

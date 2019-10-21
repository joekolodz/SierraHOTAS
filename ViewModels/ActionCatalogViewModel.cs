using SierraHOTAS.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace SierraHOTAS.ViewModels
{
    public class ActionCatalogViewModel
    {
        public ObservableCollection<ActionCatalogItem> Catalog { get; }

        public ActionCatalogViewModel()
        {
            Catalog = new ObservableCollection<ActionCatalogItem>();
        }

        public void Clear()
        {
            Catalog.Clear();
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

        public ActionCatalogItem Add(MapViewModel map)
        {
            var item = map.ActionItem;
            Logging.Log.Info("Add to actions catalog");
            if (string.IsNullOrWhiteSpace(item.ActionName))
            {
                item.ActionName = $"Unassigned {map.ButtonName}";
                item.Actions = map.GetHotasActions().ToObservableCollection();
            }

            if (Catalog.Contains(item))
            {
                Logging.Log.Info($"{item.ActionName} - {map.ButtonName} already exists in actions catalog. Removing first.");
                Catalog.Remove(item);
            }
            Catalog.Add(item);
            Logging.Log.Info($"{item.ActionName} - {map.ButtonName} added to actions catalog");
            return item;
        }
    }
}

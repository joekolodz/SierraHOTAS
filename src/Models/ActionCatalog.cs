using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SierraHOTAS.Models
{
    public class ActionCatalog : INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ObservableCollection<ActionCatalogItem> Catalog { get; private set; }

        public ActionCatalog()
        {
            Catalog = new ObservableCollection<ActionCatalogItem>();
            AddEmptyItem();

            Catalog.CollectionChanged += Catalog_CollectionChanged;
        }

        private void Catalog_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ActionCatalogItem item)
            {
                ReSortItem(item);
            }
        }

        private void ReSortItem(ActionCatalogItem item)
        {
            var index = Catalog.IndexOf(item);
            Catalog.RemoveAt(index);
            Insert(item);
        }

        private int GetSortPosition(ActionCatalogItem item)
        {
            IComparer<ActionCatalogItem> comparer = Comparer<ActionCatalogItem>.Default;
            var i = 0;
            while (i < Catalog.Count && comparer.Compare(Catalog[i], item) <= 0)
            {
                i++;
            }
            return i;
        }

        //public void Sort()
        //{
        //    Catalog = Catalog.OrderBy(x => x.ActionName).ToObservableCollection();
        //}

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
            Insert(item);
        }

        private void Insert(ActionCatalogItem item)
        {
            var index = GetSortPosition(item);
            Catalog.Insert(index, item);
            Logging.Log.Debug($"[{item.ActionName}] added to actions catalog at position: {index}");
        }

        public ActionCatalogItem Get(string actionName)
        {
            return Catalog.FirstOrDefault(x => x.ActionName == actionName);
        }

        private void AddEmptyItem()
        {
            Add(ActionCatalogItem.EmptyItem());
        }
    }
}

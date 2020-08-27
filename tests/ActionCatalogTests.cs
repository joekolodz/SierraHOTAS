using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;
using Xunit;

namespace SierraHOTAS.Tests
{
    public class ActionCatalogTests
    {
        [Fact]
        public void new_catalog_then_noActionItem_exists()
        {
            var catalog = new ActionCatalogViewModel();
            Assert.True(catalog.Contains("<No Action>"));
        }

        [Fact]
        public void add_item_then_value_exists()
        {
            var catalog = new ActionCatalogViewModel();
            var item = new ActionCatalogItem() { ActionName = "testitem1" };
            catalog.Add(item);
            Assert.True(catalog.Contains(item.ActionName));
        }

        [Fact]
        public void add_item_twice_then_not_added()
        {
            var catalog = new ActionCatalogViewModel();
            var item = new ActionCatalogItem() { ActionName = "testitem1" };
            catalog.Add(item);
            Assert.Equal(2, catalog.Catalog.Count);
            catalog.Add(item);
            Assert.Equal(2, catalog.Catalog.Count);
        }

        [Fact]
        public void add_item_with_same_name_then_not_added()
        {
            var sameName = "samename";
            var catalog = new ActionCatalogViewModel();
            var item1 = new ActionCatalogItem() { ActionName = sameName };
            catalog.Add(item1);
            Assert.Equal(2, catalog.Catalog.Count);

            var item2 = new ActionCatalogItem() { ActionName = sameName };
            catalog.Add(item2);
            Assert.Equal(2, catalog.Catalog.Count);
        }

        [Fact]
        public void add_map_then_value_exists()
        {
            var catalog = new ActionCatalogViewModel();
            var item = new ButtonMapViewModel() { ActionItem = new ActionCatalogItem() { ActionName = "testitem", NoAction = false } };
            catalog.Add(item);
            Assert.True(catalog.Contains(item.ActionName));
        }

        [Fact]
        public void add_map_twice_then_not_added()
        {
            var catalog = new ActionCatalogViewModel();
            var item = new ButtonMapViewModel() { ActionItem = new ActionCatalogItem() { ActionName = "testitem", NoAction = false } };
            catalog.Add(item);
            Assert.Equal(2, catalog.Catalog.Count);
            catalog.Add(item);
            Assert.Equal(2, catalog.Catalog.Count);
        }

        [Fact]
        public void add_map_with_blank_buttonName_generates_new_name()
        {
            var catalog = new ActionCatalogViewModel();
            var item = new ButtonMapViewModel() {ButtonName = "Buttons0", ActionItem = new ActionCatalogItem() { ActionName = "", NoAction = false } };
            catalog.Add(item);
            Assert.Equal("Action for Buttons0",item.ActionName);
        }
    }
}

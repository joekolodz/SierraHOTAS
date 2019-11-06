using SierraHOTAS.ViewModels;
using System.Windows.Controls;

namespace SierraHOTAS.Controls
{
    /// <summary>
    /// Interaction logic for MacroTransport.xaml
    /// </summary>
    public partial class MacroTransport : UserControl
    {
        public MacroTransport()
        {
            InitializeComponent();
        }
        private void ActionList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ComboBox box) || !(box.DataContext is ButtonMapViewModel mapContext)) return;
            if (e.AddedItems.Count <= 0) return;
            if (!(e.AddedItems[0] is ActionCatalogItem selectedAction)) return;

            mapContext.AssignActions(selectedAction);
        }
    }
}

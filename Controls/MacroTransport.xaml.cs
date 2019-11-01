using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SierraHOTAS.ViewModel;
using SierraHOTAS.ViewModels;

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

            Logging.Log.Info($"map selected to be changed: {mapContext.ButtonName}");

            mapContext.AssignActions(selectedAction);
        }
    }
}

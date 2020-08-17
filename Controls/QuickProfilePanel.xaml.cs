using System.ComponentModel;
using SierraHOTAS.ViewModels;
using System.Windows.Controls;

namespace SierraHOTAS.Controls
{
    public partial class QuickProfilePanel : UserControl
    {
        public QuickProfilePanelViewModel QuickProfilePanelViewModel { get; }
        public QuickProfilePanel()
        {
            InitializeComponent();

            QuickProfilePanelViewModel = new QuickProfilePanelViewModel();
            DataContext = QuickProfilePanelViewModel;

            if (DesignerProperties.GetIsInDesignMode(this)) return;

            Loaded += QuickProfilePanel_Loaded;
        }

        private void QuickProfilePanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            QuickProfilePanelViewModel.SetupQuickProfiles();
        }
    }
}

using SierraHOTAS.ViewModels;
using System.ComponentModel;
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
            BuildQuickLoadButtons();
        }

        private void BuildQuickLoadButtons()
        {
            var profileIdCount = 0;
            foreach (var child in MainPanel.Children)
            {
                if (child is QuickProfileButton quickButton)
                {
                    if (QuickProfilePanelViewModel.QuickProfilesList.ContainsKey(profileIdCount))
                    {
                        var fileName = QuickProfilePanelViewModel.QuickProfilesList[profileIdCount];
                        quickButton.SetFileName(fileName);
                    }
                    quickButton.ProfileId = profileIdCount++;
                    quickButton.QuickLoadButtonClicked += QuickButton_QuickLoadButtonClicked;
                    quickButton.QuickLoadButtonCleared += QuickButton_QuickLoadButtonCleared;
                }
            }
        }

        private void QuickButton_QuickLoadButtonCleared(object sender, System.EventArgs e)
        {
            if (!(sender is QuickProfileButton quickButton)) return;

            QuickProfilePanelViewModel.QuickProfileClearedCommand.Execute(quickButton.ProfileId);
            quickButton.Reset();
        }

        private void QuickButton_QuickLoadButtonClicked(object sender, System.EventArgs e)
        {
            var quickButton = sender as QuickProfileButton;
            QuickProfilePanelViewModel.QuickProfileSelectedCommand.Execute(quickButton?.ProfileId);

            if (quickButton == null || !QuickProfilePanelViewModel.QuickProfilesList.ContainsKey(quickButton.ProfileId)) return;

            var fileName = QuickProfilePanelViewModel.QuickProfilesList[quickButton.ProfileId];
            quickButton.SetFileName(fileName);
        }
    }
}

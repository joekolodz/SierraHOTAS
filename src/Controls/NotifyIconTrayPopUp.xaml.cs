using SierraHOTAS.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SierraHOTAS.Controls
{
    /// <summary>
    /// Interaction logic for NotifyIconTrayPopUp.xaml
    /// </summary>
    public partial class NotifyIconTrayPopUp : UserControl
    {
        public QuickProfilePanelViewModel QuickProfilePanelViewModel { get; set; }

        public NotifyIconTrayPopUp()
        {
            InitializeComponent();
            DataContextChanged += QuickProfilePanel_DataContextChanged;
        }

        private void QuickProfilePanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(DataContext is QuickProfilePanelViewModel viewModel)) return;

            QuickProfilePanelViewModel = viewModel;
            Initialize();
        }

        private void Initialize()
        {
            QuickProfilePanelViewModel.SetupQuickProfiles();
            QuickProfilePanelViewModel.PropertyChanged += QuickProfilePanelViewModel_PropertyChanged;
            BuildQuickLoadButtons();
        }

        private void QuickProfilePanelViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(QuickProfilePanelViewModel.QuickProfilesList)) return;
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
                        quickButton.ProfileId = profileIdCount;
                        quickButton.QuickLoadButtonClicked += QuickButton_QuickLoadButtonClicked;
                        quickButton.HideClearButton = true;
                        quickButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        quickButton.Visibility = Visibility.Collapsed;
                    }
                    profileIdCount++;
                }
            }
        }

        private void QuickButton_QuickLoadButtonClicked(object sender, System.EventArgs e)
        {
            if (!(sender is QuickProfileButton quickButton)) return;

            if (QuickProfilePanelViewModel.QuickProfilesList.ContainsKey(quickButton.ProfileId))
            {
                QuickProfilePanelViewModel.QuickProfileSelectedCommand.Execute(quickButton?.ProfileId);
            }
        }
    }
}

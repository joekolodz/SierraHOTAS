﻿using SierraHOTAS.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SierraHOTAS.Controls
{
    public partial class QuickProfilePanel : UserControl
    {
        public QuickProfilePanelViewModel QuickProfilePanelViewModel { get; set; }

        public QuickProfilePanel()
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
            if (!(sender is QuickProfileButton quickButton)) return;

            if (QuickProfilePanelViewModel.QuickProfilesList.ContainsKey(quickButton.ProfileId))
            {
                QuickProfilePanelViewModel.QuickProfileSelectedCommand.Execute(quickButton?.ProfileId);
            }
            else
            {
                QuickProfilePanelViewModel.QuickProfileRequestedCommand.Execute(quickButton?.ProfileId);
                if(!QuickProfilePanelViewModel.QuickProfilesList.ContainsKey(quickButton.ProfileId)) return;
                var fileName = QuickProfilePanelViewModel.QuickProfilesList[quickButton.ProfileId];
                quickButton.SetFileName(fileName);
            }
        }
    }
}

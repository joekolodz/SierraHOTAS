using SierraHOTAS.ModeProfileWindow;
using SierraHOTAS.ViewModels.Commands;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace SierraHOTAS.ViewModels
{
    public class QuickProfilePanelViewModel
    {
        private const string QUICK_PROFILE_LIST_FILE_NAME = "quick-profile-list.json";

        private const string INVALID_JSON_MESSAGE = "Could not load file! Is this a SierraHOTAS compatible JSON file?";

        public Dictionary<int, string> QuickProfilesList { get; set; }

        private ICommand _quickProfileSelectedCommand;

        public ICommand QuickProfileSelectedCommand => _quickProfileSelectedCommand ?? (_quickProfileSelectedCommand = new CommandHandlerWithParameter<int>(QuickProfile_Selected));

        private ICommand _quickProfileClearedCommand;
        public ICommand QuickProfileClearedCommand => _quickProfileClearedCommand ?? (_quickProfileClearedCommand = new CommandHandlerWithParameter<int>(QuickProfile_Cleared));

        public void SetupQuickProfiles()
        {
            QuickProfilesList = FileSystem.LoadQuickProfilesList(QUICK_PROFILE_LIST_FILE_NAME) ?? new Dictionary<int, string>();
        }

        public void QuickProfile_Selected(int quickProfileId)
        {
            if (QuickProfilesList.ContainsKey(quickProfileId))
            {
                var path = QuickProfilesList[quickProfileId];
                var profileEvent = new QuickProfileSelectedEvent()
                {
                    Id = quickProfileId,
                    Path = path
                };
                EventAggregator.Publish(profileEvent);
            }
            else
            {
                var path = FileSystem.ChooseHotasProfileForQuickLoad();

                if (string.IsNullOrWhiteSpace(path)) return;

                var hotas = FileSystem.FileOpen(path);
                if (hotas == null)
                {
                    var modeMessageWindow = new ModeProfileMessageWindow(INVALID_JSON_MESSAGE)
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    };
                    modeMessageWindow.ShowDialog();
                    return;
                }

                QuickProfilesList.Add(quickProfileId, path);
                FileSystem.SaveQuickProfilesList(QuickProfilesList, QUICK_PROFILE_LIST_FILE_NAME);
            }
        }

        public void QuickProfile_Cleared(int quickProfileId)
        {
            if (!QuickProfilesList.ContainsKey(quickProfileId)) return;
            QuickProfilesList.Remove(quickProfileId);
            FileSystem.SaveQuickProfilesList(QuickProfilesList, QUICK_PROFILE_LIST_FILE_NAME);
        }
    }
}

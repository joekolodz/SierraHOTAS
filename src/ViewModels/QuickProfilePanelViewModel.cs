using SierraHOTAS.Models;
using SierraHOTAS.ModeProfileWindow;
using SierraHOTAS.ViewModels.Commands;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace SierraHOTAS.ViewModels
{
    public class QuickProfilePanelViewModel : DependencyObject, INotifyPropertyChanged
    {
        private const string QUICK_PROFILE_LIST_FILE_NAME = "quick-profile-list.json";
        private const string INVALID_JSON_MESSAGE = "Could not load file! Is this a SierraHOTAS compatible JSON file?";
        private readonly IFileSystem _fileSystem;
        private readonly IEventAggregator _eventAggregator;

        public Dictionary<int, string> QuickProfilesList { get; set; }

        private ICommand _quickProfileSelectedCommand;
        public ICommand QuickProfileSelectedCommand => _quickProfileSelectedCommand ?? (_quickProfileSelectedCommand = new CommandHandlerWithParameter<int>(QuickProfile_Selected));
        
        private ICommand _quickProfileRequestedCommand;
        public ICommand QuickProfileRequestedCommand => _quickProfileRequestedCommand ?? (_quickProfileRequestedCommand = new CommandHandlerWithParameter<int>(QuickProfile_Requested));

        private ICommand _quickProfileClearedCommand;
        public ICommand QuickProfileClearedCommand => _quickProfileClearedCommand ?? (_quickProfileClearedCommand = new CommandHandlerWithParameter<int>(QuickProfile_Cleared));

        public QuickProfilePanelViewModel(IEventAggregator eventAggregator, IFileSystem fileSystem)
        {
            _eventAggregator = eventAggregator;
            _fileSystem = fileSystem;
        }

        public void SetupQuickProfiles()
        {
            QuickProfilesList = _fileSystem.LoadQuickProfilesList(QUICK_PROFILE_LIST_FILE_NAME) ?? new Dictionary<int, string>();
        }

        public void QuickProfile_Requested(int quickProfileId)
        {
            var path = _fileSystem.ChooseHotasProfileForQuickLoad();

            if (string.IsNullOrWhiteSpace(path)) return;

            var hotas = _fileSystem.FileOpen(path);
            //if (hotas == null)
            //{
            //    var modeMessageWindow = new MessageWindow(INVALID_JSON_MESSAGE)
            //    {
            //        Owner = Application.Current.MainWindow,
            //        WindowStartupLocation = WindowStartupLocation.CenterOwner,
            //    };
            //    modeMessageWindow.ShowDialog();
            //    return;
            //}

            QuickProfilesList.Add(quickProfileId, path);
            OnPropertyChanged(nameof(QuickProfilesList));

            _fileSystem.SaveQuickProfilesList(QuickProfilesList, QUICK_PROFILE_LIST_FILE_NAME);
        }

        public void QuickProfile_Selected(int quickProfileId)
        {
            if (!QuickProfilesList.ContainsKey(quickProfileId)) return;

            var path = QuickProfilesList[quickProfileId];
            var profileEvent = new QuickProfileSelectedEvent()
            {
                Id = quickProfileId,
                Path = path
            };
            _eventAggregator.Publish<QuickProfileSelectedEvent>(profileEvent);
        }

        public void QuickProfile_Cleared(int quickProfileId)
        {
            if (!QuickProfilesList.ContainsKey(quickProfileId)) return;
            QuickProfilesList.Remove(quickProfileId);
            OnPropertyChanged(nameof(QuickProfilesList));

            _fileSystem.SaveQuickProfilesList(QuickProfilesList, QUICK_PROFILE_LIST_FILE_NAME);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

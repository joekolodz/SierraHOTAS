﻿using System;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels.Commands;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        public event EventHandler<EventArgs> ShowMainWindow;
        public event EventHandler<EventArgs> Close;

        public Dictionary<int, QuickProfileItem> QuickProfilesList { get; set; } = new Dictionary<int, QuickProfileItem>();

        private ICommand _quickProfileSelectedCommand;
        public ICommand QuickProfileSelectedCommand => _quickProfileSelectedCommand ?? (_quickProfileSelectedCommand = new CommandHandlerWithParameter<int>(QuickProfile_Selected));
        
        private ICommand _quickProfileRequestedCommand;
        public ICommand QuickProfileRequestedCommand => _quickProfileRequestedCommand ?? (_quickProfileRequestedCommand = new CommandHandlerWithParameter<int>(QuickProfile_Requested));

        private ICommand _quickProfileClearedCommand;
        public ICommand QuickProfileClearedCommand => _quickProfileClearedCommand ?? (_quickProfileClearedCommand = new CommandHandlerWithParameter<int>(QuickProfile_Cleared));

        private ICommand _autoLoadSelectedCommand;
        public ICommand AutoLoadSelectedCommand => _autoLoadSelectedCommand ?? (_autoLoadSelectedCommand = new CommandHandlerWithParameter<int>(QuickProfile_AutoLoadSelected));

        public QuickProfilePanelViewModel() { }

        public QuickProfilePanelViewModel(IEventAggregator eventAggregator, IFileSystem fileSystem)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public void SetupQuickProfiles()
        {
            if (QuickProfilesList != null && QuickProfilesList.Count > 0) return;

            QuickProfilesList = _fileSystem.LoadQuickProfilesList(QUICK_PROFILE_LIST_FILE_NAME) ?? new Dictionary<int, QuickProfileItem>();
        }

        private void QuickProfile_Requested(int quickProfileId)
        {
            var path = _fileSystem.ChooseHotasProfileForQuickLoad();

            if (string.IsNullOrWhiteSpace(path)) return;

            var hotas = _fileSystem.FileOpen(path);
            if (hotas == null)
            {
                _eventAggregator.Publish(new ShowMessageWindowEvent() { Message = INVALID_JSON_MESSAGE });
                return;
            }

            QuickProfilesList.Add(quickProfileId, new QuickProfileItem(){Path = path, AutoLoad = false});
            
            OnPropertyChanged(nameof(QuickProfilesList));

            _fileSystem.SaveQuickProfilesList(QuickProfilesList, QUICK_PROFILE_LIST_FILE_NAME);
        }

        public void QuickProfile_Selected(int quickProfileId)
        {
            if (!QuickProfilesList.ContainsKey(quickProfileId)) return;

            var profileItem = QuickProfilesList[quickProfileId];
            var profileEvent = new QuickProfileSelectedEvent()
            {
                Id = quickProfileId,
                Path = profileItem.Path
            };
            _eventAggregator.Publish(profileEvent);
        }

        public void QuickProfile_Cleared(int quickProfileId)
        {
            if (!QuickProfilesList.ContainsKey(quickProfileId)) return;
            QuickProfilesList.Remove(quickProfileId);
            OnPropertyChanged(nameof(QuickProfilesList));

            _fileSystem.SaveQuickProfilesList(QuickProfilesList, QUICK_PROFILE_LIST_FILE_NAME);
        }

        private void QuickProfile_AutoLoadSelected(int quickProfileId)
        {
            if (!QuickProfilesList.ContainsKey(quickProfileId)) return;
            var profileItem = QuickProfilesList[quickProfileId];
            var previousValue = profileItem.AutoLoad;

            foreach (var item in QuickProfilesList)
            {
                item.Value.AutoLoad = false;
            }

            profileItem.AutoLoad = !previousValue;
            _fileSystem.SaveQuickProfilesList(QuickProfilesList, QUICK_PROFILE_LIST_FILE_NAME);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ShowWindow()
        {
            ShowMainWindow?.Invoke(this, new EventArgs());
        }

        public virtual string GetAutoLoadPath()
        {
            if (QuickProfilesList == null || QuickProfilesList.Count == 0) return string.Empty;
            return QuickProfilesList.Where(item => item.Value.AutoLoad).Select(item => item.Value.Path).FirstOrDefault();
        }
        
        public void CloseApp()
        {
            Close?.Invoke(this, new EventArgs());
        }
    }
}

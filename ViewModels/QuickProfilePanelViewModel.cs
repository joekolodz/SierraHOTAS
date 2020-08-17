using SierraHOTAS.Annotations;
using SierraHOTAS.ViewModels.Commands;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SierraHOTAS.ViewModels
{
    public class QuickProfilePanelViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Dictionary<int, string> QuickProfilesList { get; set; }

        private ICommand _quickProfileSelectedCommand;

        public ICommand QuickProfileSelectedCommand => _quickProfileSelectedCommand ?? (_quickProfileSelectedCommand = new CommandHandlerWithParameter<string>(QuickProfile_Selected));

        //TODO: move to array of controls
        public bool IsQuickProfileSet1
        {
            get => QuickProfilesList?.Keys.Contains(1) ?? false;
            set { }
        }
        public bool IsQuickProfileSet2
        {
            get => QuickProfilesList?.Keys.Contains(2) ?? false;
            set { }
        }
        public bool IsQuickProfileSet3
        {
            get => QuickProfilesList?.Keys.Contains(3) ?? false;
            set { }
        }
        public bool IsQuickProfileSet4
        {
            get => QuickProfilesList?.Keys.Contains(4) ?? false;
            set { }
        }
        public bool IsQuickProfileSet5
        {
            get => QuickProfilesList?.Keys.Contains(5) ?? false;
            set { }
        }

        public QuickProfilePanelViewModel()
        {
            //setup events
        }

        public void SetupQuickProfiles()
        {
            //TODO: should QuickProfilesList be a model?

            QuickProfilesList = FileSystem.LoadQuickProfilesList("quick-profile-list.json") ?? new Dictionary<int, string>();

            NotifyQuickProfileChanged();
        }

        public void QuickProfile_Selected(string id)
        {
            if (!int.TryParse(id, out var quickProfileId)) return;

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
                QuickProfilesList.Add(quickProfileId, path);
                FileSystem.SaveQuickProfilesList(QuickProfilesList, "quick-profile-list.json");
                NotifyQuickProfileChanged();
            }
        }

        private void NotifyQuickProfileChanged()
        {
            OnPropertyChanged(nameof(IsQuickProfileSet1));
            OnPropertyChanged(nameof(IsQuickProfileSet2));
            OnPropertyChanged(nameof(IsQuickProfileSet3));
            OnPropertyChanged(nameof(IsQuickProfileSet4));
            OnPropertyChanged(nameof(IsQuickProfileSet5));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

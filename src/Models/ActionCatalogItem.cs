using System;
using SierraHOTAS.Annotations;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SierraHOTAS.Models
{
    public class ActionCatalogItem : INotifyPropertyChanged
    {
        private string _actionName;
        private const string NO_ACTION_TEXT = "<No Action>";

        public Guid Id { get; set; }

        public bool NoAction { get; set; } = false;
        
        [DefaultValue("")]
        public string ActionName
        {
            get => _actionName;
            set
            {
                if (_actionName == value) return;
                _actionName = value;
                OnPropertyChanged(nameof(ActionName));
            }
        }

        public ObservableCollection<ButtonAction> Actions { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return $"{ActionName} - {Actions.Count}";
        }

        public ActionCatalogItem()
        {
            ActionName = "";
            Actions = new ObservableCollection<ButtonAction>();
            Id = Guid.NewGuid();
        }

        public static ActionCatalogItem EmptyItem()
        {
            return new ActionCatalogItem {Id = Guid.Empty, NoAction = true, ActionName = NO_ACTION_TEXT, Actions = new ObservableCollection<ButtonAction>() };
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

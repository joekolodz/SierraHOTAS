using SierraHOTAS.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SierraHOTAS.Annotations;

namespace SierraHOTAS.Models
{
    public class ActionCatalogItem : INotifyPropertyChanged
    {
        private string _actionName;
        public bool NoAction { get; set; } = false;
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
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

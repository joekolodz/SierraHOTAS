using System;
using SierraHOTAS.Annotations;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SierraHOTAS.ViewModels.Commands;
using System.Windows.Input;
using SierraJSON;

namespace SierraHOTAS.Models
{
    [SierraJsonObject(SierraJsonObject.MemberSerialization.OptIn)]
    public class ActionCatalogItem : INotifyPropertyChanged, IComparable<ActionCatalogItem>
    {
        public event EventHandler<ActionCatalogItemRemovedRequestedEventArgs> RemoveRequested;

        private string _actionName;
        public const string NO_ACTION_TEXT = "<No Action>";

        [SierraJsonProperty]
        public Guid Id { get; set; }

        public bool NoAction { get; set; } = false;

        public bool IsRemovable => !NoAction;
        
        [DefaultValue("")]
        [SierraJsonProperty]
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

        [SierraJsonProperty]
        public ObservableCollection<ButtonAction> Actions { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public int CompareTo(ActionCatalogItem other)
        {
            if (other == null) return 1;
            var value = string.Compare(other.ActionName, ActionName, comparisonType: StringComparison.OrdinalIgnoreCase) * -1;
            Logging.Log.Info($"Comparing:'{other.ActionName}' to '{ActionName}': {value}");
            return value;
        }

        public override string ToString()
        {
            return $"{ActionName} ({Actions.Count} button actions)";
        }

        private ICommand _removeActionCatalogItemCommand;

        public ICommand RemoveActionCatalogItemCommand => _removeActionCatalogItemCommand ?? (_removeActionCatalogItemCommand = new CommandHandlerWithParameter<ActionCatalogItem>(RemoveActionCatalogItem));

        public ActionCatalogItem()
        {
            ActionName = "";
            Actions = new ObservableCollection<ButtonAction>();
            Id = Guid.NewGuid();
        }

        private void RemoveActionCatalogItem(ActionCatalogItem item)
        {
            RemoveRequested?.Invoke(this, new ActionCatalogItemRemovedRequestedEventArgs(item));
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

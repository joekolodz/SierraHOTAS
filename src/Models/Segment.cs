using SierraHOTAS.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SierraHOTAS.Models
{
    public class Segment : INotifyPropertyChanged
    {
        public int Id { get; set; }

        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public Segment()
        {
        }

        public Segment(int id, int value)
        {
            Id = id;
            Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Segment Clone()
        {
            return new Segment(Id, Value);
        }
    }
}

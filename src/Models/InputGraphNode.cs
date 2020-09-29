using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SierraHOTAS.Models
{
    public class InputGraphValue
    {
        public int Value { get; set; }
    }

    public class InputGraphNode : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        public double X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged(nameof(Y));
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class InputGraphPointNode : InputGraphNode
    {
        public Geometry Geometry { get; set; }
        public Brush Stroke { get; set; }
        public Brush Fill { get; set; }
    }
}

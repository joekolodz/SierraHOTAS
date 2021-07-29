using System.Windows.Threading;
using SierraHOTAS.Models;

namespace SierraHOTAS.Factories
{
    public class DispatcherFactory
    {
        public virtual IDispatcher CreateDispatcher()
        {
            return new DispatcherWrapper(Dispatcher.CurrentDispatcher);
        }
    }
}

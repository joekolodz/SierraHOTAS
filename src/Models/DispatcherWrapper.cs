using System;
using System.Windows.Threading;

namespace SierraHOTAS.Models
{
    public class DispatcherWrapper : IDispatcher
    {
        private readonly Dispatcher _dispatcher;

        public DispatcherWrapper(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Invoke(Action callback)
        {
            _dispatcher.Invoke(callback);
        }
    }
}

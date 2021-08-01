using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;

namespace SierraHOTAS.Models
{
    [ExcludeFromCodeCoverage]
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

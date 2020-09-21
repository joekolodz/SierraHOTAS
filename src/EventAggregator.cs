using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SierraHOTAS
{
    public class EventAggregator : IEventAggregator
    {
        private readonly SynchronizationContext _synchronizationContext;
        private readonly List<Delegate> _subscribers;

        public EventAggregator()
        {
            _synchronizationContext = SynchronizationContext.Current;
            _subscribers = new List<Delegate>();
        }

        public void Subscribe<T>(Action<T> subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            _subscribers.Add(subscriber);
        }
        public void Unsubscribe<T>(Action<T> subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            _subscribers.Remove(subscriber);
        }

        public void Publish<T>(T message)
        {
            if (message == null) return;

            _synchronizationContext.Send(m=>Dispatcher((T)m), message);
            //Dispatcher(message);
        }

        private void Dispatcher<T>(T message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var handlers = _subscribers.OfType<Action<T>>().ToList();
            foreach (var handler in handlers)
            {
                handler(message);
            }
        }

    }
}

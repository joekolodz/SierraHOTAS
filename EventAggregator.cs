using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SierraHOTAS
{
    public static class EventAggregator
    {
        private static readonly SynchronizationContext synchronizationContext;
        private static readonly List<Delegate> subscribers = new List<Delegate>();

        static EventAggregator()
        {
            synchronizationContext = SynchronizationContext.Current;
        }

        public static void Subscribe<T>(Action<T> subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            subscribers.Add(subscriber);
        }
        public static void Unsubscribe<T>(Action<T> subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            subscribers.Remove(subscriber);
        }

        public static void Publish<T>(T message)
        {
            if (message == null) return;

            synchronizationContext.Post(m=>Dispatcher((T)m), message);
        }

        private static void Dispatcher<T>(T message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var handlers = subscribers.OfType<Action<T>>().ToList();
            foreach (var handler in handlers)
            {
                handler(message);
            }
        }

    }
}

using System;

namespace SierraHOTAS
{
    public interface IEventAggregator
    {
        void Subscribe<T>(Action<T> subscriber);
        void Unsubscribe<T>(Action<T> subscriber);
        void Publish<T>(T message);
    }
}
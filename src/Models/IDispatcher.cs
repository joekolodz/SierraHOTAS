using System;

namespace SierraHOTAS.Models
{
    public interface IDispatcher
    {
        void Invoke(Action callback);
    }
}

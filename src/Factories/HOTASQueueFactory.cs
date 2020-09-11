using SierraHOTAS.Models;

namespace SierraHOTAS.Factories
{
    public class HOTASQueueFactory
    {
        public virtual IHOTASQueue CreateHOTASQueue()
        {
            return new HOTASQueue();
        }
    }
}

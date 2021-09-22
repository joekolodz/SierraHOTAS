using SierraHOTAS.Models;
using SierraHOTAS.Win32;

namespace SierraHOTAS.Factories
{
    public class HOTASQueueFactory
    {
        private readonly IKeyboard _keyboard;
        public HOTASQueueFactory(IKeyboard keyboard)
        {
            _keyboard = keyboard;
        }

        public virtual IHOTASQueue CreateHOTASQueue()
        {
            return new HOTASQueue(_keyboard);
        }
    }
}

using SharpDX.DirectInput;
using SierraHOTAS.Models;

namespace SierraHOTAS.Factories
{
    public class DirectInputFactory
    {
        public virtual IDirectInput CreateDirectInput()
        {
            return new DirectInputWrapper(new DirectInput());
        }
    }
}

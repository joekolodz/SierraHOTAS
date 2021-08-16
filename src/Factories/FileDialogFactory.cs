using SierraHOTAS.Models;

namespace SierraHOTAS.Factories
{
    public class FileDialogFactory
    {
        public virtual IOpenFileDialog CreateOpenFileDialog()
        {
            return new OpenFileDialogWrapper();

        }
        public virtual ISaveFileDialog CreateSaveFileDialog()
        {
            return new SaveFileDialogWrapper();

        }
    }
}

using SierraHOTAS.Models;

namespace SierraHOTAS.Factories
{
    public class FileDialogFactory
    {
        public IOpenFileDialog CreateOpenFileDialog()
        {
            return new OpenFileDialogWrapper();

        }
        public ISaveFileDialog CreateSaveFileDialog()
        {
            return new SaveFileDialogWrapper();

        }
    }
}

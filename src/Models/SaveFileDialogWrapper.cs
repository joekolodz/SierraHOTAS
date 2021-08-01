using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;

namespace SierraHOTAS.Models
{
    [ExcludeFromCodeCoverage]
    public class SaveFileDialogWrapper : ISaveFileDialog
    {
        private readonly SaveFileDialog _saveFileDialog;

        public SaveFileDialogWrapper()
        {
            _saveFileDialog = new SaveFileDialog();
        }

        public string FileName
        {
            get => _saveFileDialog.FileName;
            set => _saveFileDialog.FileName = value;
        }

        public string DefaultExt
        {
            get => _saveFileDialog.DefaultExt;
            set => _saveFileDialog.DefaultExt = value;
        }

        public string Filter
        {
            get => _saveFileDialog.Filter;
            set => _saveFileDialog.Filter = value;
        }

        public bool? ShowDialog()
        {
            return _saveFileDialog.ShowDialog();
        }
    }
}

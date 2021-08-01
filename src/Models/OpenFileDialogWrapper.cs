using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;

namespace SierraHOTAS.Models
{
    [ExcludeFromCodeCoverage]
    public class OpenFileDialogWrapper : IOpenFileDialog
    {
        private readonly OpenFileDialog _openFileDialog;

        public OpenFileDialogWrapper()
        {
            _openFileDialog = new OpenFileDialog();
        }

        public string FileName
        {
            get => _openFileDialog.FileName;
            set => _openFileDialog.FileName = value;
        }

        public string DefaultExt
        {
            get => _openFileDialog.DefaultExt;
            set => _openFileDialog.DefaultExt = value;
        }

        public string Filter
        {
            get => _openFileDialog.Filter;
            set => _openFileDialog.Filter = value;
        }

        public string InitialDirectory 
        { 
            get => _openFileDialog.InitialDirectory;
            set => _openFileDialog.InitialDirectory = value;
        }

        public bool? ShowDialog()
        {
            return _openFileDialog.ShowDialog();
        }
    }
}


namespace SierraHOTAS.Models
{
    public interface IOpenFileDialog
    {
        string FileName { get; set; }
        string DefaultExt { get; set; }
        string Filter { get; set; }
        string InitialDirectory { get; set; }
        bool? ShowDialog();
    }
}

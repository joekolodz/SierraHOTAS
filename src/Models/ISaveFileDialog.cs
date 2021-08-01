namespace SierraHOTAS.Models
{
    public interface ISaveFileDialog
    {
        string FileName { get; set; }
        string DefaultExt { get; set; }
        string Filter { get; set; }
        bool? ShowDialog();
    }
}

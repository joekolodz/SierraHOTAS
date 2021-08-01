using System.IO;

namespace SierraHOTAS.Models
{
    public interface IFileIO
    {
        string GetExecutingAssemblyLocation();
        string GetDirectoryName(string path);
        string Combine(string folder, string fileName);
        StreamWriter CreateText(string path);
        StreamReader OpenText(string path);
        bool FileExists(string path);
        string GetCurrentDirectory();
    }
}

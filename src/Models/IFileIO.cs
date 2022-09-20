using System.IO;

namespace SierraHOTAS.Models
{
    public interface IFileIO
    {
        string GetExecutingAssemblyLocation();
        string GetDirectoryName(string path);
        string Combine(string folder, string fileName);
        StreamWriter CreateText(string path);
        void WriteAllText(string path, string contents);
        StreamReader OpenText(string path);
        string ReadAllText(string path);
        bool FileExists(string path);
        string GetCurrentDirectory();
        IFileStream CreateFileStream(string path, FileMode fileMode, FileAccess fileAccess);
    }
}

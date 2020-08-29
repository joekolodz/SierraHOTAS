using System.IO;

namespace SierraHOTAS.Models
{
    public interface IFileIO
    {
        string GetDirectoryName(string path);
        string Combine(string path1, string path2);
        StreamWriter CreateText(string path);
        bool Exists(string path);
        StreamReader OpenText(string path);
    }
}

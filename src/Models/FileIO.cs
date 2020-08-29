using System.IO;

namespace SierraHOTAS.Models
{
    public class FileIO : IFileIO
    {
        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public string Combine(string path1, string path2)
        {
           return Path.Combine(path1, path2);
        }

        public StreamWriter CreateText(string path)
        {
            return File.CreateText(path);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public StreamReader OpenText(string path)
        {
            return File.OpenText(path);
        }
    }
}

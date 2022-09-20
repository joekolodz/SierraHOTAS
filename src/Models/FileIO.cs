using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SierraHOTAS.Models
{
    [ExcludeFromCodeCoverage]
    public class FileIO : IFileIO
    {
        public string GetExecutingAssemblyLocation()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public string Combine(string folder, string fileName)
        {
            return Path.Combine(folder, fileName);
        }

        public StreamWriter CreateText(string path)
        {
            return File.CreateText(path);
        }

        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public StreamReader OpenText(string path)
        {
            return File.OpenText(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string GetCurrentDirectory()
        {
            return Environment.CurrentDirectory;
        }

        public IFileStream CreateFileStream(string path, FileMode fileMode, FileAccess fileAccess)
        {
            return new FileStreamWrapper(new FileStream(path, fileMode, fileAccess));
        }
    }
}

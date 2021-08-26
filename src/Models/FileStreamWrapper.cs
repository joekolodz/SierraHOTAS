using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SierraHOTAS.Models
{
    public class FileStreamWrapper : IFileStream, IDisposable
    {
        private readonly FileStream _fileStream;

        public FileStreamWrapper(FileStream fileStream)
        {
            _fileStream = fileStream;
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return _fileStream.Seek(offset, origin);
        }

        public int Read([In, Out] byte[] array, int offset, int count)
        {
            return _fileStream.Read(array, offset, count);
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }
    }
}

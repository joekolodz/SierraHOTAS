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

        public int Read(out byte[] array, int offset, int count)
        {
            var inner = new byte[count];
            var read = _fileStream.Read(inner, offset, count);
            array = inner;
            return read;
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }
    }
}

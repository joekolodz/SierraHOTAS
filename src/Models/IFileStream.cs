using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SierraHOTAS.Models
{
    public interface IFileStream : IDisposable
    {
        long Seek(long offset, SeekOrigin origin);
        int Read([In, Out] byte[] array, int offset, int count);
    }
}

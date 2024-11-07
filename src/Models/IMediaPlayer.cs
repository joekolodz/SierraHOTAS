using System;

namespace SierraHOTAS.Models
{
    public interface IMediaPlayer
    {
        void Play();
        void Close();
        void Open(string sourceFilePath);
        float Volume { get; set; }
        long Position { get; set; }
    }
}

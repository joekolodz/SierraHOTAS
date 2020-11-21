using System;
using System.Windows.Threading;

namespace SierraHOTAS.Models
{
    public interface IMediaPlayer
    {
        void Play();
        void Close();
        void Open(Uri source);
        Dispatcher Dispatcher { get; }
        bool IsMuted { get; set; }
        double Volume { get; set; }
        TimeSpan Position { get; set; }
    }
}

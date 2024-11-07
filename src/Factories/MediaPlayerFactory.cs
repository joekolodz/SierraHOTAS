using NAudio.Wave;
using SierraHOTAS.Models;

namespace SierraHOTAS.Factories
{
    public class MediaPlayerFactory
    {
        public virtual IMediaPlayer CreateMediaPlayer()
        {
            return new MediaPlayerWrapper(new WaveOutEvent());
        }
    }
}

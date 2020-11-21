using SierraHOTAS.Models;
using System.Windows.Media;

namespace SierraHOTAS.Factories
{
    public class MediaPlayerFactory
    {
        public virtual IMediaPlayer CreateMediaPlayer()
        {
            return new MediaPlayerWrapper(new MediaPlayer());
        }
    }
}

using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace SierraHOTAS.Models
{
    public class MediaPlayerWrapper : IMediaPlayer
    {
        private readonly MediaPlayer _mediaPlayer;

        public MediaPlayerWrapper(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
        }

        public void Play()
        {
            _mediaPlayer.Play();
        }

        public void Close()
        {
            _mediaPlayer.Close();
        }

        public void Open(Uri source)
        {
            _mediaPlayer.Open(source);
        }

        public Dispatcher Dispatcher => _mediaPlayer.Dispatcher;
        

        public bool IsMuted
        {
            get => _mediaPlayer.IsMuted;
            set => _mediaPlayer.IsMuted = value;
        }

        public double Volume
        {
            get => _mediaPlayer.Volume;
            set => _mediaPlayer.Volume = value;
        }

        public TimeSpan Position
        {
            get => _mediaPlayer.Position;
            set => _mediaPlayer.Position = value;
        }
    }
}

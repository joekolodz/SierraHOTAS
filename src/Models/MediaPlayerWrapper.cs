using NAudio.Wave;
using System.Diagnostics.CodeAnalysis;

namespace SierraHOTAS.Models
{
    [ExcludeFromCodeCoverage]
    public class MediaPlayerWrapper : IMediaPlayer
    {
        private readonly WaveOutEvent _outputDevice;
        private AudioFileReader _audioFile;

        public MediaPlayerWrapper(WaveOutEvent outputDevice)
        {
            _outputDevice = outputDevice;
        }

        public void Play()
        {
            _outputDevice.Play();
        }

        public void Close()
        {
            _audioFile?.Close();
        }

        public void Open(string sourceFilePath)
        {
            _audioFile = new AudioFileReader(sourceFilePath);
            _outputDevice.Init(_audioFile);
        }

        public float Volume
        {
            get => _outputDevice.Volume;
            set => _outputDevice.Volume = value;
        }

        public long Position
        {
            get => _audioFile.Position;
            set => _audioFile.Position = value;
        }
    }
}

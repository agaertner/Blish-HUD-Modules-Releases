using NAudio.Wave;
using System;

namespace Nekres.Music_Mixer.Core.Player.Source
{
    internal class EndOfStreamProvider : ISampleProvider
    {
        public event EventHandler<EventArgs> Ended;
        
        public WaveFormat WaveFormat => _sourceProvider.WaveFormat;

        public bool IsBuffering { get; private set; }

        private bool _ended;

        private MediaFoundationReader _mediaProvider;

        private ISampleProvider _sourceProvider;

        public EndOfStreamProvider(MediaFoundationReader mediaProvider)
        {
            _mediaProvider = mediaProvider;
            _sourceProvider = mediaProvider.ToSampleProvider();
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = _sourceProvider.Read(buffer, offset, count);

            this.IsBuffering = read <= 0;
            
            if (_mediaProvider.CurrentTime >= _mediaProvider.TotalTime && !_ended)
            {
                _ended = true;
                Ended?.Invoke(this, EventArgs.Empty);
            }
            return read;
        }
    }
}

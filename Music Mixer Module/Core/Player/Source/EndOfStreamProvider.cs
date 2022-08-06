using NAudio.Wave;
using System;

namespace Nekres.Music_Mixer.Core.Player.Source
{
    internal class EndOfStreamProvider : ISampleProvider
    {
        private ISampleProvider _sourceProvider;

        public event EventHandler<EventArgs> Ended;
        
        public WaveFormat WaveFormat => _sourceProvider.WaveFormat;

        private bool _ended;

        public EndOfStreamProvider(ISampleProvider source)
        {
            _sourceProvider = source;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = _sourceProvider.Read(buffer, offset, count);
            if (read <= 0 && !_ended)
            {
                _ended = true;
                Ended?.Invoke(this, EventArgs.Empty);
            }
            return read;
        }
    }
}

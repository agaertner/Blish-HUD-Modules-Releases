using NAudio.Wave;
using Nekres.Music_Mixer.Core.Player.Source.DSP;

namespace Nekres.Music_Mixer.Core.Player.Source
{
    internal class BiQuadFilterSource : ISampleProvider
    {
        private readonly object _lockObject = new object();
        private BiQuad _biquad;
        private ISampleProvider _sourceProvider;

        public WaveFormat WaveFormat => _sourceProvider.WaveFormat;

        public bool Enabled { get; set; }

        public BiQuad Filter
        {
            get => _biquad;
            set
            {
                lock (_lockObject)
                {
                    _biquad = value;
                }
            }
        }

        public BiQuadFilterSource(ISampleProvider source)
        {
            _sourceProvider = source;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = _sourceProvider.Read(buffer, offset, count);
            lock (_lockObject)
            {
                if (Filter != null && Enabled)
                {
                    for (int i = 0; i < read; i++)
                    {
                        buffer[i + offset] = Filter.Process(buffer[i + offset]);
                    }
                }
            }

            return read;
        }
    }
}

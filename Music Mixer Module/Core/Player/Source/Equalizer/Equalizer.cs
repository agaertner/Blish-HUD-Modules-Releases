using System;
using System.Collections;
using System.Collections.Generic;
using NAudio.Wave;

namespace Nekres.Music_Mixer.Core.Player.Source.Equalizer
{
    /// <summary>
    ///     Represents an equalizer which can be dynamically modified by adding, removing or modifying
    ///     <see cref="EqualizerFilter" />.
    /// </summary>
    public class Equalizer : ISampleProvider
    {
        private readonly EqualizerFilterCollection _equalizerFilters = new();

        private ISampleProvider _sampleProvider;

        public WaveFormat WaveFormat => _sampleProvider.WaveFormat;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Equalizer" /> class based on an underlying wave stream.
        /// </summary>
        /// <param name="source">The underlying wave stream.</param>
        public Equalizer(ISampleProvider source)
        {
            _sampleProvider = source;
            if (source == null)
                throw new ArgumentNullException(nameof(source));
        }

        /// <summary>
        ///     Gets a list which contains all <see cref="EqualizerFilter" /> used by the equalizer.
        /// </summary>
        /// <remarks>
        ///     None of the <see cref="EqualizerFilter" />
        /// </remarks>
        public IList<EqualizerFilter> SampleFilters => _equalizerFilters;

        /// <summary>
        ///     Returns a new instance of the <see cref="Equalizer" /> class with 10 preset <see cref="EqualizerFilter" />.
        /// </summary>
        /// <param name="source">The underlying sample source which provides the data for the equalizer.</param>
        /// <returns>A new instance of the <see cref="Equalizer" /> class with 10 preset <see cref="EqualizerFilter" />.</returns>
        public static Equalizer Create10BandEqualizer(ISampleProvider source)
        {
            return Create10BandEqualizer(source, 18, 0);
        }

        /// <summary>
        ///     Returns a new instance of the <see cref="Equalizer" /> class with 10 preset <see cref="EqualizerFilter" />.
        /// </summary>
        /// <param name="source">The underlying sample source which provides the data for the equalizer.</param>
        /// <param name="bandWidth">The bandwidth to use for the 10 <see cref="EqualizerFilter" />. The default value is 18.</param>
        /// <param name="defaultGain">
        ///     The default gain to use for the 10 <see cref="EqualizerFilter" />. The default value is zero
        ///     which means that the data, passed through the equalizer won't be affected by the <see cref="EqualizerFilter" />.
        /// </param>
        /// <returns>A new instance of the <see cref="Equalizer" /> class with 10 preset <see cref="EqualizerFilter" />.</returns>
        public static Equalizer Create10BandEqualizer(ISampleProvider source, int bandWidth, int defaultGain)
        {
            int sampleRate = source.WaveFormat.SampleRate;
            int channels = source.WaveFormat.Channels;

            if (sampleRate < 32000)
            {
                throw new ArgumentException(
                    "The sample rate of the source must not be less than 32kHz since the 10 band eq includes a 16kHz filter.",
                    nameof(source));
            }

            var sampleFilters = new EqualizerChannelFilter[]
            {
                new (sampleRate, 31, bandWidth, defaultGain),
                new (sampleRate, 62, bandWidth, defaultGain),
                new (sampleRate, 125, bandWidth, defaultGain),
                new (sampleRate, 250, bandWidth, defaultGain),
                new (sampleRate, 500, bandWidth, defaultGain),
                new (sampleRate, 1000, bandWidth, defaultGain),
                new (sampleRate, 2000, bandWidth, defaultGain),
                new (sampleRate, 4000, bandWidth, defaultGain),
                new (sampleRate, 8000, bandWidth, defaultGain),
                new (sampleRate, 16000, bandWidth, defaultGain)
            };

            var equalizer = new Equalizer(source);
            foreach (var equalizerChannelFilter in sampleFilters)
            {
                equalizer.SampleFilters.Add(new EqualizerFilter(channels, equalizerChannelFilter));
            }
            return equalizer;
        }

        /// <summary>
        ///     Reads a sequence of samples from the underlying <see cref="ISampleProvider" />, applies the equalizer
        ///     effect and advances the position within the stream by
        ///     the number of samples read.
        /// </summary>
        /// <param name="buffer">
        ///     An array of floats. When this method returns, the <paramref name="buffer" /> contains the specified
        ///     float array with the values between <paramref name="offset" /> and (<paramref name="offset" /> +
        ///     <paramref name="count" /> - 1) replaced by the floats read from the current source.
        /// </param>
        /// <param name="offset">
        ///     The zero-based offset in the <paramref name="buffer" /> at which to begin storing the data
        ///     read from the current stream.
        /// </param>
        /// <param name="count">The maximum number of samples to read from the current source.</param>
        /// <returns>The total number of samples read into the buffer.</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int read = _sampleProvider.Read(buffer, offset, count);

            for (int c = 0; c < WaveFormat.Channels; c++)
            {
                for (int i = _equalizerFilters.Count; i-- > 0;)
                {
                    _equalizerFilters[i].Filters[c].Process(buffer, offset, read, c, WaveFormat.Channels);
                }
            }

            for (int n = offset; n < count; n++)
            {
                buffer[n] = Math.Max(-1, Math.Min(buffer[n], 1));
            }

            return read;
        }

        private class EqualizerFilterCollection : IList<EqualizerFilter>
        {
            private readonly List<EqualizerFilter> _list = new();

            public IEnumerator<EqualizerFilter> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(EqualizerFilter item)
            {
                _list.Add(item);
                _list.Sort();
            }

            public void Clear()
            {
                _list.Clear();
            }

            public bool Contains(EqualizerFilter item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(EqualizerFilter[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public bool Remove(EqualizerFilter item)
            {
                return _list.Remove(item);
            }

            public int Count => _list.Count;

            public bool IsReadOnly => false;

            public int IndexOf(EqualizerFilter item)
            {
                return _list.IndexOf(item);
            }

            public void Insert(int index, EqualizerFilter item)
            {
                _list.Insert(index, item);
                _list.Sort();
            }

            public void RemoveAt(int index)
            {
                _list.RemoveAt(index);
            }

            public EqualizerFilter this[int index]
            {
                get => _list[index];
                set
                {
                    _list[index] = value;
                    _list.Sort();
                }
            }
        }
    }
}
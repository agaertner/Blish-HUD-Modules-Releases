using System;
using Nekres.Music_Mixer.Core.Player.Source.DSP;

namespace Nekres.Music_Mixer.Core.Player.Source.Equalizer
{
    /// <summary>
    ///     Represents an EqualizerFilter for a single channel.
    /// </summary>
    public class EqualizerChannelFilter : ICloneable
    {
        private readonly PeakFilter _biQuadFilter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EqualizerChannelFilter" /> class.
        /// </summary>
        /// <param name="sampleRate">The sampleRate of the audio data to process.</param>
        /// <param name="centerFrequency">The center frequency to adjust.</param>
        /// <param name="bandWidth">The bandWidth.</param>
        /// <param name="gain">The gain value in dB.</param>
        public EqualizerChannelFilter(int sampleRate, double centerFrequency, double bandWidth, double gain)
        {
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate));
            if (centerFrequency <= 0)
                throw new ArgumentOutOfRangeException(nameof(centerFrequency));
            if (bandWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(bandWidth));

            _biQuadFilter = new PeakFilter(sampleRate, centerFrequency, bandWidth, gain);
        }

        /// <summary>
        ///     Gets or sets the gain value in dB.
        /// </summary>
        public double GainDB
        {
            get => _biQuadFilter.GainDB;
            set => _biQuadFilter.GainDB = value;
        }

        /// <summary>
        ///     Gets or sets the bandwidth.
        /// </summary>
        public double BandWidth
        {
            get => _biQuadFilter.BandWidth;
            set => _biQuadFilter.BandWidth = value;
        }

        /// <summary>
        ///     Gets the frequency.
        /// </summary>
        public double Frequency => _biQuadFilter.Frequency;

        /// <summary>
        ///     Gets the samplerate.
        /// </summary>
        public int SampleRate => _biQuadFilter.SampleRate;


        /// <summary>
        ///     Returns a copy of the <see cref="EqualizerChannelFilter" />.
        /// </summary>
        /// <returns>A copy of the <see cref="EqualizerChannelFilter" /></returns>
        public object Clone()
        {
            return new EqualizerChannelFilter(SampleRate, Frequency, BandWidth, GainDB);
        }

        /// <summary>
        ///     Processes an array of input samples.
        /// </summary>
        /// <param name="input">The input samples to process.</param>
        /// <param name="offset">The zero-based offset in the <paramref name="input" /> buffer to start at.</param>
        /// <param name="count">The number of samples to process.</param>
        /// <param name="channelIndex">Specifies the channel to process as a zero-based index.</param>
        /// <param name="channelCount">The total number of channels.</param>
        public void Process(float[] input, int offset, int count, int channelIndex, int channelCount)
        {
            for (int i = channelIndex + offset; i < count + offset; i += channelCount)
            {
                input[i] = _biQuadFilter.Process(input[i]);
            }
        }
    }
}

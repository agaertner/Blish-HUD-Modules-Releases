using System;

namespace Nekres.Music_Mixer.Core.Player.Source.DSP
{
    /// <summary>
    /// Used to apply a lowpass-filter to a signal.
    /// </summary>
    public class LowPassFilter : BiQuad
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LowPassFilter"/> class.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="frequency">The filter's corner frequency.</param>
        public LowPassFilter(int sampleRate, double frequency)
            : base(sampleRate, frequency)
        {
        }

        /// <summary>
        /// Calculates all coefficients.
        /// </summary>
        protected override void CalculateBiQuadCoefficients()
        {
            double k = Math.Tan(Math.PI * Frequency / SampleRate);
            var norm = 1 / (1 + k / Q + k * k);
            A0 = k * k * norm;
            A1 = 2 * A0;
            A2 = A0;
            B1 = 2 * (k * k - 1) * norm;
            B2 = (1 - k / Q + k * k) * norm;
        }
    }
}

using NAudio.Wave;
using System;

namespace Nekres.Music_Mixer
{
    public static class WaveStreamExtensions
    {
        /// <summary>
        /// Set position of WaveStream to nearest block to supplied position
        /// </summary>
        public static void SetPosition(this WaveStream strm, long position)
        {
            // distance from block boundary (may be 0)
            long adj = position % strm.WaveFormat.BlockAlign;
            // adjust position to boundary and clamp to valid range
            long newPos = Math.Max(0, Math.Min(strm.Length, position - adj));
            // set playback position
            strm.Position = newPos;
        }

        /// <summary>
        /// Set playback position of WaveStream by seconds
        /// </summary>
        public static void SetPosition(this WaveStream strm, double seconds)
        {
            strm.SetPosition((long)(seconds * strm.WaveFormat.AverageBytesPerSecond));
        }

        /// <summary>
        /// Set playback position of WaveStream by time (as a TimeSpan)
        /// </summary>
        public static void SetPosition(this WaveStream strm, TimeSpan time)
        {
            strm.SetPosition(time.TotalSeconds);
        }

        /// <summary>
        /// Set playback position of WaveStream relative to current position
        /// </summary>
        public static void Seek(this WaveStream strm, double offset)
        {
            strm.SetPosition(strm.Position + (long)(offset * strm.WaveFormat.AverageBytesPerSecond));
        }
    }
}

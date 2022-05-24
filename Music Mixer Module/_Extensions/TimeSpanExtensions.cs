using System;

namespace Nekres.Music_Mixer
{
    internal static class TimeSpanExtensions
    {
        public static string ToShortForm(this TimeSpan t)
        {
            return (t.Hours > 0 ? $"{t.Hours}:" : string.Empty) + t.ToString(t.Minutes > 9 ? "mm\\:ss" : "m\\:ss");
        }
    }
}

using System;

namespace Nekres.Music_Mixer
{
    internal static class TimeSpanExtensions
    {
        public static string ToShortForm(this TimeSpan t)
        {
            var shortForm = string.Empty;
            if (t.Hours > 0)
            {
                shortForm += $"{t.Hours}:";
            }
            if (t.Minutes > 0)
            {
                shortForm += $"{t.Minutes}:";
            }
            if (t.Seconds > 0)
            {
                shortForm += $"{t.Seconds}";
            }
            return shortForm;
        }
    }
}

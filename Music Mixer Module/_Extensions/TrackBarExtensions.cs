using System;
using Blish_HUD.Controls;
using Nekres.Music_Mixer.Core.UI.Controls;

namespace Nekres.Music_Mixer
{
    internal static class TrackBarExtensions
    {
        public static void RefreshValue(this TrackBar volumeTrackBar, float value)
        {
            volumeTrackBar.MinValue = Math.Min(volumeTrackBar.MinValue, value);
            volumeTrackBar.MaxValue = Math.Max(volumeTrackBar.MaxValue, value);

            volumeTrackBar.Value = value;
        }

        public static void RefreshValue(this TrackBar2 volumeTrackBar, float value)
        {
            volumeTrackBar.MinValue = Math.Min(volumeTrackBar.MinValue, value);
            volumeTrackBar.MaxValue = Math.Max(volumeTrackBar.MaxValue, value);

            volumeTrackBar.Value = value;
        }
    }
}

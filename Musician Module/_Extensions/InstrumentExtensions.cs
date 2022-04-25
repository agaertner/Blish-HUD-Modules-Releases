using Microsoft.Xna.Framework.Graphics;
using Nekres.Musician.Core.Models;
using System.Collections.Generic;

namespace Nekres.Musician
{
    internal static class InstrumentExtensions
    {
        private static Dictionary<Instrument, Texture2D> _iconCache = new ();
        public static Texture2D GetIcon(this Instrument instrument)
        {
            if (_iconCache.TryGetValue(instrument, out var icon))
                return icon;
            icon = MusicianModule.ModuleInstance.ContentsManager.GetTexture($@"instruments\{instrument.ToString().ToLowerInvariant()}.png");
            _iconCache.Add(instrument, icon);
            return icon;
        }
    }
}

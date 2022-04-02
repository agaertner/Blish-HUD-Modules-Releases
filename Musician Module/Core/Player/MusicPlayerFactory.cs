using System;
using Nekres.Musician.Core.Instrument;
using Nekres.Musician.Core.Instrument.Bass;
using Nekres.Musician.Core.Instrument.Bell;
using Nekres.Musician.Core.Instrument.Bell2;
using Nekres.Musician.Core.Instrument.Flute;
using Nekres.Musician.Core.Instrument.Harp;
using Nekres.Musician.Core.Instrument.Horn;
using Nekres.Musician.Core.Instrument.Lute;
using Nekres.Musician.Core.Models;
using Nekres.Musician.Core.Player.Algorithms;
namespace Nekres.Musician.Core.Player
{
    internal static class MusicPlayerFactory
    {
        internal static MusicPlayer Create(MusicSheet musicSheet, InstrumentMode mode)
        {
            return MusicBoxNotationMusicPlayerFactory(musicSheet, mode);
        }


        private static MusicPlayer MusicBoxNotationMusicPlayerFactory(MusicSheet musicSheet)
        {
            var algorithm = musicSheet.Algorithm == Algorithm.FavorNotes ? new FavorNotesAlgorithm() : (IPlayAlgorithm)new FavorChordsAlgorithm();

            switch (musicSheet.Instrument)
            {
                case Models.Instrument.Bass:
                    return new MusicPlayer(musicSheet, new Bass(), algorithm);
                case Models.Instrument.Bell:
                    return new MusicPlayer(musicSheet, new Bell(), algorithm);
                case Models.Instrument.Bell2:
                    return new MusicPlayer(musicSheet, new Bell2(), algorithm);
                case Models.Instrument.Flute:
                    return new MusicPlayer(musicSheet, new Flute(), algorithm);
                case Models.Instrument.Harp:
                    return new MusicPlayer(musicSheet, new Harp(), algorithm);
                case Models.Instrument.Horn:
                    return new MusicPlayer(musicSheet, new Horn(), algorithm);
                case Models.Instrument.Lute:
                    return new MusicPlayer(musicSheet, new Lute(), algorithm);
            }
        }
    }
}
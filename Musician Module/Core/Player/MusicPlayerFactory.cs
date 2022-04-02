using System;
using Nekres.Musician.Core.Models;
using Nekres.Musician.Core.Player.Algorithms;
using Nekres.Musician_Module;
using Nekres.Musician_Module.Controls.Instrument;

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
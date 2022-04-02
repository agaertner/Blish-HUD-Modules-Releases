using System.Collections.Generic;
using Nekres.Musician.Core.Domain;

namespace Nekres.Musician_Module.Domain
{
    public class MusicSheet
    {
        public MusicSheet(string title, string instrument, Metronome metronomeMark, IEnumerable<ChordOffset> melody)
        {
            MetronomeMark = metronomeMark;
            Melody = melody;
            Instrument = instrument;
            Title = title;
        }

        public string Artist { get; }

        public string Title { get; }

        public string User { get; }

        public string Instrument { get; }

        public Metronome MetronomeMark { get; }

        public IEnumerable<ChordOffset> Melody { get; }
    }
}
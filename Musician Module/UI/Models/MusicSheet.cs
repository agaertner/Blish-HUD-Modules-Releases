﻿using System.Collections.Generic;
using Nekres.Musician_Module.Domain.Values;

namespace Nekres.Musician_Module.Domain
{
    public class MusicSheet
    {
        public MusicSheet(string title, string instrument, MetronomeMark metronomeMark, IEnumerable<ChordOffset> melody)
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

        public MetronomeMark MetronomeMark { get; }

        public IEnumerable<ChordOffset> Melody { get; }
    }
}
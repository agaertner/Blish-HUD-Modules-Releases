using System;
using Blish_HUD;

namespace Nekres.Musician.Core.Domain
{
    public class Metronome
    {
        public int Tempo { get; }
        public Fraction BeatsPerMeasure { get; }
        public TimeSpan QuaterNoteLength { get; }
        public TimeSpan WholeNoteLength { get; }

        public Metronome(int tempo, Fraction beatsPerMeasure)
        {
            BeatsPerMeasure = beatsPerMeasure;
            Tempo = tempo;

            QuaterNoteLength = TimeSpan.FromMinutes(1)
                .Divide(tempo*16/beatsPerMeasure.Denominator);

            WholeNoteLength = TimeSpan.FromMinutes(1)
                .Divide(tempo*16/beatsPerMeasure.Denominator)
                .Multiply(4);
        }
    }
}
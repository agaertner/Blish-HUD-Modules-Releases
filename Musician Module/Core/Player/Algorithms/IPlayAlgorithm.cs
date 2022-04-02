using Nekres.Musician.Core.Domain;

namespace Nekres.Musician.Core.Player.Algorithms
{
    public interface IPlayAlgorithm
    {
        void Play(Instrument.Instrument instrument, Metronome metronomeMark, ChordOffset[] melody);
        void Dispose();
    }
}
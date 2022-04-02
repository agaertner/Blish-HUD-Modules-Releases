namespace Nekres.Musician.Core.Domain
{
    public class ChordOffset
    {
        public ChordOffset(Chord chord, Beat offset)
        {
            Chord = chord;
            Offset = offset;
        }

        public Chord Chord { get; }
        public Beat Offset { get; }

        public string Serialize()
        {
            return Chord.Serialize();
        }
    }
}
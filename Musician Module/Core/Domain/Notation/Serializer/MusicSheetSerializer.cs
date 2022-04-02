﻿using System.Text;
using Nekres.Musician_Module.Domain;
using Nekres.Musician_Module.Notation.Persistance;

namespace Nekres.Musician_Module.Notation.Serializer
{
    public class MusicSheetSerializer
    {
        private readonly ChordOffsetSerializer _chordOffsetSerializer;

        public MusicSheetSerializer(ChordOffsetSerializer chordOffsetSerializer)
        {
            _chordOffsetSerializer = chordOffsetSerializer;
        }

        public RawMusicSheet Serialize(MusicSheet musicSheet)
        {
            var artist = musicSheet.Artist;
            var title = musicSheet.Title;
            var user = musicSheet.User;
            var instrument = musicSheet.Instrument;
            var tempo = SerializeTempo(musicSheet);
            var meter = SerializeMeter(musicSheet);
            var melody = SerializeMelody(musicSheet);
            var algorithm = SerializeAlgorithm();

            return new RawMusicSheet(artist, title, user, instrument, tempo, meter, melody, algorithm);
        }

        private static string SerializeTempo(MusicSheet musicSheet)
        {
            return musicSheet.MetronomeMark.Metronome.ToString();
        }

        private static string SerializeMeter(MusicSheet musicSheet)
        {
            var beatsPerMeasure = musicSheet.MetronomeMark.BeatsPerMeasure;
            return $"{beatsPerMeasure.Nominator}/{beatsPerMeasure.Denominator}";
        }

        private string SerializeMelody(MusicSheet musicSheet)
        {
            var stringBuilder = new StringBuilder();

            foreach (var chordOffset in musicSheet.Melody)
            {
                stringBuilder.Append(" ");
                stringBuilder.Append(_chordOffsetSerializer.Serialize(chordOffset));
            }

            return stringBuilder.ToString();
        }

        private static string SerializeAlgorithm()
        {
            return "favor chords";
        }
    }
}
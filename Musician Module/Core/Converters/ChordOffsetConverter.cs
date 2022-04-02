using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Nekres.Musician.Core.Domain.Converters
{
    internal class ChordOffsetConverter : JsonConverter<IEnumerable<ChordOffset>>
    {
        private static readonly Regex NonWhitespace = new Regex(@"[^\s]+");
        public override void WriteJson(JsonWriter writer, IEnumerable<ChordOffset> value, JsonSerializer serializer)
        {
            var stringBuilder = new StringBuilder();

            foreach (var chordOffset in value)
            {
                stringBuilder.Append(" ");
                stringBuilder.Append(chordOffset.Serialize());
            }
            writer.WriteValue(stringBuilder.ToString());
        }

        public override IEnumerable<ChordOffset> ReadJson(JsonReader reader, Type objectType, IEnumerable<ChordOffset> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return null;
            var val = (string)reader.Value;

            var currentBeat = 0m;

            return NonWhitespace.Matches(val).Cast<Match>().Select(textChord =>
                {
                    var chord = Chord.Parse(textChord.Value);

                    var chordOffset = new ChordOffset(chord, new Beat(currentBeat));

                    currentBeat += chord.Length;

                    return chordOffset;
                });
        }
    }
}

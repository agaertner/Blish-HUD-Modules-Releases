using System;
using Newtonsoft.Json;

namespace Nekres.Musician.Core.Domain
{
    public class MetronomeConverter : JsonConverter<Metronome>
    {
        public override void WriteJson(JsonWriter writer, Metronome value, JsonSerializer serializer)
        {
            writer.WriteValue($"{value.Tempo} {value.BeatsPerMeasure.Nominator}/{value.BeatsPerMeasure.Denominator}");
        }

        public override Metronome ReadJson(JsonReader reader, Type objectType, Metronome existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return null;
            var val = ((string)reader.Value).Split(' ');
            if (val.Length < 2) return null;
            var fraction = val[1].Split('/');
            return new Metronome(int.Parse(val[0]), new Fraction(int.Parse(fraction[0]), int.Parse(fraction[1])));
        }
    }
}
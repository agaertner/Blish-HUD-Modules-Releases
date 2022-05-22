using Newtonsoft.Json;
using System;

namespace Nekres.Music_Mixer
{
    internal class TimeSpanFromSecondsConverter : JsonConverter<TimeSpan>
    {
        public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return TimeSpan.Zero;
            if (reader.Value is string val && long.TryParse(val, out var time))
                return TimeSpan.FromSeconds(time);
            return TimeSpan.FromSeconds((long)reader.Value);
        }
    }
}

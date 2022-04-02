using System.Collections.Generic;
using System.Runtime.Serialization;
using Nekres.Musician.Core.Domain;
using Nekres.Musician.Core.Domain.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nekres.Musician.Core.Models
{
    public enum Algorithm
    {
        [EnumMember(Value = "favor-notes")]
        FavorNotes,
        [EnumMember(Value = "favor-chords")]
        FavorChords
    }

    public enum Instrument
    {
        [EnumMember(Value = "bass")]
        Bass,
        [EnumMember(Value = "bell")]
        Bell,
        [EnumMember(Value = "bell2")]
        Bell2,
        [EnumMember(Value = "flute")]
        Flute,
        [EnumMember(Value = "harp")]
        Harp,
        [EnumMember(Value = "horn")]
        Horn,
        [EnumMember(Value = "lute")]
        Lute
    }

    public class MusicSheet
    {
        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("instrument"), JsonConverter(typeof(StringEnumConverter))]
        public Instrument Instrument { get; set; }

        [JsonProperty("tempo"), JsonConverter(typeof(MetronomeConverter))]
        public Metronome Tempo { get; set; }

        [JsonProperty("algorithm"), JsonConverter(typeof(StringEnumConverter))]
        public Algorithm Algorithm { get; set; }

        [JsonProperty("melody"), JsonConverter(typeof(ChordOffsetConverter))]
        public IEnumerable<ChordOffset> Melody { get; set; }
    }
}
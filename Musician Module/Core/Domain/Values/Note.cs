using System;
using System.Linq;

namespace Nekres.Musician_Module.Domain.Values
{
    public enum Keys
    {
        C,
        D,
        E,
        F,
        G,
        A,
        B
    }
    public enum Octaves
    {
        None,
        Lowest,
        Low,
        Middle,
        High,
        Highest
    }

    public class Note
    {
        public Keys Key { get; }
        public Octaves Octave { get; }

        public Note(Keys key, Octaves octave)
        {
            Key = key;
            Octave = octave;
        }

        public override string ToString()
        {
            return $"{(Octave >= Octaves.High ? "▲" : Octave <= Octaves.Low ? "▼" : string.Empty)}{Key}";
        }

        public override bool Equals(object obj) => Equals((Note)obj);
        protected bool Equals(Note other) => Key == other.Key && Octave == other.Octave;

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Key*397) ^ (int)Octave;
            }
        }

        public static Note FromString(string str)
        {
            if (string.IsNullOrEmpty(str) || !Enum.TryParse<Keys>(str[0].ToString(), true, out var key))
                throw new FormatException("Provided string is not valid.");
            return new Note(key, ParseOctave(str));
        }

        private static Octaves ParseOctave(string text)
        {
            var lol = Enum.GetValues(typeof(Keys)).Cast<string>();
            if (string.IsNullOrEmpty(text) || Array.Exists(Enum.GetValues(typeof(Keys)).Cast<Keys>().ToArray(), k => char.Parse(k.ToString()).Equals(char.ToUpperInvariant(text[0])))) 
                return Octaves.None;

            if (text.Length == 1)
                return char.IsUpper(text[0]) ? Octaves.Lowest : Octaves.Low;

            var octaveMarks = text.Substring(1, text.Length);

            if (octaveMarks[0] == ',')
            {
                switch (octaveMarks.Length)
                {
                    case 1: return Octaves.Lowest;
                    case 2: return Octaves.Low;
                    case 3: return Octaves.Middle;
                    default: return Octaves.None;
                }
            }

            if (octaveMarks[0] != '\'') return Octaves.None;

            switch (octaveMarks.Length)
            {
                case 1: return Octaves.Middle;
                case 2: return Octaves.High;
                default: return Octaves.Highest;
            }
        }
    }
}
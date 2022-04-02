using System;
using System.Linq;

namespace Nekres.Musician.Core.Domain
{
    public enum Key
    {
        Z,
        C,
        D,
        E,
        F,
        G,
        A,
        B
    }
    public enum Octave
    {
        Lowest, // C,,,
        Lower,  // C,,
        Low,    // C,
        Minor,  // C
        Major,  // c
        High,   // c'
        Higher, // c''
        Highest // c'''
    }

    public class Note
    {
        public Key Key { get; }
        public Octave Octave { get; }

        private Note(Key key, Octave octave)
        {
            Key = key;
            Octave = octave;
        }

        public override string ToString()
        {
            return $"{(Octave >= Octave.Minor ? "▲" : Octave <= Octave.Major ? "▼" : string.Empty)}{Key}";
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

        public string Serialize()
        {
            if (Key == Key.Z)
                return "z";

            var result = Key.ToString();

            switch (Octave)
            {
                case Octave.Lowest:
                    result += ",,,";
                    break;
                case Octave.Lower:
                    result += ",,";
                    break;
                case Octave.Low:
                    result += ",";
                    break;
                case Octave.Major:
                    result = result.ToLowerInvariant();
                    break;
                case Octave.High:
                    result = $"{result.ToLowerInvariant()}'";
                    break;
                case Octave.Higher:
                    result = $"{result.ToLowerInvariant()}''";
                    break;
                case Octave.Highest:
                    result = $"{result.ToLowerInvariant()}'''";
                    break;
                default: break;
            }
            return result;
        }

        public static Note Deserialize(string str)
        {
            if (string.IsNullOrEmpty(str) || !Enum.TryParse<Key>(str[0].ToString(), true, out var key) 
                                          || !TryParseOctave(str, out var octave))
                throw new FormatException("Provided string is not valid.");
            return new Note(key, octave);
        }

        private static bool TryParseOctave(string text, out Octave octave)
        {
            octave = 0;
            if (string.IsNullOrEmpty(text) || !Array.Exists(Enum.GetValues(typeof(Key)).Cast<Key>().ToArray(), k => char.Parse(k.ToString()).Equals(char.ToUpperInvariant(text[0])))) 
                return false;

            if (text.Length == 1)
            {
                octave = char.IsUpper(text[0]) ? Octave.Minor : Octave.Major;
                return true;
            }

            var octaveMarks = text.Substring(1, text.Length);

            switch (octaveMarks[0])
            {
                case ',':
                    switch (octaveMarks.Length)
                    {
                        case 3: octave = Octave.Lowest; break;
                        case 2: octave = Octave.Lower; break;
                        case 1: octave = Octave.Low; break;
                    }
                    return true;
                case '\'':
                    switch (octaveMarks.Length)
                    {
                        case 3: octave = Octave.Highest; break;
                        case 2: octave = Octave.Higher; break;
                        case 1: octave = Octave.High; break;
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
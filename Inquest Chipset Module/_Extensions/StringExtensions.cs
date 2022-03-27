using System;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.Inquest_Module
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Finds the closest matching string in the list.
        /// </summary>
        /// <param name="input">the string to find a closest match for.</param>
        /// <param name="list">the list to search in.</param>
        /// <returns></returns>
        public static string FindClosestMatch(this string input, IEnumerable<string> list)
        {
            if (input.Length == 0) return input;

            var result = string.Empty;

            //The value here is a purely nonsensical high value and serves no other purpose
            var minScore = 20000;
            var lev = new Fastenshtein.Levenshtein(input);

            foreach (var element in list)
            {
                var score = lev.DistanceFrom(element);
                if (score >= minScore) continue;
                minScore = score;
                result = element;
            }
            return result;
        }

        public static bool Contains(this string input, string expected)
        {
            var lev = new Fastenshtein.Levenshtein(input);
            return lev.DistanceFrom(input) < 2;
        }

        public static IEnumerable<string> GetNumbers(this string input)
        {
            return new string(input.Select(c => char.IsDigit(c) ? c : ' ').ToArray()).Split(' ').Select(s => s.Trim());
        }
    }
}

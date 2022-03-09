using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Drawing;
namespace Nekres.Regions_Of_Tyria
{
    public static class StringExtensions
    {
        public static IReadOnlyList<ValueTuple<Color, string>> FetchMarkupColoredText(this string input, Color? regularTextColor = null)
        {
            if (!regularTextColor.HasValue)
                regularTextColor = Color.White;

            if (string.IsNullOrEmpty(input)) 
                return new List<ValueTuple<Color, string>>{ new ValueTuple<Color, string>(regularTextColor.Value, input) };

            var colorRegex = new Regex(@"<(c|color)=(#?((?'rgb'[a-fA-F0-9]{6})|(?'argb'[a-fA-F0-9]{8})))?>(?'text'.*?)<\s*\/\s*\1\s*>", RegexOptions.Multiline);

            var lines = new List<ValueTuple<Color, string>>();
            var startIndex = 0;
            foreach (Match m in colorRegex.Matches(input))
            {
                // Current match is not starting at the end of the last match which means there is non-captured text between.
                if (startIndex != m.Index)
                    lines.Add(new ValueTuple<Color, string>(regularTextColor.Value, input.Substring(startIndex, m.Index - startIndex)));

                startIndex = m.Index + m.Length;

                var color = Color.FromArgb(int.Parse(m.Groups["rgb"].Success ? "FF" + m.Groups["rgb"].Value : m.Groups["argb"].Value, NumberStyles.HexNumber));
                lines.Add(new ValueTuple<Color, string>(color, m.Groups["text"].Value));
            }

            // String does not end with the final match which means there is non-captured text remaining.
            if (startIndex != input.Length)
                lines.Add(new ValueTuple<Color, string>(regularTextColor.Value, input.Substring(startIndex, input.Length - startIndex)));

            return lines;
        }

        public static string StripMarkupLazy(string input)
        {
            if (string.IsNullOrEmpty(input)) 
                return input;

            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        public static string StripMarkup(string input)
        {
            if (string.IsNullOrEmpty(input)) 
                return input;

            var matchingTags = new Regex("<\\s*([^ >]+)[^>]*>(?'text'.*?)<\\s*\\/\\s*\\1\\s*>", RegexOptions.Multiline);
            while (matchingTags.IsMatch(input))
            {
                var match = matchingTags.Match(input);
                var text = match.Groups["text"].Value;
                input = input.Replace(match.Value, text);
            }
            return input;
        }
    }
}

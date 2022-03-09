using System;
using System.Linq;
namespace Nekres.Stream_Out
{
    public static class IntegerExtensions
    {
        public static string ToRomanNumeral(this int number)
        {
            number = Math.Abs(number);

            // 1-3999
            var romanNumerals = new[,] {
                {"", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX"},
                {"", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC"},
                {"", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM"},
                {"", "M", "MM", "MMM", "", "", "", "", "", ""}
            };

            var intArr = number.ToString().ToCharArray().Reverse().ToArray();
            var len = intArr.Length - 1;
            var romanNumeral = "";
            var i = len;

            while (i >= 0)
            {
                romanNumeral += romanNumerals[i, int.Parse(intArr[i].ToString())];
                i--;
            }
            return romanNumeral;
        }

        public static bool InRange(this int number, int[] range)
        {
            return number >= range.Min() && number <= range.Max();
        }
    }
}

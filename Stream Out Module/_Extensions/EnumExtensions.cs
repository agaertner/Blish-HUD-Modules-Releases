using System;
using System.Text.RegularExpressions;
namespace Nekres.Stream_Out
{
    public static class EnumExtensions
    {
        public static string ToDisplayString(this Enum _enum)
        {
            return Regex.Replace(_enum.ToString(), "([A-Z]|[1-9])", " $1", RegexOptions.Compiled).Trim();
        }
    }
}

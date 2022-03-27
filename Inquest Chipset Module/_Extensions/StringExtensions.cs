namespace Nekres.Inquest_Module
{
    internal static class StringExtensions
    {
        public static bool IsDigitsOnly(this string str)
        {
            if (string.IsNullOrEmpty(str) || str[0] == '0') return false;

            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }
    }
}

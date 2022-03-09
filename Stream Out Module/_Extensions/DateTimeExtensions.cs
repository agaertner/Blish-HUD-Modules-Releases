using System;
namespace Nekres.Stream_Out
{
    public static class DateTimeExtensions
    {
        public static bool IsBetween(this DateTime time, DateTime start, DateTime end)
        {
            if (start < end)
                return start <= time && time <= end;
            return !(end < time && time < start);
        }
    }
}

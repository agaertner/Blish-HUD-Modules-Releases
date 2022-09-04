using System;
using System.Linq;

namespace Nekres.Mumble_Info
{
    internal static class BitmaskUtil
    {
        public static uint GetBitmask(params bool[] bits)
        {
            return (uint)bits.Select((b, i) => b ? 1 << i : 0).Aggregate((a, b) => a | b);
        }

        public static bool[] GetBooleans(uint mask)
        {
            return mask.ToString().ToCharArray().Select(Convert.ToInt32).Select(b => (mask & (1 << b)) != 0).ToArray();
        }
    }
}

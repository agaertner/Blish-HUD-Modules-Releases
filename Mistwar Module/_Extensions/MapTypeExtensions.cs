using MapType = Gw2Sharp.Models.MapType;
namespace Nekres.Mistwar
{
    internal static class MapTypeExtensions
    {
        public static bool IsWorldVsWorld(this MapType type)
        {
            switch (type)
            {
                case MapType.Center:
                case MapType.BlueHome:
                case MapType.GreenHome:
                case MapType.RedHome:
                    return true;
                default: return false;
            }
        }
    }
}

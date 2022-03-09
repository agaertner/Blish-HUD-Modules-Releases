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
                case MapType.EdgeOfTheMists: return true;
                default: return false;
            }
        }
    }
}

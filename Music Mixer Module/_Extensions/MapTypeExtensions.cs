using Gw2Sharp.Models;

namespace Nekres.Music_Mixer
{
    internal static class MapTypeExtensions
    {
        public static bool IsWvW(this MapType type)
        {
            switch (type) {
                case MapType.Center:
                case MapType.BlueHome:
                case MapType.GreenHome:
                case MapType.RedHome:
                case MapType.JumpPuzzle:
                case MapType.EdgeOfTheMists:
                case MapType.WvwLounge:
                    return true;
                default: return false;
            }
        }

        public static bool IsInstance(this MapType type)
        {
            switch (type)
            {
                case MapType.Tutorial:
                case MapType.Instance:
                case MapType.FortunesVale:
                    return true;
                default: return false;
            }
        }

        public static bool IsPublic(this MapType type)
        {
            switch (type)
            {
                case MapType.PublicMini:
                case MapType.Public:
                    return true;
                default: return false;
            }
        }

        public static bool IsTournament(this MapType type)
        {
            switch (type)
            {
                case MapType.Tournament:
                case MapType.UserTournament:
                    return true;
                default: return false;
            }
        }

        public static bool IsPvP(this MapType type)
        {
            switch (type)
            {
                case MapType.Pvp:
                case MapType.Gvg:
                    return true;
                default: return IsTournament(type);
            }
        }
    }
}

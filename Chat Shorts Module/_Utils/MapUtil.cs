using Blish_HUD;
using Gw2Sharp.Models;
using Nekres.Chat_Shorts.UI.Models;

namespace Nekres.Chat_Shorts
{
    internal static class MapUtil
    {
        public static GameMode GetCurrentGameMode()
        {
            switch (GameService.Gw2Mumble.CurrentMap.Type)
            {
                case MapType.Pvp:
                case MapType.Gvg:
                case MapType.Tournament:
                case MapType.UserTournament:
                case MapType.EdgeOfTheMists:
                    return GameMode.PvP;
                case MapType.Instance:
                case MapType.Public:
                case MapType.Tutorial:
                case MapType.PublicMini:
                    return GameService.Gw2Mumble.CurrentMap.Id == 350 ? GameMode.PvP : GameMode.PvE; // Heart of the Mists
                case MapType.Center:
                case MapType.BlueHome:
                case MapType.GreenHome:
                case MapType.RedHome:
                case MapType.WvwLounge:
                case MapType.JumpPuzzle:
                    return GameMode.WvW;
                default: return GameMode.All;
            }
        }
    }
}

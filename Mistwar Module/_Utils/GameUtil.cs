using Blish_HUD;

namespace Nekres.Mistwar
{
    internal static class GameUtil
    {
        public static bool IsUiAvailable()
        {
            return GameService.Gw2Mumble.IsAvailable && GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen;
        }
    }
}

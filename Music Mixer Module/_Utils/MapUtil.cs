using Blish_HUD;
using Gw2Sharp.WebApi.Exceptions;
using Gw2Sharp.WebApi.V2.Models;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer
{
    internal static class MapUtil
    {
        public static async Task<int> GetRegion(int mapId)
        {
            if (mapId <= 0) return 0;
            try
            {
                return await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Maps.GetAsync(mapId)
                    .ContinueWith(t => t.Result.RegionId);
            }
            catch (RequestException e)
            {
                MusicMixer.Logger.Error(e, e.Message);
                return -1;
            }
        }

        public static async Task<string> GetSHA1(int mapId)
        {
            if (mapId <= 0) return string.Empty;
            try
            {
                return await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Maps.GetAsync(mapId)
                    .ContinueWith(t => GetSHA1(t.Result.ContinentId, t.Result.ContinentRect));
            }
            catch (RequestException e)
            {
                MusicMixer.Logger.Error(e, e.Message);
                return string.Empty;
            }
        }

        public static string GetSHA1(int continentId, Rectangle continentRect)
        {
            var rpcHash = $"{continentId}{continentRect.TopLeft.X}{continentRect.TopLeft.Y}{continentRect.BottomRight.X}{continentRect.BottomRight.Y}";
            return rpcHash.ToSHA1Hash().Substring(0, 8);

        }
    }
}

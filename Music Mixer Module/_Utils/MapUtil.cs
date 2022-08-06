using System.Threading.Tasks;
using Blish_HUD;
using Gw2Sharp.WebApi.Exceptions;

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
    }
}

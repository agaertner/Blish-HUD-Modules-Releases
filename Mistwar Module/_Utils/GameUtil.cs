using Blish_HUD;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.Mistwar
{
    internal static class GameUtil
    {
        public static bool IsAvailable()
        {
            return GameService.Gw2Mumble.IsAvailable && GameService.GameIntegration.Gw2Instance.IsInGame
                                                      && !GameService.Gw2Mumble.UI.IsMapOpen;
        }

        private static readonly IReadOnlyList<int> EmergencyWayPoints = new List<int>
        {
            // Desert Borderlands (1099)
            2244,
            2100,
            2195,
            2091,
            2209,
            2248,
            2207,
            // Alpine Borderlands (95)
            2347,
            2337,
            2350,
            2325,
            2343,
            2338,
            2345,
            // Alpine Borderlands (96)
            2351,
            2322,
            2328,
            2324,
            2339,
            2348,
            2341,
            // Eternal Battlegrounds (38)
            2228,
            2293,
            2243,
            2252,
            2109,
            2263,
            2145,
            2280,
            2154,
            2103,
            2267,
            2253,
            2275,
            2217,
            2148,
            2134
        };

        public static bool IsEmergencyWayPoint(ContinentFloorRegionMapPoi waypoint)
        {
            return waypoint.Type == PoiType.Waypoint && EmergencyWayPoints.Contains(waypoint.Id);
        }
    }
}

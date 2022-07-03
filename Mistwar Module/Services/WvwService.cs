using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Nekres.Mistwar.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Mistwar.Services
{
    internal class WvwService : IDisposable
    {
        private Gw2ApiManager _api;

        private Dictionary<int, IEnumerable<WvwObjectiveEntity>> _wvwObjectiveCache;

        private DateTime _prevApiRequestTime = DateTime.UtcNow;

        public WvwService(Gw2ApiManager api)
        {
            _api = api;
            _wvwObjectiveCache = new Dictionary<int, IEnumerable<WvwObjectiveEntity>>();
        }

        public async Task Update()
        {
            if (!GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld() || !_wvwObjectiveCache.ContainsKey(GameService.Gw2Mumble.CurrentMap.Id)) return;

            if (DateTime.UtcNow.Subtract(_prevApiRequestTime).TotalSeconds < 15)
                return;
            _prevApiRequestTime = DateTime.UtcNow;

            var worldId = await GetWorldId();
            if (worldId == -1) return;

            foreach (var id in await GetWvWMapIds(worldId))
            {
                await UpdateObjectives(worldId, id);
            }
        }

        public async Task<int> GetWorldId()
        {
            return await _api.Gw2ApiClient.V2.Account.GetAsync().ContinueWith(task =>
            {
                if (task.IsFaulted) return -1;
                return task.Result.World;
            });
        }

        public async Task<int[]> GetWvWMapIds(int worldId)
        {
            return await _api.Gw2ApiClient.V2.Wvw.Matches.World(worldId).GetAsync().ContinueWith(t =>
            {
                if (t.IsFaulted) return Array.Empty<int>();
                return t.Result.Maps.Select(m => m.Id).ToArray();
            });
        }

        private async Task UpdateObjectives(int worldId, int mapId)
        {
            var objEntities = await GetObjectives(mapId);
            var objectives = await RequestObjectives(worldId, GameService.Gw2Mumble.CurrentMap.Id);
            if (objectives.IsNullOrEmpty()) return;

            foreach (var objEntity in objEntities)
            {
                var obj = objectives.FirstOrDefault(v => v.Id.Equals(objEntity.Id, StringComparison.InvariantCultureIgnoreCase));
                if (obj == null) continue;
                objEntity.LastFlipped = obj.LastFlipped ?? DateTime.MinValue;
                objEntity.Owner = obj.Owner.Value;
                objEntity.ClaimedBy = obj.ClaimedBy ?? Guid.Empty;
                objEntity.GuildUpgrades = obj.GuildUpgrades;
                objEntity.YaksDelivered = obj.YaksDelivered ?? 0;
            }
        }

        private async Task<IReadOnlyList<WvwMatchMapObjective>> RequestObjectives(int worldId, int mapId)
        {
            return await _api.Gw2ApiClient.V2.Wvw.Matches.World(worldId).GetAsync()
                .ContinueWith(task =>
                {
                    if (task.IsFaulted) return null;
                    return task.Result.Maps.FirstOrDefault(x => x.Id == mapId)?.Objectives;
                });
        }

        public async Task<IEnumerable<WvwObjectiveEntity>> GetObjectives(int mapId)
        {
            if (_wvwObjectiveCache.TryGetValue(mapId, out var objEntities))
            {
                return objEntities;
            }

            return await _api.Gw2ApiClient.V2.Wvw.Objectives.AllAsync().ContinueWith(async task =>
            {
                if (task.IsFaulted) return Enumerable.Empty<WvwObjectiveEntity>();
                var map = await MapUtil.RequestMap(mapId);
                var sectors = await MapUtil.RequestSectorsForFloor(map.ContinentId, map.DefaultFloor, map.RegionId, mapId);
                if (sectors == null) return Enumerable.Empty<WvwObjectiveEntity>();

                var newObjectives = new List<WvwObjectiveEntity>();
                foreach (var sector in sectors)
                {
                    var obj = task.Result.FirstOrDefault(x => x.SectorId == sector.Id);
                    if (obj == null) continue;
                    newObjectives.Add(new WvwObjectiveEntity(obj, map, sector));
                }
                _wvwObjectiveCache.Add(map.Id, newObjectives);
                return newObjectives;
            }).Unwrap();
        }

        public void Dispose()
        {
        }
    }
}

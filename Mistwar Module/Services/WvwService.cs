﻿using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Nekres.Mistwar.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Mistwar.Services
{
    internal class WvwService
    {
        public Guid? CurrentGuild { get; private set; }
        public WvwOwner CurrentTeam { get; private set; }

        private Gw2ApiManager _api;

        private AsyncCache<int, IEnumerable<WvwObjectiveEntity>> _wvwObjectiveCache;

        private IEnumerable<World> _worlds;

        private WvwMatchTeamList _teams;

        private DateTime _prevApiRequestTime;

        public WvwService(Gw2ApiManager api)
        {
            _prevApiRequestTime = DateTime.MinValue.ToUniversalTime();
            _api = api;
            _wvwObjectiveCache = new AsyncCache<int, IEnumerable<WvwObjectiveEntity>>(RequestObjectives);
        }

        public async Task LoadAsync()
        {
            _worlds = await _api.Gw2ApiClient.V2.Worlds.AllAsync();
        }

        public async Task Update()
        {
            if (DateTime.UtcNow.Subtract(_prevApiRequestTime).TotalSeconds < 15)
                return;
            _prevApiRequestTime = DateTime.UtcNow;

            this.CurrentGuild = await GetRepresentedGuild();
            if (!GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld()) return;

            var worldId = await GetWorldId();
            if (worldId == -1) return;

            foreach (var id in await GetWvWMapIds(worldId))
            {
                await UpdateObjectives(worldId, id);
            }
        }

        public async Task<Guid?> GetRepresentedGuild()
        {
            if (!_api.HasPermissions(new []{TokenPermission.Account, TokenPermission.Characters})) return null;
            return await _api.Gw2ApiClient.V2.Characters[GameService.Gw2Mumble.PlayerCharacter.Name].GetAsync().ContinueWith(t =>
                        {
                            if (t.IsFaulted) return null;
                            return t.Result.Guild;
                        });
        }

        public string GetWorldName(WvwOwner owner)
        {
            IReadOnlyList<int> team;
            switch (owner)
            {
                case WvwOwner.Red:
                    team = _teams.Red;
                    break;
                case WvwOwner.Blue:
                    team = _teams.Blue;
                    break;
                case WvwOwner.Green:
                    team = _teams.Green;
                    break;
                default: return string.Empty;
            }
            return _worlds.OrderBy(x => x.Population.Value).Reverse().FirstOrDefault(y => team.Contains(y.Id))?.Name ?? string.Empty;
        }

        public async Task<int> GetWorldId()
        {
            if (!_api.HasPermission(TokenPermission.Account)) return -1;
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

        public async Task<IEnumerable<WvwObjectiveEntity>> GetObjectives(int mapId)
        {
            return await _wvwObjectiveCache.GetItem(mapId);
        }

        private async Task UpdateObjectives(int worldId, int mapId)
        {
            var objEntities = await GetObjectives(mapId);
            await _api.Gw2ApiClient.V2.Wvw.Matches.World(worldId).GetAsync()
                .ContinueWith(task =>
                {
                    if (task.IsFaulted) return;

                    _teams = task.Result.AllWorlds;
                    this.CurrentTeam = 
                        _teams.Blue.Contains(worldId) ? WvwOwner.Blue : 
                        _teams.Red.Contains(worldId) ? WvwOwner.Red :
                        _teams.Green.Contains(worldId) ? WvwOwner.Green : WvwOwner.Unknown;

                    var objectives = task.Result.Maps.FirstOrDefault(x => x.Id == mapId)?.Objectives;
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
                });
        }

        private async Task<IEnumerable<WvwObjectiveEntity>> RequestObjectives(int mapId)
        {
            return await _api.Gw2ApiClient.V2.Wvw.Objectives.AllAsync().ContinueWith(async task =>
            {
                if (task.IsFaulted) return Enumerable.Empty<WvwObjectiveEntity>();

                var map = await MapUtil.GetMap(mapId);
                var mapExpanded = await MapUtil.GetMapExpanded(map, map.DefaultFloor);

                if (mapExpanded == null) return Enumerable.Empty<WvwObjectiveEntity>();
                var newObjectives = new List<WvwObjectiveEntity>();
                foreach (var sector in mapExpanded.Sectors.Values)
                {
                    var obj = task.Result.FirstOrDefault(x => x.SectorId == sector.Id);
                    if (obj == null) continue;
                    var o = new WvwObjectiveEntity(obj, mapExpanded);
                    newObjectives.Add(o);
                }
                return newObjectives;
            }).Unwrap();
        }
    }
}

﻿using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.Exceptions;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Blish_HUD.GameService;

namespace Nekres.Stream_Out.Core.Services
{
    internal class MapService : IExportService
    {
        private Logger Logger => StreamOutModule.Logger;
        private Gw2ApiManager Gw2ApiManager => StreamOutModule.ModuleInstance?.Gw2ApiManager;
        private DirectoriesManager DirectoriesManager => StreamOutModule.ModuleInstance?.DirectoriesManager;

        private const string MAP_TYPE = "map_type.txt";
        private const string MAP_NAME = "map_name.txt";

        public MapService()
        {
            Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
            OnMapChanged(null, new ValueEventArgs<int>(Gw2Mumble.CurrentMap.Id));
        }

        private async Task<IEnumerable<ContinentFloorRegionMapSector>> RequestSectors(int continentId, int floor, int regionId, int mapId)
        {
            return await Gw2ApiManager.Gw2ApiClient.V2.Continents[continentId].Floors[floor].Regions[regionId].Maps[mapId].Sectors.AllAsync()
                .ContinueWith(task => task.IsFaulted ? Enumerable.Empty<ContinentFloorRegionMapSector>() : task.Result);
        }

        private async void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            if (e.Value <= 0)
            {
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{MAP_NAME}", string.Empty);
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{MAP_TYPE}", string.Empty);
                return;
            }

            Map map;
            try
            {
                map = await Gw2ApiManager.Gw2ApiClient.V2.Maps.GetAsync(e.Value);
                if (map == null)
                    throw new NullReferenceException("Unknown error.");
            }
            catch (Exception ex) when (ex is UnexpectedStatusException or NullReferenceException)
            {
                Logger.Warn(CommonStrings.WebApiDown);
                return;
            }

            var location = map.Name;
            // Some instanced maps consist of just a single sector and hide their display name in it.
            if (map.Name.Equals(map.RegionName, StringComparison.InvariantCultureIgnoreCase))
            {
                var defaultSector = (await RequestSectors(map.ContinentId, map.DefaultFloor, map.RegionId, map.Id)).FirstOrDefault();
                if (defaultSector != null && !string.IsNullOrEmpty(defaultSector.Name))
                    location = defaultSector.Name.Replace("<br>", " ");
            }
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{MAP_NAME}", location);

            var type = string.Empty;
            switch (map.Type.Value)
            {
                case MapType.Center:
                case MapType.BlueHome:
                case MapType.GreenHome:
                case MapType.RedHome:
                case MapType.JumpPuzzle:
                case MapType.EdgeOfTheMists:
                case MapType.WvwLounge:
                    type = "WvW";
                    break;
                case MapType.PublicMini:
                case MapType.Public:
                    type = map.Id != 350 ? "PvE" : "PvP"; // Edge of the Mists
                    break;
                case MapType.Pvp:
                    type = "PvP";
                    break;
                case MapType.Gvg:
                    type = "GvG";
                    break;
                case MapType.CharacterCreate:
                case MapType.Tutorial:
                case MapType.Instance:
                case MapType.Tournament:
                case MapType.UserTournament:
                case MapType.FortunesVale:
                    type = map.Type.Value.ToDisplayString();
                    break;
                default: break;
            }
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{MAP_TYPE}", type);
        }

        public async Task Update()
        {
        }

        public async Task Initialize()
        {
        }

        public async Task ResetDaily()
        {
        }

        public void Dispose()
        {
        }
    }
}
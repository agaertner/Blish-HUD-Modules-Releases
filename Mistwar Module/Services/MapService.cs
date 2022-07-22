using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;
using Nekres.Mistwar.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Mistwar.Services
{
    internal class MapService : IDisposable
    {
        private const int MAX_MAP_LOAD_RETRIES = 2;

        private DirectoriesManager _dir;
        private Gw2ApiManager _api;
        private WvwService _wvw;

        private MapImage _mapControl;

        private float _opacity;
        public float Opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                _mapControl?.SetOpacity(value);
            }
        }

        private float _colorIntensity;
        public float ColorIntensity
        {
            get => _colorIntensity;
            set
            {
                _colorIntensity = value;
                _mapControl?.SetColorIntensity(value);
            }
        }

        private readonly IProgress<string> _loadingIndicator;

        public bool IsLoading { get; private set; }

        public string Log { get; private set; }

        private Dictionary<int, AsyncTexture2D> _mapCache;

        private int _mapLoadRetries;

        public MapService(Gw2ApiManager api, DirectoriesManager dir, WvwService wvw, IProgress<string> loadingIndicator)
        {
            _dir = dir;
            _api = api;
            _wvw = wvw;
            _loadingIndicator = loadingIndicator;
            _mapCache = new Dictionary<int, AsyncTexture2D>();

            _mapControl = new MapImage
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(0, 0),
                Location = new Point(0, 0)
            };
            _mapControl.Hide();

            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged += OnIsMapOpenChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged += OnIsInGameChanged;
        }

        public void DownloadMaps(int[] mapIds)
        {
            if (mapIds.IsNullOrEmpty()) return;

            var thread = new Thread(() => LoadMapsInBackground(mapIds))
            {
                IsBackground = true
            };

            thread.Start();
        }

        private void LoadMapsInBackground(int[] mapIds)
        {
            this.IsLoading = true;
            foreach (var id in mapIds)
            {
                var t = new Task<Task>(async () => await DownloadMapImage(id));
                t.Start();
                t.Unwrap().Wait();
                // Progress indicator cannot yet handle total percentage of all tasks combined hence we do not use Task.WaitAll(taskList).
                // Considering tile download on slow connections and tile structuring on low-end hardware in addition to loosing indicator info depth a
                // refactor of the code to support it would not yield much value.
            }
            this.IsLoading = false;
            this.Log = null;
            _loadingIndicator.Report(null);
        }

        private async Task DownloadMapImage(int id)
        {
            _mapLoadRetries = 0;
            if (!_mapCache.TryGetValue(id, out var cacheTex))
            {
                cacheTex = new AsyncTexture2D();
                _mapCache.Add(id, cacheTex);
            }

            var filePath = $"{_dir.GetFullDirectoryPath("mistwar")}/{id}.png";

            if (LoadFromCache(filePath, cacheTex))
            {
                await ReloadMap();
                return;
            }

            await MapUtil.BuildMap(await MapUtil.RequestMap(id), filePath, true, _loadingIndicator).ContinueWith(async _ =>
            {
                if (!LoadFromCache(filePath, cacheTex))
                {
                    if (_mapLoadRetries > MAX_MAP_LOAD_RETRIES)
                    {
                        return;
                    }
                    _mapLoadRetries++;
                    await DownloadMapImage(id);
                    return;
                }
                await ReloadMap();
            });
        }

        private bool LoadFromCache(string filePath, AsyncTexture2D cacheTex)
        {
            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (timeout > DateTime.UtcNow)
            {
                try
                {
                    if (!File.Exists(filePath)) continue;
                    using var fil = new MemoryStream(File.ReadAllBytes(filePath));
                    var tex = Texture2D.FromStream(GameService.Graphics.GraphicsDevice, fil);
                    cacheTex.SwapTexture(tex);
                    break;
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException or ArgumentException or InvalidOperationException)
                {
                    if (DateTime.UtcNow < timeout) continue;
                    MistwarModule.Logger.Error(e, e.Message);
                    return false;
                }
            }
            return true;
        }

        public async Task ReloadMap()
        {
            if (GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld() && _mapCache.TryGetValue(GameService.Gw2Mumble.CurrentMap.Id, out var tex))
            {
                _mapControl.Texture.SwapTexture(tex);
                _mapControl.WvwObjectives = await _wvw.GetObjectives(GameService.Gw2Mumble.CurrentMap.Id);
            }
        }

        public void Toggle()
        {
            if (IsLoading)
            {
                ScreenNotification.ShowNotification("Mistwar is initializing.", ScreenNotification.NotificationType.Error);
                return;
            }
            if (!GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld())
            {
                return;
            }
            _mapControl?.Toggle();
        }

        private void OnIsMapOpenChanged(object o, ValueEventArgs<bool> e)
        {
            if (!e.Value) return;
            _mapControl.Hide();
        }

        private void OnIsInGameChanged(object o, ValueEventArgs<bool> e)
        {
            if (!e.Value) return;
            _mapControl.Hide();
        }

        private async void OnMapChanged(object o, ValueEventArgs<int> e)
        { 
            await ReloadMap();
        }

        public void Dispose()
        {
            GameService.Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged -= OnIsMapOpenChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged -= OnIsInGameChanged;
            _mapControl?.Dispose();
            foreach (var tex in _mapCache.Values)
            {
                tex?.Dispose();
            }
        }
    }
}

using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;
using Nekres.Mistwar.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Mistwar.Services
{
    internal class MapService : IDisposable
    {
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
            if (!_mapCache.TryGetValue(id, out var cacheTex))
            {
                cacheTex = new AsyncTexture2D();
                _mapCache.Add(id, cacheTex);
            }

            await ReloadMap(); // We set the async texture on the control here for the case that we are already in WvW when the module loads.

            var filePath = $"{_dir.GetFullDirectoryPath("mistwar")}/{id}.png";
            if (File.Exists(filePath))
            {
                using var fil = new MemoryStream(File.ReadAllBytes(filePath));
                var tex = Texture2D.FromStream(GameService.Graphics.GraphicsDevice, fil);
                cacheTex.SwapTexture(tex);
                return;
            }

            await MapUtil.BuildMap(await MapUtil.RequestMap(id), filePath, true, _loadingIndicator).ContinueWith(async _ =>
            {
                using var fil = new MemoryStream(File.ReadAllBytes(filePath));
                var tex = Texture2D.FromStream(GameService.Graphics.GraphicsDevice, fil);
                cacheTex.SwapTexture(tex);
            });
        }

        public async Task ReloadMap()
        {
            _mapControl.Hide();
            _mapControl.Enabled = false;
            if (GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld() && _mapCache.TryGetValue(GameService.Gw2Mumble.CurrentMap.Id, out var tex))
            {
                _mapControl.Texture = tex;
                _mapControl.WvwObjectives = await _wvw.GetObjectives(GameService.Gw2Mumble.CurrentMap.Id);
                _mapControl.Enabled = true;
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
            _mapControl.Enabled = false;
        }

        private void OnIsInGameChanged(object o, ValueEventArgs<bool> e)
        {
            if (!e.Value) return;
            _mapControl.Hide();
            _mapControl.Enabled = false;
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
        }
    }
}

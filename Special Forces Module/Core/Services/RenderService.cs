using Blish_HUD;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.Exceptions;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nekres.Special_Forces.Core.Services
{
    internal class RenderService : IDisposable
    {
        public event EventHandler<ValueEventArgs<bool>> LoadingChanged;

        private Dictionary<int, Texture2D> _eliteRenderRepository;
        private Dictionary<ProfessionType, Texture2D> _professionRenderRepository;
        private Dictionary<int, string> _eliteSpecNames;
        private Dictionary<ProfessionType, string> _profNames;

        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (value == _isLoading) return;
                _isLoading = value;
                LoadingChanged?.Invoke(this, new ValueEventArgs<bool>(value));
            }
        }

        private readonly IProgress<string> _loadingIndicator;

        public RenderService(IProgress<string> loadingIndicator)
        {
            _loadingIndicator = loadingIndicator;
            _eliteRenderRepository = new Dictionary<int, Texture2D>();
            _professionRenderRepository = new Dictionary<ProfessionType, Texture2D>();
            _eliteSpecNames = new Dictionary<int, string>();
            _profNames = new Dictionary<ProfessionType, string>();
        }

        public void DownloadIcons()
        {
            var thread = new Thread(LoadIconsInBackground)
            {
                IsBackground = true
            };
            thread.Start();
        }

        private void LoadIconsInBackground()
        {
            this.IsLoading = true;
            this.RequestIcons();
            this.IsLoading = false;
            _loadingIndicator.Report(null);
        }

        public Texture2D GetProfessionRender(ProfessionType professionType)
        {
            return _professionRenderRepository.TryGetValue(professionType, out var tex) ? tex : ContentService.Textures.Pixel;
        }

        public Texture2D GetEliteRender(int specId)
        {
            if (_eliteRenderRepository.TryGetValue(specId, out var tex))
            {
                return tex;
            }
            return ContentService.Textures.TransparentPixel;
        }

        public bool IsEliteSpec(int specId)
        {
            return _eliteSpecNames.ContainsKey(specId);
        }

        public string GetEliteSpecName(int specId)
        {
            return _eliteSpecNames.TryGetValue(specId, out var name) ? name : string.Empty;
        }

        public string GetProfessionName(ProfessionType profType)
        {
            return _profNames.TryGetValue(profType, out var name) ? name : string.Empty;
        }

        private void RequestIcons()
        {
            try
            {
                LoadProfessionIcons().Wait();
                LoadEliteIcons().Wait();
            }
            catch (RequestException e)
            {
                SpecialForcesModule.Logger.Error(e, e.Message);
            }
        }

        private async Task LoadProfessionIcons()
        {
            _loadingIndicator.Report("Loading professions..");
            await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Professions.AllAsync().ContinueWith(t =>
            {
                if (t.IsFaulted) return;
                foreach (var profession in t.Result)
                {
                    var renderUri = (string)profession.IconBig;
                    var id = Enum.TryParse<ProfessionType>(profession.Id, true, out var prof) ? prof : ProfessionType.Guardian;
                    _professionRenderRepository.Add(id, GameService.Content.GetRenderServiceTexture(renderUri));
                    _profNames.Add(id, profession.Name);
                }
            });
        }

        private async Task LoadEliteIcons()
        {
            _loadingIndicator.Report("Loading elite specializations..");
            await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Specializations.AllAsync().ContinueWith(t =>
            {
                if (t.IsFaulted) return;
                foreach (var specialization in t.Result)
                {
                    if (!specialization.Elite) continue;
                    _eliteRenderRepository.Add(specialization.Id, GameService.Content.GetRenderServiceTexture(specialization.ProfessionIconBig));
                    _eliteSpecNames.Add(specialization.Id, specialization.Name);
                }
            });
        }

        public void Dispose()
        {
            foreach (var tex in _eliteRenderRepository.Values.Union(_professionRenderRepository.Values))
            {
                tex?.Dispose();
            }
        }
    }
}

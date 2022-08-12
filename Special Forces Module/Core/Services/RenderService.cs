using Blish_HUD;
using Blish_HUD.Content;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.Exceptions;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nekres.Special_Forces.Core.Services
{
    internal class RenderService
    {
        private Dictionary<int, AsyncTexture2D> _eliteRenderRepository;
        private Dictionary<int, AsyncTexture2D> _professionRenderRepository;

        public bool IsLoading { get; private set; }

        private readonly IProgress<string> _loadingIndicator;

        public RenderService(IProgress<string> loadingIndicator)
        {
            _loadingIndicator = loadingIndicator;
            _eliteRenderRepository = new Dictionary<int, AsyncTexture2D>();
            _professionRenderRepository = new Dictionary<int, AsyncTexture2D>();
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
            this.RequestIcons().Wait();
            this.IsLoading = false;
            _loadingIndicator.Report(null);
        }

        public AsyncTexture2D GetProfessionRender(ProfessionType professionType)
        {
            return _professionRenderRepository[(int)professionType];
        }

        public AsyncTexture2D GetEliteRender(Specialization spec)
        {
            if (!spec.Elite)
            {
                return GetProfessionRender(Enum.TryParse<ProfessionType>(spec.Profession, true, out var prof) ? prof : ProfessionType.Guardian);
            }
            return _eliteRenderRepository[spec.Id];
        }

        private async Task RequestIcons()
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
            var professions = await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Professions.AllAsync();
            foreach (var profession in professions)
            {
                var renderUri = (string)profession.IconBig;
                var id = (int)(Enum.TryParse<ProfessionType>(profession.Id, true, out var prof) ? prof : ProfessionType.Guardian);
                _professionRenderRepository.Add(id, GameService.Content.GetRenderServiceTexture(renderUri));
            }
        }

        private async Task LoadEliteIcons()
        {
            var specializations = await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Specializations.AllAsync();
            foreach (var specialization in specializations)
            {
                if (!specialization.Elite) continue;
                _eliteRenderRepository.Add(specialization.Id, GameService.Content.GetRenderServiceTexture(specialization.ProfessionIconBig));
            }
        }
    }
}

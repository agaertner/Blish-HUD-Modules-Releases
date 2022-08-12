using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.Exceptions;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Special_Forces.Persistance;

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

        public AsyncTexture2D GetProfessionRender(RawTemplate template)
        {
            if (_professionRenderRepository.All(x => x.Key != (int)template.BuildChatLink.Profession))
            {
                var render = new AsyncTexture2D();
                _professionRenderRepository.Add((int)template.BuildChatLink.Profession, render);
                return render;
            }
            return _professionRenderRepository[(int)template.BuildChatLink.Profession];
        }

        public AsyncTexture2D GetEliteRender(RawTemplate template)
        {
            if (template.Specialization != null && template.Specialization.Elite)
                return GetProfessionRender(template);
            if (_eliteRenderRepository.All(x => x.Key != template.BuildChatLink.Specialization3Id))
            {
                var render = new AsyncTexture2D();
                _eliteRenderRepository.Add(template.BuildChatLink.Specialization3Id, render);
                return render;
            }
            return _eliteRenderRepository[template.BuildChatLink.Specialization3Id];
        }

        private async Task RequestIcons()
        {
            try
            {
                var professionRenderRepository = await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Professions.AllAsync();
                LoadProfessionIcons(professionRenderRepository).Wait();
                LoadEliteIcons().Wait();
            }
            catch (RequestException e)
            {
                SpecialForcesModule.Logger.Error(e, e.Message);
            }
        }

        private async Task LoadProfessionIcons(IEnumerable<Profession> professions)
        {
            foreach (var profession in professions)
            {
                var renderUri = (string)profession.IconBig;
                var id = (int)Enum.GetValues(typeof(ProfessionType)).Cast<ProfessionType>().ToList()
                    .Find(x => x.ToString().Equals(profession.Id, StringComparison.InvariantCultureIgnoreCase));
                if (_professionRenderRepository.Any(x => x.Key == id))
                {
                    try
                    {
                        var textureDataResponse = await GameService.Gw2WebApi.AnonymousConnection.Client.Render
                            .DownloadToByteArrayAsync(renderUri);

                        using (var textureStream = new MemoryStream(textureDataResponse))
                        {
                            var loadedTexture =
                                Texture2D.FromStream(GameService.Graphics.GraphicsDevice, textureStream);

                            _professionRenderRepository[id].SwapTexture(loadedTexture);
                        }
                    }
                    catch (Exception ex)
                    {
                        SpecialForcesModule.Logger.Warn(ex, $"Request to render service for {renderUri} failed.", renderUri);
                    }
                }
                else
                {
                    _professionRenderRepository.Add(id, GameService.Content.GetRenderServiceTexture(renderUri));
                }
            }
        }

        private async Task LoadEliteIcons()
        {
            var ids = await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Specializations.IdsAsync();
            var specializations = await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Specializations.ManyAsync(ids);
            foreach (var specialization in specializations)
            {
                if (!specialization.Elite) continue;
                if (_eliteRenderRepository.Any(x => x.Key == specialization.Id))
                {
                    var renderUri = (string)specialization.ProfessionIconBig;
                    try
                    {
                        var textureDataResponse = await GameService.Gw2WebApi.AnonymousConnection
                            .Client
                            .Render.DownloadToByteArrayAsync(renderUri);

                        using (var textureStream = new MemoryStream(textureDataResponse))
                        {
                            var loadedTexture = Texture2D.FromStream(GameService.Graphics.GraphicsDevice, textureStream);

                            _eliteRenderRepository[specialization.Id].SwapTexture(loadedTexture);
                        }
                    }
                    catch (Exception ex)
                    {
                        SpecialForcesModule.Logger.Warn(ex, $"Request to render service for {renderUri} failed.", renderUri);
                    }
                }
                else
                {
                    _eliteRenderRepository.Add(specialization.Id, GameService.Content.GetRenderServiceTexture(specialization.ProfessionIconBig));
                }
            }
        }
    }
}

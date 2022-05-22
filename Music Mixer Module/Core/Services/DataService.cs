using Blish_HUD;
using Blish_HUD.Content;
using LiteDB.Async;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Music_Mixer.Core.Player.API;
using Nekres.Music_Mixer.Core.Services.Entities;
using Nekres.Music_Mixer.Core.UI.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Formats.Jpeg;
using Image = SixLabors.ImageSharp.Image;

namespace Nekres.Music_Mixer.Core.Services
{
    internal class DataService : IDisposable
    {
        private LiteDatabaseAsync _db;
        private ILiteCollectionAsync<MusicContextEntity> _ctx;
        private ILiteStorageAsync<string> _thumbnails;
        private Guid _prevRandom;

        public DataService(string cacheDir)
        {
            _db = new LiteDatabaseAsync(Path.Combine(cacheDir, "data.db"));
            _ctx = _db.GetCollection<MusicContextEntity>("music_contexts");
            _thumbnails = _db.GetStorage<string>("thumbnails", "thumbnail_chunks");
        }

        public void DownloadThumbnail(MusicContextModel model)
        { 
            youtube_dl.Instance.GetThumbnail(model.Thumbnail, model.Uri, model.Uri, ThumbnailUrlReceived);
        }

        private void ThumbnailUrlReceived(AsyncTexture2D tex, string id, string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            var client = new WebClient();
            client.OpenReadAsync(new Uri(url));
            client.OpenReadCompleted += async (o, e) =>
            {
                try
                {
                    if (e.Cancelled) return;
                    if (e.Error != null) throw e.Error;

                    var stream = e.Result;
                    using var image = await Image.LoadAsync(stream);
                    using (var ms = new MemoryStream())
                    {
                        await image.SaveAsync(ms, JpegFormat.Instance);
                        ms.Position = 0;
                        await _thumbnails.UploadAsync(id, url, ms);
                        stream.Close();
                        ((WebClient)o).Dispose();
                        var thumb = Texture2D.FromStream(GameService.Graphics.GraphicsDevice, ms);
                        tex.SwapTexture(thumb);
                    }
                }
                catch (Exception ex) when (ex is WebException or ImageFormatException or ArgumentException or InvalidOperationException)
                {
                    MusicMixer.Logger.Info(ex, ex.Message);
                }
            };
        }

        public async Task GetThumbnail(MusicContextModel model)
        {
            var texture = await _thumbnails.OpenReadAsync(model.Uri);
            if (texture == null) return;
            try
            {
                var thumbnail = Texture2D.FromStream(GameService.Graphics.GraphicsDevice, texture);
                model.Thumbnail.SwapTexture(thumbnail);
            }
            catch (InvalidOperationException e)
            {
                // Unsupported image format.
                MusicMixer.Logger.Info(e,e.Message);
            }
        }

        public async Task Upsert(MusicContextModel model)
        {
            var entity = await _ctx.FindOneAsync(x => x.Id.Equals(model.Id));
            if (entity == null)
            {
                entity = MusicContextEntity.FromModel(model);
                await _ctx.InsertAsync(entity);
            }
            else
            {
                entity.DayTimes = model.DayTimes.ToList();
                entity.MapIds = model.MapIds.ToList();
                entity.SectorIds = model.SectorIds.ToList();
                entity.MountTypes = model.MountTypes.ToList();
                entity.States = model.States.ToList();
                await _ctx.UpdateAsync(entity);
            }
            await _ctx.EnsureIndexAsync(x => x.Id);
        }

        public async Task<int> Count()
        {
            return await _ctx.CountAsync();
        }

        public async Task Delete(MusicContextModel model)
        {
            await _ctx.DeleteManyAsync(x => model.Id.Equals(model.Id));
            model.Delete();
        }

        public async Task<MusicContextEntity> FindById(Guid id)
        {
            return await _ctx.FindOneAsync(x => x.Id.Equals(id));
        }

        public async Task<MusicContextEntity> GetRandom()
        {
            var actives = await _ctx.CountAsync(x =>
                (!x.DayTimes.Any() || x.DayTimes.Contains(TyrianTimeUtil.GetCurrentDayCycle()))
                 && (!x.MapIds.Any() || x.MapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id))
                 && (!x.MountTypes.Any() || x.MountTypes.Contains(GameService.Gw2Mumble.PlayerCharacter.CurrentMount))
                 && (!x.States.Any() || x.States.Contains(MusicMixer.Instance.Gw2State.CurrentState)) 
                && !_prevRandom.Equals(x.Id));
            if (actives <= 0) return null;
            return (await _ctx.FindAsync(x => 
                (!x.DayTimes.Any() || x.DayTimes.Contains(TyrianTimeUtil.GetCurrentDayCycle()))
                  && (!x.MapIds.Any() || x.MapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id))
                  && (!x.MountTypes.Any() || x.MountTypes.Contains(GameService.Gw2Mumble.PlayerCharacter.CurrentMount))
                  && (!x.States.Any() || x.States.Contains(MusicMixer.Instance.Gw2State.CurrentState)) 
                && !_prevRandom.Equals(x.Id), RandomUtil.GetRandom(0, actives - 1), 1)).ToArray()[0];
        }

        public async Task<IEnumerable<MusicContextEntity>> GetPage(int startIndex, int pageSize)
        {
            return await _ctx.FindAsync(x => true, startIndex, pageSize);
        }

        public async Task<IEnumerable<MusicContextEntity>> GetAll()
        {
            return await _ctx.FindAllAsync();
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}

using Blish_HUD;
using Blish_HUD.Content;
using LiteDB.Async;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Music_Mixer.Core.Player.API;
using Nekres.Music_Mixer.Core.Services.Entities;
using Nekres.Music_Mixer.Core.UI.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Image = SixLabors.ImageSharp.Image;

namespace Nekres.Music_Mixer.Core.Services
{
    internal class DataService : IDisposable
    {
        private LiteDatabaseAsync _db;
        private ILiteCollectionAsync<MusicContextEntity> _ctx;
        private ILiteStorageAsync<string> _thumbnails;
        private Dictionary<Gw2StateService.State, HashSet<Guid>> _playlists;
        public DataService(string cacheDir)
        {
            _playlists = new Dictionary<Gw2StateService.State, HashSet<Guid>>();
            foreach (var state in Enum.GetValues(typeof(Gw2StateService.State)).Cast<Gw2StateService.State>())
            {
                _playlists.Add(state, new HashSet<Guid>());
            }
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
                entity.ExcludedMapIds = model.ExcludedMapIds.ToList();
                entity.MountTypes = model.MountTypes.ToList();
                entity.State = model.State;
                await _ctx.UpdateAsync(entity);
            }
            await _ctx.EnsureIndexAsync(x => x.Id);
        }

        public async Task Delete(MusicContextModel model)
        {
            await _ctx.DeleteManyAsync(x => x.Id.Equals(model.Id));
        }

        public async Task<MusicContextEntity> FindById(Guid id)
        {
            return await _ctx.FindOneAsync(x => x.Id.Equals(id));
        }

        public async Task<MusicContextEntity> GetRandom()
        {
            // Get already played songs
            var playlist = _playlists[MusicMixer.Instance.Gw2State.CurrentState];

            // Get tracks not already played.
            var tracks = (await GetByState(MusicMixer.Instance.Gw2State.CurrentState)).Where(x => !playlist.Contains(x.Id)).ToList();
            

            // Clear if all songs have been played.
            if (!tracks.Select(x => x.Id).Except(playlist).Any())
            {
                playlist.Clear();
            }

            // Get songs playable
            var actives = tracks.Where(MusicContextEntity.CanPlay).ToList();
            if (actives.Count <= 0) return null;

            // Get one random
            var random = actives[RandomUtil.GetRandom(0, actives.Count - 1)];
            playlist.Add(random.Id);

            return random;
        }

        public async Task<IEnumerable<MusicContextEntity>> GetAll()
        {
            return await _ctx.FindAllAsync();
        }

        public async Task<IEnumerable<MusicContextEntity>> GetByState(Gw2StateService.State state)
        {
            return await _ctx.FindAsync(x => x.State == state);
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}

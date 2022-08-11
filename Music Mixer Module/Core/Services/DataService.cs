using Blish_HUD;
using Blish_HUD.Content;
using LiteDB;
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
using System.Linq.Expressions;
using System.Net;
using Image = SixLabors.ImageSharp.Image;

namespace Nekres.Music_Mixer.Core.Services
{
    internal class DataService : IDisposable
    {
        private LiteDatabase _db;
        private ILiteCollection<MusicContextEntity> _ctx;
        private ILiteStorage<string> _thumbnails;
        private Dictionary<string, HashSet<Guid>> _playlists;
        public DataService(string cacheDir)
        {
            _playlists = new Dictionary<string, HashSet<Guid>>();
            _db = new LiteDatabase(new ConnectionString
            {
                Filename = Path.Combine(cacheDir, "data.db"),
                Connection = ConnectionType.Shared
            });
            _ctx = _db.GetCollection<MusicContextEntity>("music_contexts");
            _thumbnails = _db.GetStorage<string>("thumbnails", "thumbnail_chunks");
        }

        public void DownloadThumbnail(MusicContextModel model)
        { 
            youtube_dl.GetThumbnail(model.Thumbnail, model.Uri, model.Uri, ThumbnailUrlReceived);
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
                    using var ms = new MemoryStream();
                    await image.SaveAsync(ms, JpegFormat.Instance);
                    ms.Position = 0;
                    _thumbnails.Upload(id, url, ms);
                    stream.Close();
                    ((WebClient)o).Dispose();
                    var thumb = Texture2D.FromStream(GameService.Graphics.GraphicsDevice, ms);
                    tex.SwapTexture(thumb);
                }
                catch (Exception ex) when (ex is WebException or ImageFormatException or ArgumentException or InvalidOperationException)
                {
                    MusicMixer.Logger.Info(ex, ex.Message);
                }
            };
        }

        public void GetThumbnail(MusicContextModel model)
        {
            var texture = _thumbnails.OpenRead(model.Uri);
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

        public void Upsert(MusicContextModel model)
        {
            _ctx.EnsureIndex(x => x.Id);
            var entity = _ctx.FindOne(x => x.Id.Equals(model.Id));
            if (entity == null)
            {
                entity = MusicContextEntity.FromModel(model);
                _ctx.Insert(entity);
            }
            else
            {
                entity.DayTimes = model.DayTimes.ToList();
                entity.MapIds = model.MapIds.ToList();
                entity.ExcludedMapIds = model.ExcludedMapIds.ToList();
                entity.MountTypes = model.MountTypes.ToList();
                entity.State = model.State;
                entity.Volume = model.Volume;
                _ctx.Update(entity);
            }
        }

        public void Delete(MusicContextModel model)
        {
            _ctx.DeleteMany(x => x.Id.Equals(model.Id));
        }

        public MusicContextEntity FindById(Guid id)
        {
            return _ctx.FindOne(x => x.Id.Equals(id));
        }

        public MusicContextEntity GetRandom()
        {
            var state = MusicMixer.Instance.Gw2State.CurrentState;
            var mapId = GameService.Gw2Mumble.CurrentMap.Id;
            var dayCycle = MusicMixer.Instance.Gw2State.TyrianTime;
            var mount = GameService.Gw2Mumble.PlayerCharacter.CurrentMount;

            // Get all tracks for state.
            var tracks = FindWhere(x => (x.State == state
                                               && state != Gw2StateService.State.Mounted && x.MapIds.Contains(mapId)
                                               || state == Gw2StateService.State.Mounted && x.State == Gw2StateService.State.Mounted)
                                               && x.DayTimes.Contains(MusicMixer.Instance.ToggleFourDayCycleSetting.Value ? TyrianTimeUtil.GetCurrentDayCycle() : TyrianTimeUtil.GetCurrentDayCycle().Resolve())
                                               && (!x.MountTypes.Any() || x.MountTypes.Contains(mount))).ToList();

            if (!tracks.Any())
            {
                return null;
            }

            var context = $"{state}{mapId}{dayCycle}{mount}";
            if (!_playlists.ContainsKey(context))
            {
                _playlists.Add(context, new HashSet<Guid>());
            }

            // Get already played tracks.
            var playlist = _playlists[context];

            var unPlayed = tracks.Where(x => playlist.Contains(x.Id)).ToList();
            // Clear if all songs have been played.
            if (!unPlayed.Any())
            {
                playlist.Clear();
                unPlayed = tracks;
            }

            // Get one random
            var random = unPlayed[RandomUtil.GetRandom(0, Math.Max(0, unPlayed.Count - 1))];
            playlist.Add(random.Id);

            return random;
        }

        public IEnumerable<MusicContextEntity> FindWhere(Expression<Func<MusicContextEntity, bool>> expr)
        {
            return _ctx.Find(expr);
        }

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        /// <remarks>
        ///  Will do nothing if the connection type is shared since it's closed after each operation.<br/>
        ///  See also: <seealso href="https://www.litedb.org/docs/connection-string/"/>
        /// </remarks>
        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}

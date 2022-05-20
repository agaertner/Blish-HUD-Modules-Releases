using Blish_HUD;
using LiteDB.Async;
using Nekres.Music_Mixer.Core.Services.Entities;
using Nekres.Music_Mixer.Core.UI.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer.Core.Services
{
    internal class DataService : IDisposable
    {

        private LiteDatabaseAsync _db;
        private ILiteCollectionAsync<MusicContextEntity> _ctx;
        private Guid _prevRandom;

        public DataService(string cacheDir)
        {
            _db = new LiteDatabaseAsync(Path.Combine(cacheDir, "data.db"));
            _ctx = _db.GetCollection<MusicContextEntity>("music_contexts");
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
                entity.DayTimes = model.DayTimes;
                entity.MapIds = model.MapIds;
                entity.SectorIds = model.SectorIds;
                entity.MountTypes = model.MountTypes;
                entity.States = model.States;
                entity.Title = model.Title;
                entity.Uri = model.Uri;
                await _ctx.UpdateAsync(entity);
            }
            await _ctx.EnsureIndexAsync(x => x.Id);
        }

        public async Task Delete(MusicContextModel model)
        {
            await _ctx.DeleteManyAsync(x => model.Id.Equals(model.Id));
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
                 && (!x.States.Any() || x.States.Contains(MusicMixerModule.ModuleInstance.Gw2State.CurrentState)) 
                && !_prevRandom.Equals(x.Id));
            if (actives <= 0) return null;
            return (await _ctx.FindAsync(x => 
                (!x.DayTimes.Any() || x.DayTimes.Contains(TyrianTimeUtil.GetCurrentDayCycle()))
                  && (!x.MapIds.Any() || x.MapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id))
                  && (!x.MountTypes.Any() || x.MountTypes.Contains(GameService.Gw2Mumble.PlayerCharacter.CurrentMount))
                  && (!x.States.Any() || x.States.Contains(MusicMixerModule.ModuleInstance.Gw2State.CurrentState)) 
                && !_prevRandom.Equals(x.Id), RandomUtil.GetRandom(0, actives - 1), 1)).ToArray()[0];
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}

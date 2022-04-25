using Blish_HUD;
using Microsoft.Xna.Framework.Audio;
using Nekres.Musician.Core.Models;
using Nekres.Musician.UI.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nekres.Musician.UI
{
    internal class MusicSheetService : IDisposable
    {
        public event EventHandler<ValueEventArgs<MusicSheetModel>> OnSheetUpdated;

        public string CacheDir { get; private set; }

        private SQLiteAsyncConnection _db;

        private SoundEffect[] _deleteSfx;
        public SoundEffect DeleteSfx => _deleteSfx[RandomUtil.GetRandom(0, 1)];
        public MusicSheetService(string cacheDir)
        {
            _deleteSfx = new []
            {
                MusicianModule.ModuleInstance.ContentsManager.GetSound(@"audio\crumbling-paper-1.wav"),
                MusicianModule.ModuleInstance.ContentsManager.GetSound(@"audio\crumbling-paper-2.wav")
            }; 
            this.CacheDir = cacheDir;
        }

        public async Task LoadAsync()
        {
            await LoadDatabase();
        }

        private async Task LoadDatabase()
        {
            var filePath = Path.Combine(this.CacheDir, "db.sqlite");
            _db = new SQLiteAsyncConnection(filePath);
            await _db.CreateTableAsync<MusicSheetModel>();
        }

        public async Task AddOrUpdate(MusicSheet musicSheet, bool silent = false)
        {
            
            var sheet = await _db.Table<MusicSheetModel>().FirstOrDefaultAsync(x => x.Id.Equals(musicSheet.Id));

            if (sheet == null)
            {
                await _db.InsertAsync(musicSheet.ToModel());
            }
            else
            {
                var model = musicSheet.ToModel();
                await _db.UpdateAsync(model);
                OnSheetUpdated?.Invoke(this, new ValueEventArgs<MusicSheetModel>(model));
            }

            if (silent) return;
            GameService.Content.PlaySoundEffectByName("color-change");
        }

        public async Task Delete(Guid key)
        {
            DeleteSfx.Play(GameService.GameIntegration.Audio.Volume, 0, 0);
            await _db.Table<MusicSheetModel>().DeleteAsync(x => x.Id.Equals(key));
        }

        public void Dispose()
        {
            foreach (var sfx in _deleteSfx) sfx?.Dispose();
        }

        public async Task<MusicSheetModel> GetById(Guid id)
        {
            return await _db.Table<MusicSheetModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
        }

        public async Task<IEnumerable<MusicSheetModel>> GetAll()
        {
            return await _db.Table<MusicSheetModel>().ToListAsync();
        }
    }
}

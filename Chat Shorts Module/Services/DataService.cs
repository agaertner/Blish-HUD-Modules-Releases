using Blish_HUD;
using LiteDB.Async;
using Nekres.Chat_Shorts.UI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Chat_Shorts.Services
{
    internal class DataService
    {
        public event EventHandler<ValueEventArgs<Guid>> MacroDeleted;

        private LiteDatabaseAsync _liteDatabase;
        private ILiteCollectionAsync<MacroEntity> _ctx;

        public bool Loading { get; private set; }

        private string _cacheDir;
        public DataService(string cacheDir)
        {
            _cacheDir = cacheDir;
            _liteDatabase = new LiteDatabaseAsync(Path.Combine(_cacheDir, "data.db"));
            _ctx = _liteDatabase.GetCollection<MacroEntity>("macros");
        }

        public async Task UpsertMacro(MacroModel model)
        {
            var e = await _ctx.FindOneAsync(x => x.Id.Equals(model.Id));
            if (e == null)
            {
                await _ctx.InsertAsync(model.ToEntity());
                GameService.Content.PlaySoundEffectByName("color-change");
            }
            else
            {
                e.Text = model.Text;
                e.Title = model.Title;
                e.GameMode = model.Mode;
                e.MapIds = model.MapIds;
                e.PrimaryKey = model.KeyBinding.PrimaryKey;
                e.ModifierKey = model.KeyBinding.ModifierKeys;
                await _ctx.UpdateAsync(e);
            }
            await _ctx.EnsureIndexAsync(x => x.Id);
        }

        public async Task<IEnumerable<MacroEntity>> GetAll()
        {
            this.Loading = true;
            var result = await _ctx.FindAllAsync().ContinueWith(t =>
            {
                this.Loading = false;
                return t.Result;
            });
            return result;
        }

        public async Task DeleteById(Guid id)
        {
            await _ctx.DeleteManyAsync(x => x.Id.Equals(id));
            MacroDeleted?.Invoke(this, new ValueEventArgs<Guid>(id));
        }

        public async Task<IEnumerable<MacroEntity>> GetAllActives() => await _ctx.FindAsync(x => MacroEntity.CanActivate(x)); // async lib no haz method group overload :(
    }
}

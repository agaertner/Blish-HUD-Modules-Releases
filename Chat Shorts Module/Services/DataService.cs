using Blish_HUD;
using LiteDB;
using Nekres.Chat_Shorts.UI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nekres.Chat_Shorts.Services
{
    internal class DataService : IDisposable
    {
        public event EventHandler<ValueEventArgs<Guid>> MacroDeleted;

        private LiteDatabase _db;
        private ILiteCollection<MacroEntity> _ctx;

        public bool Loading { get; private set; }

        private string _cacheDir;

        public DataService(string cacheDir)
        {
            _cacheDir = cacheDir;
            _db = new LiteDatabase(new ConnectionString
            {
                Filename = Path.Combine(_cacheDir, "data.db"),
                Connection = ConnectionType.Shared
            });
            _ctx = _db.GetCollection<MacroEntity>("macros");
        }

        public void UpsertMacro(MacroModel model)
        {
            _ctx.EnsureIndex(x => x.Id);
            var e = _ctx.FindOne(x => x.Id.Equals(model.Id));
            if (e == null)
            {
                _ctx.Insert(model.ToEntity());
                GameService.Content.PlaySoundEffectByName("color-change");
            }
            else
            {
                e.Text = model.Text;
                e.Title = model.Title;
                e.GameMode = model.Mode;
                e.MapIds = model.MapIds.ToList();
                e.ExcludedMapIds = model.ExcludedMapIds.ToList();
                e.SquadBroadcast = model.SquadBroadcast;
                e.PrimaryKey = model.KeyBinding.PrimaryKey;
                e.ModifierKey = model.KeyBinding.ModifierKeys;
                _ctx.Update(e);
            }
            ChatShorts.Instance.BuildContextMenu();
        }

        public IEnumerable<MacroEntity> GetAll()
        {
            this.Loading = true;
            var result = _ctx.FindAll();
            return result;
        }

        public void DeleteById(Guid id)
        { 
            _ctx.DeleteMany(x => x.Id.Equals(id));
            MacroDeleted?.Invoke(this, new ValueEventArgs<Guid>(id));
        }

        public IEnumerable<MacroEntity> GetAllActives() => _ctx.Find(e => 
            (e.GameMode == MapUtil.GetCurrentGameMode() || e.GameMode == GameMode.All) &&
            (e.MapIds.Any(id => id == GameService.Gw2Mumble.CurrentMap.Id) || !e.MapIds.Any()) &&
            e.ExcludedMapIds.All(id => id != GameService.Gw2Mumble.CurrentMap.Id) &&
            (!e.SquadBroadcast || GameService.Gw2Mumble.PlayerCharacter.IsCommander));

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}

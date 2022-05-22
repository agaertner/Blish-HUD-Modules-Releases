using System;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.Player.API;
using Nekres.Music_Mixer.Core.Player.API.Models;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Views;

namespace Nekres.Music_Mixer.Core.UI.Presenters
{
    internal class LibraryPresenter : Presenter<LibraryView, LibraryModel>
    {
        public LibraryPresenter(LibraryView view, LibraryModel model) : base(view, model)
        {
        }

        public async Task AddNew(IProgress<string> e)
        {
            var url = await ClipboardUtil.WindowsClipboardService.GetTextAsync();
            e.Report("Checking link validity..");
            if (!await youtube_dl.Instance.IsUrlSupported(url))
            {
                GameService.Content.PlaySoundEffectByName("error");
                ScreenNotification.ShowNotification("Your clipboard does not contain a suitable video link.", ScreenNotification.NotificationType.Error);
                this.View.Loading = false;
                return;
            }
            e.Report("Ok! Fetching metadata...");
            youtube_dl.Instance.GetMetaData(url, MetaDataReceived);
        }

        private async Task MetaDataReceived(MetaData data)
        {
            var model = new MusicContextModel(data.Title, data.Artist, data.Url, data.Duration);
            Add(model);
            await MusicMixer.Instance.DataService.Upsert(model);
            MusicMixer.Instance.DataService.DownloadThumbnail(model);
            GameService.Content.PlaySoundEffectByName("color-change");
            this.View.Loading = false;
        }

        public void Add(MusicContextModel model)
        {
            var contextEntry = new MusicContextDetails(model)
            {
                Parent = this.View.MusicContextPanel,
                Size = new Point(345, 100)
            };
            contextEntry.EditClick += OnMusicContextConfigClicked;
        }

        private void OnMusicContextConfigClicked(object o, MouseEventArgs e)
        {
            var ctrl = (MusicContextDetails)o;
            ctrl.Active = true;
            var bgTex = GameService.Content.GetTexture("controls/window/502049");
            var windowRegion = new Rectangle(40, 26, 895 + 38, 780 - 56);
            var contentRegion = new Rectangle(70, 41, 895 - 43, 780 - 142);
            var editWindow = new StandardWindow(bgTex, windowRegion, contentRegion)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = new Point((GameService.Graphics.SpriteScreen.Width - windowRegion.Width) / 2, (GameService.Graphics.SpriteScreen.Height - windowRegion.Height) / 2),
                Title = $"Config",
                Id = $"{nameof(MusicMixer)}_{nameof(MusicContextConfigView)}_6895faf4-ed5d-433b-b0e2-25f291333519",
                SavesPosition = true
            };
            editWindow.Show(new MusicContextConfigView(ctrl.Model));
            editWindow.Hidden += (_, _) => editWindow.Dispose();
            editWindow.Disposed += (_, _) =>
            {
                ctrl.Model.Deleted -= OnEntryDeleted;
                ctrl.Active = false;
            };
            ctrl.Model.Deleted += OnEntryDeleted;
        }

        private void OnEntryDeleted(object o, ValueEventArgs<Guid> e)
        {
            var ctrl = this.View.MusicContextPanel?.Children
                .Where(x => x.GetType() == typeof(MusicContextDetails))
                .Cast<MusicContextDetails>().FirstOrDefault(x => x.Model.Id.Equals(e.Value));
            ctrl?.Dispose();
        }
    }
}

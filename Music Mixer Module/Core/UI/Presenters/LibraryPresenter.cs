using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.Player.API;
using Nekres.Music_Mixer.Core.Player.API.Models;
using Nekres.Music_Mixer.Core.Services;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Views;
using System;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer.Core.UI.Presenters
{
    internal class LibraryPresenter : Presenter<LibraryView, MainModel>
    {
        public LibraryPresenter(LibraryView view, MainModel model) : base(view, model)
        {
            model.Changed += OnModelChanged;
        }

        private void OnModelChanged(object o, EventArgs e)
        {
            ((ViewContainer)this.View?.MusicContextPanel?.Parent)?.Show(new LibraryView(this.Model));
        }

        public void Add(MusicContextModel model)
        {
            var contextEntry = new MusicContextDetails(model)
            {
                Parent = this.View.MusicContextPanel,
                Size = new Point(this.View.MusicContextPanel.ContentRegion.Width - 12, 100),
                Editable = this.Model.State != Gw2StateService.State.Mounted
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
                Title = $"Edit Song Context - {ctrl.Title.Substring(0, Math.Min(ctrl.Title.Length - 1, 25))}",
                Id = $"{nameof(MusicMixer)}_{nameof(ConfigView)}_1450662d-40f1-48e3-b398-b373d5cd9314",
                SavesPosition = true
            };
            editWindow.Show(new ConfigView(
                new ConfigModel(ctrl.Model)
                {
                    ContinentId = this.Model.ContinentId,
                    RegionId = this.Model.RegionId,
                    MapId = this.Model.MapId,
                    DayCycle = this.Model.DayCycle,
                    MountType = this.Model.MountType
                }));
            editWindow.Hidden += (_, _) => editWindow.Dispose();
            editWindow.Disposed += (_, _) => ctrl.Active = false;
        }

        public async Task AddNew(IProgress<string> e)
        {
            var url = await ClipboardUtil.WindowsClipboardService.GetTextAsync();
            e.Report("Checking link validity..");
            if (!await youtube_dl.IsUrlSupported(url))
            {
                GameService.Content.PlaySoundEffectByName("error");
                ScreenNotification.ShowNotification("Your clipboard does not contain a suitable video link.", ScreenNotification.NotificationType.Error);
                this.View.Loading = false;
                return;
            }
            e.Report("Ok! Fetching metadata...");
            youtube_dl.GetMetaData(url, MetaDataReceived);
        }

        private void MetaDataReceived(MetaData data)
        {
            var model = new MusicContextModel(this.Model.State, data.Title, data.Artist, data.Url, data.Duration,
                new[] { this.Model.MapId }, 
                null,
                new []{ this.Model.DayCycle }, 
                new []{ this.Model.MountType });
            Add(model);
            MusicMixer.Instance.DataService.Upsert(model);
            MusicMixer.Instance.DataService.DownloadThumbnail(model);
            GameService.Content.PlaySoundEffectByName("color-change");
            this.View.Loading = false;
        }

        protected override void Unload()
        {
            this.Model.Changed -= OnModelChanged;
            base.Unload();
        }
    }
}

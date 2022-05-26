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
using Nekres.Music_Mixer.Core.UI.Views.StateViews;
using System;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer.Core.UI.Presenters
{
    internal class LibraryPresenter : Presenter<StateView, Gw2StateService.State>
    {
        public Gw2StateService.State State { get; }
        public LibraryPresenter(StateView view, Gw2StateService.State state) : base(view, state)
        {
            State = state;
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
            var model = new MusicContextModel(this.State, data.Title, data.Artist, data.Url, data.Duration);
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
            var view = new MusicContextConfigView(ctrl.Model);
            this.View.ConfigView.Show(view);
        }
    }
}

using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.Screenshot_Manager.UI.Controls;
using Nekres.Screenshot_Manager.UI.Models;
using Nekres.Screenshot_Manager.UI.Views;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nekres.Screenshot_Manager.Properties;

namespace Nekres.Screenshot_Manager.UI.Presenters
{
    public class ScreenshotManagerPresenter : Presenter<ScreenshotManagerView, ScreenshotManagerModel>
    {
        public ScreenshotManagerPresenter(ScreenshotManagerView view, ScreenshotManagerModel model) : base(view, model)
        {
            this.Model.FileWatcherFactory.FileAdded += OnScreenShotAdded;
            this.Model.FileWatcherFactory.FileDeleted += OnScreenShotDeleted;
            this.Model.FileWatcherFactory.FileRenamed += OnScreenShotRenamed;
        }

        private void LoadTextures()
        {
            //this.Model.PortaitModeIcon128 = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("portaitMode_icon_128x128.png");
            //this.Model.PortaitModeIcon512 = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("portaitMode_icon_128x128.png");
            //this.Model.DeleteSearchBoxContentIcon = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("784262.png");
        }

        protected override Task<bool> Load(IProgress<string> progress)
        {
            return base.Load(progress);
        }

        public async void OnScreenShotAdded(object o, ValueEventArgs<string> e)
        {
            var texture = new AsyncTexture2D();
            this.CreateThumbnail(this.View.ThumbnailFlowPanel, texture, e.Value);
            await LoadTexture(texture, e.Value);
        }

        public void OnScreenShotDeleted(object o, ValueEventArgs<string> e)
        {
            if (!FindThumbnailByFileName(e.Value, out var ctrl)) return;
            this.View.ThumbnailFlowPanel.RemoveChild(ctrl);
            ctrl?.Dispose();
        }

        public void OnScreenShotRenamed(object o, ValueChangedEventArgs<string> e)
        {
            if (!FindThumbnailByFileName(e.PreviousValue, out var ctrl)) return;
            ctrl.FileName = e.NewValue;
            ctrl.NameTextBox.Text = Path.GetFileNameWithoutExtension(e.NewValue);
        }

        private bool FindThumbnailByFileName(string fileName, out ResponsiveThumbnail thumbnail)
        {
            thumbnail = this.View.ThumbnailFlowPanel.Children.Where(x => x.GetType() == typeof(ResponsiveThumbnail)).Cast<ResponsiveThumbnail>().FirstOrDefault(y => fileName.Equals(y.FileName));
            return thumbnail != null;
        }

        public ThumbnailBase CreateThumbnail(FlowPanel parent, AsyncTexture2D texture, string fileName)
        {
            var ctrl = new ResponsiveThumbnail(texture, fileName)
            {
                Parent = parent,
                Size = new Point(parent.Width / 4 - (int)parent.ControlPadding.X, 144),
                IsFavorite = ScreenshotManagerModule.ModuleInstance.Favorites.Value.Any(x => x.Equals(Path.GetFileName(fileName)))
            };
            ctrl.OnDelete += OnClickDelete;
            ctrl.FavoriteChanged += OnFavoriteChanged;
            ctrl.OnInspect += OnClickInspect;
            return ctrl;
        }

        private void OnClickDelete(object o, EventArgs e)
        {
            var ctrl = (ThumbnailBase)o;

            if (ScreenshotManagerModule.ModuleInstance.SendToRecycleBin.Value)
            {
                DoDelete(ctrl, true);
                return;
            }

            ConfirmationPrompt.ShowPrompt(confirmed =>
            {
                if (!confirmed) return;
                DoDelete(ctrl, false);
            }, string.Format(Resources.You_are_about_to_permanently_destroy__0__, $"\u201c{Path.GetFileNameWithoutExtension(ctrl.FileName)}\u201d") + '\n' + Resources.Are_you_sure_, 
                Resources.Yes, Resources.Cancel);
        }

        private async void OnClickInspect(object o, EventArgs e)
        {
            var ctrl = (ResponsiveThumbnail)o;
            await this.Model.FileWatcherFactory.CreateInspectionPanel(ctrl.FileName);
        }

        private async void DoDelete(ThumbnailBase ctrl, bool sendToRecycleBin)
        {
            if (!await FileUtil.DeleteAsync(ctrl.FileName, sendToRecycleBin))
            {
                ScreenNotification.ShowNotification(string.Format(Resources.Failed_to_delete_image__0__, $"\u201c{Path.GetFileNameWithoutExtension(ctrl.FileName)}\u201d"), ScreenNotification.NotificationType.Error);
                GameService.Content.PlaySoundEffectByName("error");
                return;
            }
            ScreenshotManagerModule.ModuleInstance.DeleteSfx.Play(GameService.GameIntegration.Audio.Volume, 0, 0);
            this.View.ThumbnailFlowPanel.RemoveChild(ctrl);
            ctrl.Dispose();
            this.View.ThumbnailFlowPanel.SortChildren<ResponsiveThumbnail>(SortThumbnails);
        }

        private void OnFavoriteChanged(object o, EventArgs e)
        {
            this.View.ThumbnailFlowPanel.SortChildren<ResponsiveThumbnail>(SortThumbnails);
        }

        public async Task LoadTexture(AsyncTexture2D texture, string fileName)
        {
            await TextureUtil.GetThumbnail(fileName).ContinueWith(t => texture.SwapTexture(t.Result));
        }

        protected override void Unload()
        {
            this.Model.FileWatcherFactory.FileAdded -= OnScreenShotAdded;
            this.Model.FileWatcherFactory.FileDeleted -= OnScreenShotDeleted;
            this.Model.FileWatcherFactory.FileRenamed -= OnScreenShotRenamed;
            this.Model.Dispose();

            // Saving favorites
            var favorites = this.View.ThumbnailFlowPanel.Children.Where(x => x.GetType() == typeof(ResponsiveThumbnail))
                                                                                  .Cast<ResponsiveThumbnail>().Where(x => x.IsFavorite)
                                                                                  .Select(y => Path.GetFileName(y.FileName));
            ScreenshotManagerModule.ModuleInstance.Favorites.Value = favorites.ToList();
        }

        public int SortThumbnails(ResponsiveThumbnail x, ResponsiveThumbnail y)
        {
            var fileNameX = Path.GetFileNameWithoutExtension(x.FileName);
            var fileNameY = Path.GetFileNameWithoutExtension(y.FileName);
            x.Visible = fileNameX.Contains(this.View.SearchBox.Text);
            y.Visible = fileNameY.Contains(this.View.SearchBox.Text);
            return x.Visible && y.Visible ? y.IsFavorite.CompareTo(x.IsFavorite) : string.Compare(fileNameX, fileNameY, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

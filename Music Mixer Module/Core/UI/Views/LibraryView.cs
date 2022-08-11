using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer.Core.UI.Views
{
    internal class LibraryView : View<LibraryPresenter>
    {
        public LibraryView(MainModel model)
        {
            this.WithPresenter(new LibraryPresenter(this, model));
        }

        public FlowPanel MusicContextPanel;

        private bool _loading;
        public bool Loading
        {
            get => _loading;
            set
            {
                if (_loading == value) return;
                _loading = value;
                _importBtn.Enabled = !value;
                if (value)
                    _addNewLoadingSpinner?.Show();
                else
                {
                    _importBtn.BasicTooltipText = "Import Video Link from Clipboard";
                    _addNewLoadingSpinner?.Hide();
                }
            }
        }

        private LoadingSpinner _addNewLoadingSpinner;
        private ClipboardButton _importBtn;
        private List<MusicContextModel> _initialModels;

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            progress.Report("Loading playlist...");
            var models = MusicMixer.Instance.DataService.FindWhere(x =>
                x.State == this.Presenter.Model.State
                && x.MapIds.Contains(this.Presenter.Model.MapId)
                && x.DayTimes.Contains(this.Presenter.Model.DayCycle)
                && (!x.MountTypes.Any() || x.MountTypes.Contains(this.Presenter.Model.MountType)));
            _initialModels = models.Select(x => x.ToModel()).ToList();
            foreach (var model in _initialModels)
            {
                MusicMixer.Instance.DataService.GetThumbnail(model);
            }
            progress.Report(null);
            return true;
        }

        protected override void Build(Container buildPanel)
        {
            Label mapLabel = null;
            var mapName = MusicMixer.Instance.MapService.GetMapName(this.Presenter.Model.MapId);
            if (!string.IsNullOrEmpty(mapName)) {
                mapLabel = new Label
                {
                    Parent = buildPanel,
                    Width = buildPanel.ContentRegion.Width - Panel.RIGHT_PADDING,
                    Height = 32,
                    Location = new Point(buildPanel.ContentRegion.X, 0),
                    Text = mapName,
                    Font = GameService.Content.DefaultFont32,
                    StrokeText = true,
                    TextColor = new Color(238, 221, 171),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Middle
                };
            }

            this.MusicContextPanel = new FlowPanel
            {
                Parent = buildPanel,
                Width = buildPanel.ContentRegion.Width,
                Height = buildPanel.ContentRegion.Height,
                Location = new Point(buildPanel.ContentRegion.X, mapLabel?.Bottom ?? 0),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = true,
                Collapsed = false,
                ShowBorder = true,
                ShowTint = true
            };

            foreach (var model in _initialModels)
            {
                this.Presenter.Add(model);
                model.Deleted += OnModelDeleted;
            }

            _importBtn = new ClipboardButton
            {
                Parent = buildPanel,
                Size = new Point(42, 42),
                Location = new Point(this.MusicContextPanel.Right - 84, this.MusicContextPanel.Height - 64),
                BasicTooltipText = "Import Video Link from Clipboard"
            };
            _importBtn.Click += OnImportFromClipboardBtnClick;

            _addNewLoadingSpinner = new LoadingSpinner
            {
                Parent = buildPanel,
                Location = _importBtn.Location,
                Size = _importBtn.Size,
                Visible = false
            };

            base.Build(buildPanel);
        }
        private async void OnImportFromClipboardBtnClick(object o, MouseEventArgs e)
        {
            if (this.Loading) return;
            this.Loading = true;
            _addNewLoadingSpinner.Show();
            GameService.Content.PlaySoundEffectByName("button-click");
            var progress = new Progress<string>();
            progress.ProgressChanged += OnAddProgressChanged;
            await this.Presenter.AddNew(progress);
        }

        private void OnAddProgressChanged(object o, string e)
        {
            _importBtn.BasicTooltipText = e;
            _addNewLoadingSpinner.BasicTooltipText = e;
        }

        private void OnModelDeleted(object o, EventArgs e)
        {
            MusicMixer.Instance.DataService.Delete((MusicContextModel)o);
        }
    }
}

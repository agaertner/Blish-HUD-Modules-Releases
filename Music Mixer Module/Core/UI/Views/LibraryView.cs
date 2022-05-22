using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Presenters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer.Core.UI.Views
{
    internal class LibraryView : View<LibraryPresenter>
    {
        private const int MARGIN_BOTTOM = 10;

        private const string FILTER_MAP = "Current Map";
        private const string FILTER_ALL = "All";

        public FlowPanel MusicContextPanel;
        private LoadingSpinner _addNewLoadingSpinner;
        private ClipboardButton _importBtn;

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

        public LibraryView(LibraryModel model)
        {
            this.WithPresenter(new LibraryPresenter(this, model));
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            return await Task.Run(async () =>
            {
                this.Presenter.Model.Pager = new Pager(await MusicMixer.Instance.DataService.Count(), 1, 20, 20);

                this.Presenter.Model.MusicContextModels = (await MusicMixer.Instance.DataService.GetAll()).Select(x => x.ToModel()).ToList();

                foreach (var model in this.Presenter.Model.MusicContextModels)
                {
                    await MusicMixer.Instance.DataService.GetThumbnail(model);
                }
                return true;
            });
        }

        protected override void Build(Container buildPanel)
        {
            var searchBar = new TextBox
            {
                Parent = buildPanel,
                MaxLength = 256,
                Location = new Point(0, 5),
                Size = new Point(150, 32),
                PlaceholderText = "Search..."
            };
            searchBar.EnterPressed += OnSearchFilterChanged;

            // Sort drop down
            var ddSortMethod = new Dropdown
            {
                Parent = buildPanel,
                Location = new Point(buildPanel.ContentRegion.Width - 5 - 150, 5),
                Width = 150
            };
            ddSortMethod.Items.Add(FILTER_ALL);
            ddSortMethod.Items.Add(FILTER_MAP);

            // Add all mounts as a filter
            foreach (var mount in Enum.GetValues(typeof(MountType)).Cast<MountType>().Where(x => x != MountType.None))
                ddSortMethod.Items.Add(mount.ToString());

            ddSortMethod.SelectedItem = FILTER_ALL;
            ddSortMethod.ValueChanged += OnSortChanged;

            this.MusicContextPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Width - 10, buildPanel.ContentRegion.Height - 150),
                Location = new Point(0, ddSortMethod.Bottom + MARGIN_BOTTOM),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = true,
                Collapsed = false,
                ShowTint = true,
                ShowBorder = true
            };

            _importBtn = new ClipboardButton
            {
                Parent = buildPanel,
                Size = new Point(42, 42),
                Location = new Point((buildPanel.ContentRegion.Width - 42) / 2, MusicContextPanel.Bottom + MARGIN_BOTTOM),
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
            foreach (var model in this.Presenter.Model.MusicContextModels) this.Presenter.Add(model);
        }

        private void OnSearchFilterChanged(object o, EventArgs e)
        {
            var text = ((TextBox)o).Text;
            text = string.IsNullOrEmpty(text) ? text : text.ToLowerInvariant();
            this.MusicContextPanel.SortChildren<MusicContextDetails>((x, y) =>
            {
                x.Visible = string.IsNullOrEmpty(text) || x.Title.ToLowerInvariant().Contains(text);
                y.Visible = string.IsNullOrEmpty(text) || y.Title.ToLowerInvariant().Contains(text);
                if (!x.Visible || !y.Visible) return 0;
                return string.Compare(x.Title, y.Title, StringComparison.InvariantCultureIgnoreCase);
            });
        }

        private void OnSortChanged(object o, ValueChangedEventArgs e)
        {
            var filter = ((Dropdown)o).SelectedItem;
            this.MusicContextPanel.SortChildren<MusicContextDetails>((x, y) =>
            {
                x.Visible = filter.Equals(FILTER_ALL) 
                            || x.Model.MountTypes.Any(q => q.ToString().Equals(filter))
                            || x.Model.MapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id);

                y.Visible = filter.Equals(FILTER_ALL)
                            || y.Model.MountTypes.Any(q => q.ToString().Equals(filter))
                            || y.Model.MapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id);

                if (!x.Visible || !y.Visible) return 0;
                return string.Compare(x.Title, y.Title, StringComparison.InvariantCultureIgnoreCase);
            });
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
    }
}

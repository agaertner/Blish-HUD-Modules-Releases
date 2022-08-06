using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.UI.Controls;
using System;
using System.Threading.Tasks;
using Blish_HUD.Input;
using Nekres.Music_Mixer.Core.Services;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Presenters;

namespace Nekres.Music_Mixer.Core.UI.Views.StateViews
{
    internal class MainView : View<MainPresenter>
    {
        public ViewContainer ContentRegion { get; set; }

        private const int MARGIN_BOTTOM = 10;

        private const string FILTER_ALL = "All";

        private FlowPanel _regionsPanel;

        private ViewContainer _contentView;
        public MainView(Gw2StateService.State state)
        {
            this.WithPresenter(new MainPresenter(this, new MainModel{ State = state}));
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            return true;
        }

        protected override void Build(Container buildPanel)
        {
            var searchBar = new TextBox
            {
                Parent = buildPanel,
                MaxLength = 256,
                Location = new Point(buildPanel.ContentRegion.X, 0),
                Size = new Point(260, 32),
                PlaceholderText = "Search..."
            };
            searchBar.EnterPressed += OnSearchFilterChanged;

            _regionsPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(260, buildPanel.ContentRegion.Height),
                Location = new Point(buildPanel.ContentRegion.X, searchBar.Bottom + MARGIN_BOTTOM),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = true,
                Collapsed = false,
                ShowTint = true,
                ShowBorder = true
            };
            foreach (var region in MusicMixer.Instance.MapService.RegionNames)
            {
                var regionBtn = new RegionEntry(region.Key, region.Value)
                {
                    Parent = _regionsPanel,
                    Width = _regionsPanel.ContentRegion.Width - 2,
                    Height = 24
                };
                regionBtn.Click += OnRegionButtonClick;
            }

            _contentView = new ViewContainer
            {
                Parent = buildPanel,
                Width = buildPanel.ContentRegion.Width - _regionsPanel.Width,
                Height = buildPanel.ContentRegion.Height,
                Location = new Point(_regionsPanel.Right, 0),
                ShowTint = true
            };
            base.Build(buildPanel);
        }

        private void OnSearchFilterChanged(object o, EventArgs e)
        {
            var text = ((TextBox)o).Text;
            text = string.IsNullOrEmpty(text) ? text : text.ToLowerInvariant();
            this._regionsPanel.SortChildren<MusicContextDetails>((x, y) =>
            {
                x.Visible = string.IsNullOrEmpty(text) || x.Title.ToLowerInvariant().Contains(text);
                y.Visible = string.IsNullOrEmpty(text) || y.Title.ToLowerInvariant().Contains(text);
                if (!x.Visible || !y.Visible) return 0;
                return string.Compare(x.Title, y.Title, StringComparison.InvariantCultureIgnoreCase);
            });
        }

        private void OnRegionButtonClick(object o, MouseEventArgs e)
        {
            this.Presenter.Model.RegionId = ((RegionEntry)o).RegionId;
            _contentView.Show(new ContextView(this.Presenter.Model));
        }
    }
}

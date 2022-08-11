using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Presenters;
using System;
using Nekres.Music_Mixer.Core.Services;

namespace Nekres.Music_Mixer.Core.UI.Views.StateViews
{
    internal class MainView : View<MainPresenter>
    {
        private const int MARGIN_BOTTOM = 10;

        private FlowPanel _regionsPanel;

        public ViewContainer Content { get; private set; }

        public MainView(MainModel model)
        {
            this.WithPresenter(new MainPresenter(this, model));
        }

        protected override void Build(Container buildPanel)
        {
            var searchBar = new TextBox
            {
                Parent = buildPanel,
                MaxLength = 256,
                Location = new Point(buildPanel.ContentRegion.X, 0),
                Size = new Point(258, 32),
                PlaceholderText = "Region..."
            };
            searchBar.TextChanged += OnSearchFilterChanged;

            _regionsPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(260, buildPanel.ContentRegion.Height - searchBar.Height - MARGIN_BOTTOM),
                Location = new Point(buildPanel.ContentRegion.X, searchBar.Bottom + MARGIN_BOTTOM),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = true,
                Collapsed = false,
                ShowTint = true,
                ShowBorder = true
            };

            // PvP, WvW regions hardcoded...
            var regions = this.Presenter.Model.State == Gw2StateService.State.Competitive ? 
                new[] { 6, 7 } : MusicMixer.Instance.MapService.GetRegionsForContinent(this.Presenter.Model.ContinentId);

            foreach (var regionId in regions)
            {
                if (!MusicMixer.Instance.MapService.RegionHasMaps(regionId)) continue;
                var regionBtn = new RegionEntry(regionId, MusicMixer.Instance.MapService.GetRegionName(regionId))
                {
                    Parent = _regionsPanel,
                    Width = _regionsPanel.ContentRegion.Width,
                    Height = 24
                };
                regionBtn.Click += OnRegionButtonClick;
            }

            this.Content = new ViewContainer
            {
                Parent = buildPanel,
                Width = buildPanel.ContentRegion.Width - _regionsPanel.Width - 25,
                Height = buildPanel.ContentRegion.Height,
                Location = new Point(_regionsPanel.Right, 0)
            };
            base.Build(buildPanel);
        }

        private void OnSearchFilterChanged(object o, EventArgs e)
        {
            var text = ((TextBox)o).Text;
            text = string.IsNullOrEmpty(text) ? text : text.ToLowerInvariant();
            _regionsPanel.SortChildren<RegionEntry>((x, y) =>
            {
                x.Visible = string.IsNullOrEmpty(text) || x.RegionName.ToLowerInvariant().Contains(text);
                y.Visible = string.IsNullOrEmpty(text) || y.RegionName.ToLowerInvariant().Contains(text);
                if (!x.Visible || !y.Visible) return 0;
                return string.Compare(x.RegionName, y.RegionName, StringComparison.InvariantCultureIgnoreCase);
            });
        }

        private void OnRegionButtonClick(object o, MouseEventArgs e)
        {
            GameService.Content.PlaySoundEffectByName($"tab-swap-{RandomUtil.GetRandom(1, 5)}");
            this.Presenter.Model.RegionId = ((RegionEntry)o).RegionId;
            this.Content.Show(new ContextView(this.Presenter.Model));
        }
    }
}

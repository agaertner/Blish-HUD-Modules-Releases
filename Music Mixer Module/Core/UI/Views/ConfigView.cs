using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Presenters;
using System;
using Blish_HUD.Input;
using Nekres.Music_Mixer.Core.Services;

namespace Nekres.Music_Mixer.Core.UI.Views
{
    internal class ConfigView : View<ConfigPresenter>
    {
        private const int MARGIN_BOTTOM = 10;

        private FlowPanel _regionsPanel;

        public Container Parent { get; private set; }

        public ViewContainer Content { get; private set; }

        public ConfigView(ConfigModel model)
        {
            this.WithPresenter(new ConfigPresenter(this, model));
        }

        protected override void Build(Container buildPanel)
        {
            this.Parent = buildPanel;

            var musicDetails = new MusicContextDetails(this.Presenter.Model.MusicContextModel)
            {
                Parent = buildPanel,
                Width = buildPanel.ContentRegion.Width,
                Height = 100,
                Location = new Point(buildPanel.ContentRegion.X, 0),
                Editable = false,
                Deletable = false
            };

            var searchBar = new TextBox
            {
                Parent = buildPanel,
                MaxLength = 256,
                Location = new Point(buildPanel.ContentRegion.X, musicDetails.Bottom + MARGIN_BOTTOM),
                Size = new Point(258, 32),
                PlaceholderText = "Region..."
            };
            searchBar.TextChanged += OnSearchFilterChanged;

            _regionsPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(260, buildPanel.ContentRegion.Height - searchBar.Height - musicDetails.Height - MARGIN_BOTTOM),
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
                Height = buildPanel.ContentRegion.Height - musicDetails.Height,
                Location = new Point(_regionsPanel.Right, musicDetails.Bottom)
            };
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
            this.Content.Show(new ConfigContextView(this.Presenter.Model));
        }
    }
}
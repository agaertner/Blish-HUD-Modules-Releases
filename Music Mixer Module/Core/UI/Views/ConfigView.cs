using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Input;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.Services;
using Nekres.Music_Mixer.Core.UI.Controls;
using Color = Microsoft.Xna.Framework.Color;
using MountType = Gw2Sharp.Models.MountType;

namespace Nekres.Music_Mixer.Core.UI.Views
{
    internal class ConfigView : View<MusicContextConfigPresenter>
    {
        private const int MARGIN = 10;
        private IList<Map> _maps;

        private FlowPanel _mapsExclusionPanel;

        public ConfigView(MusicContextModel model)
        {
            _maps = new List<Map>();
            this.WithPresenter(new MusicContextConfigPresenter(this, model));
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            var mapIds = this.Presenter.Model.ExcludedMapIds.ToList();
            if (!mapIds.Any()) return true;
            _maps = (await MusicMixer.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps.ManyAsync(mapIds)).ToList();
            return _maps.Any();
        }

        protected override void Build(Container buildPanel)
        {
            var thumbnail = new LoadingImage(this.Presenter.Model.Thumbnail)
            {
                Parent = buildPanel,
                Size = new Point(128, 72),
                Location = new Point(Panel.LEFT_PADDING, Panel.TOP_PADDING)
            };

            var artistHeight = GameService.Content.DefaultFont12.MeasureString(this.Presenter.Model.Artist).Height;

            var title = new Label
            {
                Parent = buildPanel,
                Text = this.Presenter.Model.Title,
                Size = new Point(buildPanel.Width - thumbnail.Width - MARGIN * 2 - 100, thumbnail.Height - (int)artistHeight),
                Location = new Point(thumbnail.Right + MARGIN, thumbnail.Location.Y),
                Font = GameService.Content.DefaultFont18,
                StrokeText = true,
                WrapText = true,
                VerticalAlignment = VerticalAlignment.Top
            };

            var artist = new Label
            {
                Parent = buildPanel,
                Text = this.Presenter.Model.Artist,
                Size = new Point(title.Width, title.Height),
                Location = new Point(title.Location.X, thumbnail.Bottom - (int)artistHeight),
                Font = GameService.Content.DefaultFont12,
                TextColor = Color.LightGray,
                VerticalAlignment = VerticalAlignment.Top
            };

            var duration = new Label
            {
                Parent = buildPanel,
                Text = this.Presenter.Model.Duration.ToShortForm(),
                Size = new Point(buildPanel.ContentRegion.Width - thumbnail.Width - title.Width, thumbnail.Height),
                Location = new Point(title.Right + 5, thumbnail.Location.Y),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.LightGray
            };

            // MapIds Excluded selection
            _mapsExclusionPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Width / 2, buildPanel.ContentRegion.Height - (thumbnail.Height + Panel.BOTTOM_PADDING + StandardButton.STANDARD_CONTROL_HEIGHT)),
                Location = new Point(buildPanel.ContentRegion.X, thumbnail.Bottom + Panel.BOTTOM_PADDING),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = true,
                Collapsed = false,
                ShowTint = true,
                ShowBorder = true
            };
            foreach (var id in this.Presenter.Model.ExcludedMapIds) CreateMapEntry(id, _mapsExclusionPanel, OnExcludedMapClick);

            var btnExcludeMap = new StandardButton
            {
                Parent = buildPanel,
                Size = new Point(150, StandardButton.STANDARD_CONTROL_HEIGHT),
                Location = new Point(_mapsExclusionPanel.Location.X + (_mapsExclusionPanel.Width - 150) / 2, _mapsExclusionPanel.Location.Y + _mapsExclusionPanel.Height),
                Text = "Exclude Map"
            };
            btnExcludeMap.Click += BtnExcludeMap_Click;
        }

        private async void BtnExcludeMap_Click(object o, MouseEventArgs e)
        {
            GameService.Content.PlaySoundEffectByName("button-click");
            if (this.Presenter.Model.ExcludedMapIds.Any(id => id.Equals(GameService.Gw2Mumble.CurrentMap.Id))) return;
            await MusicMixer.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps
                .GetAsync(GameService.Gw2Mumble.CurrentMap.Id).ContinueWith(t =>
                {
                    if (t.IsFaulted) return;
                    var map = t.Result;
                    _maps.Add(map);
                    this.Presenter.Model.ExcludedMapIds.Add(map.Id);
                    CreateMapEntry(map.Id, _mapsExclusionPanel, OnExcludedMapClick);
                });
        }

        private void CreateMapEntry(int mapId, FlowPanel parent, EventHandler<MouseEventArgs> clickAction)
        {
            var map = _maps.First(x => x.Id == mapId);
            var mapEntry = new MapEntry(map.Id, map.Name)
            {
                Parent = parent,
                Size = new Point(parent.ContentRegion.Width - (int)parent.OuterControlPadding.X * 2, StandardButton.STANDARD_CONTROL_HEIGHT)
            };
            mapEntry.Click += clickAction;
        }

        private void OnExcludedMapClick(object o, MouseEventArgs e)
        {
            GameService.Content.PlaySoundEffectByName("button-click");
            var ctrl = (MapEntry)o;
            this.Presenter.Model.ExcludedMapIds.Remove(ctrl.MapId);
            ctrl.Dispose();
        }
    }
}
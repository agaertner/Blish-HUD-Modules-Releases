using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Music_Mixer.Core.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Input;
using Nekres.Music_Mixer.Core.Services;
using Nekres.Music_Mixer.Core.UI.Controls;

namespace Nekres.Music_Mixer.Core.UI.Views
{
    internal class MapSelectionView : View
    {
        private Dictionary<ContinentFloorRegionMap, Texture2D> _maps;

        private MainModel _model;

        private FlowPanel _mapsPanel;

        public MapSelectionView(MainModel model)
        {
            _model = model;
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            progress.Report("Loading maps...");
            return await MusicMixer.Instance.MapService.GetMapsForRegion(_model.ContinentId, _model.RegionId).ContinueWith(t =>
            {
                _maps = t.Result;
                progress.Report(null);
                return true;
            });
        }

        protected override void Build(Container buildPanel)
        {
            var searchBar = new TextBox
            {
                Parent = buildPanel,
                MaxLength = 256,
                Location = new Point(buildPanel.ContentRegion.X + Panel.RIGHT_PADDING, 0),
                Size = new Point(buildPanel.ContentRegion.Width - Panel.RIGHT_PADDING * 2, 32),
                PlaceholderText = "Map..."
            };
            searchBar.TextChanged += OnSearchFilterChanged;

            _mapsPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Width, buildPanel.ContentRegion.Height - searchBar.Height),
                Location = new Point(buildPanel.ContentRegion.X, searchBar.Bottom + 5),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = true,
                Collapsed = false,
                ShowTint = true,
                ShowBorder = true
            };

            if (_maps == null) return;
            foreach (var map in _maps)
            {
                var bttn = new MapThumb(map.Key.Id, map.Key.Name, map.Value)
                {
                    Parent = _mapsPanel,
                    Width = 267,
                    Height = 267,
                };
                bttn.Click += (_,_) =>
                {
                    GameService.Content.PlaySoundEffectByName($"tab-swap-{RandomUtil.GetRandom(1, 5)}");
                    _model.MapId = bttn.Id;
                    ((ViewContainer)buildPanel).Show(new LibraryView(_model));
                };
            } 
            base.Build(buildPanel);
        }

        private void OnSearchFilterChanged(object o, EventArgs e)
        {
            var text = ((TextBox)o).Text;
            text = string.IsNullOrEmpty(text) ? text : text.ToLowerInvariant();
            _mapsPanel.SortChildren<MapThumb>((x, y) =>
            {
                x.Visible = string.IsNullOrEmpty(text) || x.Name.ToLowerInvariant().Contains(text);
                y.Visible = string.IsNullOrEmpty(text) || y.Name.ToLowerInvariant().Contains(text);
                if (!x.Visible || !y.Visible) return 0;
                return string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
            });
        }
    }
}

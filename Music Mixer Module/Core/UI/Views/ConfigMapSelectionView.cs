using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer.Core.UI.Views
{
    internal class ConfigMapSelectionView : View
    {
        private IEnumerable<int> _maps;

        private ConfigModel _model;

        private FlowPanel _mapsPanel;

        public ConfigMapSelectionView(ConfigModel model)
        {
            _model = model;
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            progress.Report("Loading maps...");
            _maps = MusicMixer.Instance.MapService.GetMapsForRegion(_model.RegionId);
            progress.Report(null);
            return true;
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
            foreach (var mapId in _maps)
            {
                var bttn = new MapThumb(mapId, MusicMixer.Instance.MapService.GetMapName(mapId), MusicMixer.Instance.MapService.GetMapThumb(mapId))
                {
                    Parent = _mapsPanel,
                    Width = 267,
                    Height = 267,
                    Active = _model.MusicContextModel.MapIds.Contains(mapId)
                };
                bttn.Click += (_,_) =>
                {
                    if (_model.MusicContextModel.MapIds.Contains(bttn.Id))
                    {
                        if (_model.MusicContextModel.MapIds.Count == 1)
                        {
                            ScreenNotification.ShowNotification("You may not disable all maps!", ScreenNotification.NotificationType.Error);
                            GameService.Content.PlaySoundEffectByName("error");
                            return;
                        }
                        _model.MusicContextModel.MapIds.Remove(bttn.Id);
                        bttn.Active = false;
                    }
                    else
                    {
                        _model.MusicContextModel.MapIds.Add(bttn.Id);
                        bttn.Active = true;
                    }
                    GameService.Content.PlaySoundEffectByName("color-change");
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

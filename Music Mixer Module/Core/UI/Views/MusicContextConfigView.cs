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
    internal class MusicContextConfigView : View<MusicContextConfigPresenter>
    {
        private const int MARGIN = 10;
        private bool _deleted;
        private IList<Map> _maps;

        private FlowPanel _mapsPanel;
        private FlowPanel _mapsExclusionPanel;

        public MusicContextConfigView(MusicContextModel model)
        {
            this.WithPresenter(new MusicContextConfigPresenter(this, model));
            _maps = new List<Map>();
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            var mapIds = this.Presenter.Model.MapIds.Concat(this.Presenter.Model.ExcludedMapIds).ToList();
            if (!mapIds.Any()) return true;
            _maps = (await MusicMixer.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps.ManyAsync(mapIds)).ToList();
            return _maps.Any();
        }

        protected override async void Unload()
        {
            if (!_deleted) await MusicMixer.Instance.DataService.Upsert(this.Presenter.Model);
            base.Unload();
        }

        protected override void Build(Container buildPanel)
        {
            var thumbnail = new LoadingImage(this.Presenter.Model.Thumbnail)
            {
                Parent = buildPanel,
                Size = new Point(256, 144),
                Location = new Point(Panel.LEFT_PADDING, Panel.TOP_PADDING)
            };

            var title = new Label
            {
                Parent = buildPanel,
                Text = this.Presenter.Model.Title,
                Size = new Point(buildPanel.ContentRegion.Width - thumbnail.Width - MARGIN * 2 - 100, thumbnail.Height / 2),
                Location = new Point(thumbnail.Right + MARGIN, thumbnail.Location.Y),
                Font = GameService.Content.DefaultFont18,
                StrokeText = true,
                WrapText = true
            };

            var artist = new Label
            {
                Parent = buildPanel,
                Text = this.Presenter.Model.Artist,
                Size = new Point(title.Width, title.Height),
                Location = new Point(title.Location.X, title.Bottom),
                Font = GameService.Content.DefaultFont12,
                TextColor = Color.LightGray,
                VerticalAlignment = VerticalAlignment.Top
            };

            var duration = new Label
            {
                Parent = buildPanel,
                Text = this.Presenter.Model.Duration.ToShortForm(),
                Size = new Point(buildPanel.ContentRegion.Width - thumbnail.Width - title.Width, thumbnail.Height),
                Location = new Point(title.Right, thumbnail.Location.Y),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.LightGray
            };

            // MapIds selection
            _mapsPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(200, buildPanel.ContentRegion.Height - 350),
                Location = new Point(0, buildPanel.ContentRegion.Height / 2 - Panel.BOTTOM_PADDING),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = true,
                Collapsed = false,
                ShowTint = true,
                ShowBorder = true
            };
            foreach (var id in this.Presenter.Model.MapIds) CreateMapEntry(id, _mapsPanel, OnMapClick);

            var btnIncludeMap = new StandardButton
            {
                Parent = buildPanel,
                Size = new Point(150, StandardButton.STANDARD_CONTROL_HEIGHT),
                Location = new Point(_mapsPanel.Location.X + (_mapsPanel.Width - 150) / 2, _mapsPanel.Location.Y + _mapsPanel.Height),
                Text = "Include Map"
            };
            btnIncludeMap.Click += BtnIncludeMap_Click;

            // MapIds Excluded selection
            _mapsExclusionPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(_mapsPanel.Width, _mapsPanel.Height),
                Location = new Point(_mapsPanel.Right + 5, _mapsPanel.Location.Y),
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

            var statesPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(_mapsPanel.Width, _mapsPanel.Height),
                Location = new Point(_mapsExclusionPanel.Right + 5, _mapsPanel.Location.Y),
                FlowDirection = ControlFlowDirection.TopToBottom,
                ControlPadding = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = false,
                Collapsed = false
            };
            foreach (var state in Enum.GetValues(typeof(Gw2StateService.State)).Cast<Gw2StateService.State>())
            {
                var cbx = new Checkbox
                {
                    Parent = statesPanel,
                    Text = state.ToString(),
                    BasicTooltipText = state.GetDescription(),
                    Size = new Point(statesPanel.ContentRegion.Width - 10, 26),
                    Checked = this.Presenter.Model.States.Contains(state)
                };
                cbx.CheckedChanged += OnStateCheckboxCheckedChange;
            }

            var mountPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(_mapsPanel.Width, _mapsPanel.Height),
                Location = new Point(statesPanel.Right + 5, _mapsPanel.Location.Y),
                FlowDirection = ControlFlowDirection.TopToBottom,
                ControlPadding = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = false,
                Collapsed = false
            };
            foreach (var mount in Enum.GetValues(typeof(MountType)).Cast<MountType>())
            {
                if (mount == MountType.None) continue;
                var cbx = new Checkbox
                {
                    Parent = mountPanel,
                    Text = mount.ToString(),
                    Size = new Point(mountPanel.ContentRegion.Width - 10, 26),
                    Checked = this.Presenter.Model.MountTypes.Contains(mount)
                };
                cbx.CheckedChanged += OnMountCheckboxCheckedChange;
            }

            var dayTimePanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(_mapsPanel.Width, _mapsPanel.Height / 2),
                Location = new Point(_mapsPanel.Location.X, _mapsPanel.Top - _mapsPanel.Height / 2 - MARGIN),
                FlowDirection = ControlFlowDirection.TopToBottom,
                ControlPadding = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = false,
                Collapsed = false
            };
            foreach (var dayTime in Enum.GetValues(typeof(TyrianTime)).Cast<TyrianTime>())
            {
                if (dayTime == TyrianTime.None) continue;
                var cbx = new Checkbox
                {
                    Parent = dayTimePanel,
                    Text = dayTime.ToString(),
                    Size = new Point(dayTimePanel.ContentRegion.Width - 10, 26),
                    Checked = this.Presenter.Model.DayTimes.Contains(dayTime)
                };
                cbx.CheckedChanged += OnDayTimeCheckboxCheckedChange;
            }

            var delBtn = new DeleteButton(MusicMixer.Instance.ContentsManager)
            {
                Parent = buildPanel,
                Size = new Point(42, 42),
                Location = new Point(buildPanel.ContentRegion.Width - 42, btnIncludeMap.Location.Y + btnIncludeMap.Height - 42),
                BasicTooltipText = "Delete"
            };
            delBtn.Click += DeleteButton_Click;
        }

        private async void BtnIncludeMap_Click(object o, MouseEventArgs e)
        {
            GameService.Content.PlaySoundEffectByName("button-click");
            if (this.Presenter.Model.MapIds.Any(id => id.Equals(GameService.Gw2Mumble.CurrentMap.Id))) return;
            await MusicMixer.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps
                .GetAsync(GameService.Gw2Mumble.CurrentMap.Id).ContinueWith(t =>
                {
                    if (t.IsFaulted) return;
                    var map = t.Result;
                    _maps.Add(map);
                    this.Presenter.Model.MapIds.Add(map.Id);
                    CreateMapEntry(map.Id, _mapsPanel, OnMapClick);
                });
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

        private void OnMapClick(object o, MouseEventArgs e)
        {
            GameService.Content.PlaySoundEffectByName("button-click");
            var ctrl = (MapEntry)o;
            this.Presenter.Model.MapIds.Remove(ctrl.MapId);
            ctrl.Dispose();
        }

        private void OnExcludedMapClick(object o, MouseEventArgs e)
        {
            GameService.Content.PlaySoundEffectByName("button-click");
            var ctrl = (MapEntry)o;
            this.Presenter.Model.ExcludedMapIds.Remove(ctrl.MapId);
            ctrl.Dispose();
        }

        private void OnStateCheckboxCheckedChange(object o, CheckChangedEvent e)
        {
            var ctrl = (Checkbox)o;
            var state = (Gw2StateService.State)Enum.Parse(typeof(Gw2StateService.State), ctrl.Text);
            if (e.Checked)
            {
                this.Presenter.Model.States.Add(state);
                return;
            }
            this.Presenter.Model.States.Remove(state);
        }

        private void OnMountCheckboxCheckedChange(object o, CheckChangedEvent e)
        {
            var ctrl = (Checkbox)o;
            var mount = (MountType)Enum.Parse(typeof(MountType), ctrl.Text);
            if (e.Checked)
            {
                this.Presenter.Model.MountTypes.Add(mount);
                return;
            }
            this.Presenter.Model.MountTypes.Remove(mount);
        }

        private void OnDayTimeCheckboxCheckedChange(object o, CheckChangedEvent e)
        {
            var ctrl = (Checkbox)o;
            var dayTime = (TyrianTime)Enum.Parse(typeof(TyrianTime), ctrl.Text);
            if (e.Checked)
            {
                this.Presenter.Model.DayTimes.Add(dayTime);
                return;
            }
            this.Presenter.Model.DayTimes.Remove(dayTime);
        }

        private async void DeleteButton_Click(object o, MouseEventArgs e)
        {
            _deleted = true;
            await this.Presenter.Delete();
            ((DeleteButton)o).Parent.Hide();
        }
    }
}

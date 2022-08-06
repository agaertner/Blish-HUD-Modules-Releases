using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Nekres.Chat_Shorts.UI.Controls;
using Nekres.Chat_Shorts.UI.Models;
using Nekres.Chat_Shorts.UI.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Chat_Shorts.UI.Views
{
    internal class MacroEditView : View<MacroEditPresenter>
    {
        private bool _deleted;
        private IList<Map> _maps;

        private FlowPanel _mapsPanel;
        private FlowPanel _mapsExclusionPanel;

        public MacroEditView(MacroModel model)
        {
            this.WithPresenter(new MacroEditPresenter(this, model));
            _maps = new List<Map>();
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            var mapIds = this.Presenter.Model.MapIds.Concat(this.Presenter.Model.ExcludedMapIds).ToList();
            if (!mapIds.Any()) return true;
            _maps = (await ChatShorts.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps.ManyAsync(mapIds)).ToList();
            return _maps.Any();
        }

        protected override async void Unload()
        {
            if (!_deleted) await ChatShorts.Instance.DataService.UpsertMacro(this.Presenter.Model);
            base.Unload();
        }

        protected override void Build(Container buildPanel)
        {
            var editTitle = new TextBox
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Width, 42),
                Location = new Point(0,0),
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular),
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = this.Presenter.Model.Title
            };
            editTitle.InputFocusChanged += EditTitle_InputFocusChanged;

            var editText = new MultilineTextBox
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Width, buildPanel.ContentRegion.Height / 2 - 60),
                Location = new Point(0, editTitle.Bottom + Panel.BOTTOM_PADDING),
                Text = this.Presenter.Model.Text,
                PlaceholderText = "/say Hello World!"
            };
            editText.InputFocusChanged += EditText_InputFocusChanged;

            // MapIds selection
            _mapsPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(editText.Width / 4 - 5, buildPanel.ContentRegion.Height - editText.Height - 100),
                Location = new Point(0, editText.Bottom + Panel.BOTTOM_PADDING),
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

            // MapIds selection
            _mapsExclusionPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(editText.Width / 4 - 5, buildPanel.ContentRegion.Height - editText.Height - 100),
                Location = new Point(_mapsPanel.Right + 5, editText.Bottom + Panel.BOTTOM_PADDING),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
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

            // Squad Broadcast
            var squadBroadcastCheck = new Checkbox
            {
                Parent = buildPanel,
                Text = "Squad Broadcast",
                BasicTooltipText = "Send this text as a squad broadcast instead.",
                Location = new Point(_mapsExclusionPanel.Right + 10, _mapsExclusionPanel.Location.Y + 20),
                Checked = this.Presenter.Model.SquadBroadcast
            };
            squadBroadcastCheck.CheckedChanged += (_,e) => this.Presenter.Model.SquadBroadcast = e.Checked;

            // GameMode selection
            var labelGameMode = new Label
            {
                Parent = buildPanel,
                Text = "GameMode:",
                Size = new Point(100, 34),
                Location = new Point(squadBroadcastCheck.Location.X, squadBroadcastCheck.Bottom + Panel.BOTTOM_PADDING * 5)
            };

            var ddGameModeSelect = new Dropdown
            {
                Parent = buildPanel,
                Size = new Point(100, 30),
                Location = new Point(labelGameMode.Location.X + labelGameMode.Width + Panel.RIGHT_PADDING, labelGameMode.Location.Y)
            };
            foreach (var name in Enum.GetNames(typeof(GameMode))) ddGameModeSelect.Items.Add(name);
            ddGameModeSelect.SelectedItem = this.Presenter.Model.Mode.ToString();
            ddGameModeSelect.ValueChanged += DdGameModeSelect_ValueChanged;

            // Key Binding
            var keyAssigner = new KeybindingAssigner(this.Presenter.Model.KeyBinding)
            {
                Parent = buildPanel,
                Location = new Point(squadBroadcastCheck.Location.X, ddGameModeSelect.Location.Y + ddGameModeSelect.Height + Panel.BOTTOM_PADDING * 5),
                KeyBindingName = "Macro Key:"
            };
            keyAssigner.BindingChanged += KeyAssigner_BindingChanged;

            // Delete button
            var delBtn = new DeleteButton(ChatShorts.Instance.ContentsManager)
            {
                Parent = buildPanel,
                Size = new Point(42,42),
                Location = new Point(buildPanel.ContentRegion.Width - 42, btnIncludeMap.Location.Y + btnIncludeMap.Height - 42),
                BasicTooltipText = "Delete Macro"
            };
            delBtn.Click += DeleteButton_Click;
        }

        private void DdGameModeSelect_ValueChanged(object o, ValueChangedEventArgs e)
        {
            GameService.Content.PlaySoundEffectByName("button-click");
            this.Presenter.Model.Mode = (GameMode)Enum.Parse(typeof(GameMode), e.CurrentValue);
        }

        private void EditTitle_InputFocusChanged(object o, EventArgs e)
        {
            var ctrl = (TextBox)o;
            if (ctrl.Focused) return;
            this.Presenter.Model.Title = ctrl.Text;
            ((StandardWindow)ctrl.Parent).Title = $"Edit Macro - {ctrl.Text}";
        }

        private void EditText_InputFocusChanged(object o, EventArgs e)
        {
            var ctrl = (MultilineTextBox)o;
            if (ctrl.Focused) return;
            this.Presenter.Model.Text = ctrl.Text;
        }

        private async void BtnIncludeMap_Click(object o, MouseEventArgs e)
        {
            GameService.Content.PlaySoundEffectByName("button-click");
            if (this.Presenter.Model.MapIds.Any(id => id.Equals(GameService.Gw2Mumble.CurrentMap.Id))) return;
            await ChatShorts.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps
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
            await ChatShorts.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps
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
                Size = new Point(parent.ContentRegion.Width, StandardButton.STANDARD_CONTROL_HEIGHT)
            };
            mapEntry.Click += clickAction;
        }

        private void OnMapClick(object o, MouseEventArgs e)
        {
            GameService.Content.PlaySoundEffectByName("button-click");
            var ctrl = (MapEntry) o;
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

        private void KeyAssigner_BindingChanged(object o, EventArgs e)
        {
            this.Presenter.Model.NewKeysAssigned();
        }

        private async void DeleteButton_Click(object o, MouseEventArgs e)
        {
            _deleted = true;
            await this.Presenter.Delete();
            ((DeleteButton)o).Parent.Hide();
        }
    }
}

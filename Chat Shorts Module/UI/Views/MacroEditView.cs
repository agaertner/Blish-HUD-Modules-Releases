﻿using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Nekres.Chat_Shorts.UI.Controls;
using Nekres.Chat_Shorts.UI.Models;
using Nekres.Chat_Shorts.UI.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gw2Sharp.WebApi.V2.Models;
using Color = Microsoft.Xna.Framework.Color;

namespace Nekres.Chat_Shorts.UI.Views
{
    internal class MacroEditView : View<MacroEditPresenter>
    {
        private bool _deleted;
        private IReadOnlyList<Map> _maps;

        private FlowPanel _mapsPanel;

        public MacroEditView(MacroModel model)
        {
            this.WithPresenter(new MacroEditPresenter(this, model));
            _maps = new List<Map>();
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            if (!this.Presenter.Model.MapIds.Any()) return true;
            _maps = await ChatShorts.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps.ManyAsync(this.Presenter.Model.MapIds);
            return _maps != null;
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
                Size = new Point(buildPanel.ContentRegion.Width / 2, buildPanel.ContentRegion.Height / 2),
                Location = new Point(0, editTitle.Bottom + Panel.BOTTOM_PADDING),
                Text = this.Presenter.Model.Text
            };
            editText.InputFocusChanged += EditText_InputFocusChanged;

            // GameMode selection
            var labelGameMode = new Label
            {
                Parent = buildPanel,
                Text = "GameMode:",
                Size = new Point(100, 34),
                Location = new Point(editText.Width + Panel.RIGHT_PADDING, editText.Location.Y)
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

            // MapIds selection
            _mapsPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(editText.Width - Panel.RIGHT_PADDING, editText.Height / 2),
                Location = new Point(editText.Width + Panel.RIGHT_PADDING, ddGameModeSelect.Bottom + Panel.BOTTOM_PADDING),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = true,
                Collapsed = false,
                ShowTint = true,
                ShowBorder = true,
                BackgroundColor = Color.Black * 0.4f
            };

            foreach (var map in _maps) CreateMapEntry(map.Id, map.Name);

            var btnAddMap = new StandardButton
            {
                Parent = buildPanel,
                Size = new Point(150, StandardButton.STANDARD_CONTROL_HEIGHT),
                Location = new Point(_mapsPanel.Location.X + (_mapsPanel.Width - 150) / 2, _mapsPanel.Location.Y + _mapsPanel.Height),
                Text = "Add Current Map"
            };
            btnAddMap.Click += BtnAddMap_Click;

            var keyAssigner = new KeybindingAssigner(this.Presenter.Model.KeyBinding)
            {
                Parent = buildPanel,
                Location = new Point(0,0)
            };
            keyAssigner.Location = new Point(
                (buildPanel.ContentRegion.Width - keyAssigner.Width) / 2,
                editText.Bottom + Panel.BOTTOM_PADDING);
            keyAssigner.BindingChanged += KeyAssigner_BindingChanged;

            var delBtn = new DeleteButton(ChatShorts.Instance.ContentsManager)
            {
                Parent = buildPanel,
                Size = new Point(42,42),
                Location = new Point(buildPanel.ContentRegion.Width - 42, keyAssigner.Location.Y)
            };
            delBtn.Click += DeleteButton_Click;
        }

        private void DdGameModeSelect_ValueChanged(object o, ValueChangedEventArgs e)
        {
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

        private async void BtnAddMap_Click(object o, MouseEventArgs e)
        {
            await ChatShorts.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps
                .GetAsync(GameService.Gw2Mumble.CurrentMap.Id).ContinueWith(t =>
                    {
                        if (t.IsFaulted) return null;
                        var map = t.Result;
                        this.Presenter.Model.MapIds.Add(map.Id);
                        this.Presenter.Model.InvokeChanged();
                        CreateMapEntry(map.Id, map.Name);
                        return map;
                    });
        }

        private void CreateMapEntry(int mapId, string mapName)
        {
            var mapEntry = new MapEntry(mapId, mapName)
            {
                Parent = _mapsPanel,
                Size = new Point(_mapsPanel.ContentRegion.Width, StandardButton.STANDARD_CONTROL_HEIGHT)
            };
            mapEntry.Click += MapEntry_Click;
        }

        private void MapEntry_Click(object o, MouseEventArgs e)
        {
            var ctrl = (MapEntry) o;
            this.Presenter.Model.MapIds.Remove(ctrl.MapId);
            ctrl.Dispose();
            this.Presenter.Model.InvokeChanged();
        }

        private void KeyAssigner_BindingChanged(object o, EventArgs e)
        {
            this.Presenter.Model.InvokeChanged();
        }

        private async void DeleteButton_Click(object o, MouseEventArgs e)
        {
            _deleted = true;
            await this.Presenter.Delete();
            ((DeleteButton)o).Parent.Hide();
        }
    }
}

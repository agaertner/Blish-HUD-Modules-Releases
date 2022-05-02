using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Nekres.Chat_Shorts.UI.Controls;
using Nekres.Chat_Shorts.UI.Models;
using Nekres.Chat_Shorts.UI.Presenters;

namespace Nekres.Chat_Shorts.UI.Views
{
    internal class MacroView : View<MacroPresenter>
    {
        public event EventHandler<MouseEventArgs> EditClick;

        private Label _keys;
        private Label _title;

        public MacroView(MacroModel model)
        {
            this.WithPresenter(new MacroPresenter(this, model));
            this.Presenter.Model.Changed += OnModelChanged;
        }

        private void OnModelChanged(object o, EventArgs e)
        {
            _keys.Text = this.Presenter.Model.KeyBinding.GetBindingDisplayText();
            _title.Text = this.Presenter.Model.Title;
        }

        protected override void Build(Container buildPanel)
        {
            _keys = new Label
            {
                Parent = buildPanel,
                Size = new Point(100, 20),
                Location = new Point(Panel.LEFT_PADDING,0),
                VerticalAlignment = VerticalAlignment.Top,
                Font = GameService.Content.DefaultFont16,
                Text = this.Presenter.Model.KeyBinding.GetBindingDisplayText(),
                TextColor = Color.White
            };

            _title = new Label
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Width, buildPanel.ContentRegion.Height - _keys.Height),
                Location = new Point(Panel.LEFT_PADDING, _keys.Height),
                Text = this.Presenter.Model.Title,
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size20, ContentService.FontStyle.Regular),
                TextColor = Color.White,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeText = true,
                ShowShadow = true
            };

            var btnEdit = new EditButton(ChatShorts.Instance.ContentsManager)
            {
                Parent = buildPanel,
                Size = new Point(64, 64),
                Location = new Point(buildPanel.ContentRegion.Width - 64, 0)
            };
            btnEdit.Click += OnEditClick;
        }

        private void OnEditClick(object o, MouseEventArgs e) => EditClick?.Invoke(this, e);
    }
}

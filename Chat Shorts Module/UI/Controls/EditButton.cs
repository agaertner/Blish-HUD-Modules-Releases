using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Chat_Shorts.UI.Controls
{
    internal class EditButton : Image
    {
        private static Texture2D _editMacroTex;
        private static Texture2D _editMacroTexHover;
        private static Texture2D _editMacroTexActive;
        private static Texture2D _editMacroTexDisabled;

        public new bool Enabled
        {
            get => _enabled;
            set
            {
                SetProperty(ref _enabled, value);
                this.Texture = value ? _editMacroTex : _editMacroTexDisabled;
            }
        }

        public bool _active;
        public bool Active
        {
            get => _active;
            set
            {
                SetProperty(ref _active, value);
                this.Texture = value ? _editMacroTex : _editMacroTexActive;
            }
        }

        public EditButton(ContentsManager content)
        {
            _editMacroTex ??= content.GetTexture("155941.png");
            _editMacroTexActive ??= content.GetTexture("155942.png");
            _editMacroTexHover ??= content.GetTexture("155940.png");
            _editMacroTexDisabled ??= content.GetTexture("155939.png");
            this.Texture = this.Enabled ? _editMacroTex : _editMacroTexDisabled;
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            this.Texture = _editMacroTexHover;
            base.OnMouseMoved(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            this.Texture = _editMacroTex;
            base.OnMouseLeft(e);
        }
    }
}

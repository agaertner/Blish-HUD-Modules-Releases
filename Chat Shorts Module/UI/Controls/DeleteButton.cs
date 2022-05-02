using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Chat_Shorts.UI.Controls
{
    internal class DeleteButton : Image
    {
        private static Texture2D _deleteIcon;
        private static Texture2D _deleteIconHover;

        public DeleteButton(ContentsManager content)
        {
            _deleteIcon ??= content.GetTexture("trashcanClosed_icon_64x64.png");
            _deleteIconHover ??= content.GetTexture("trashcanOpen_icon_64x64.png");
            this.Texture = _deleteIcon;
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            this.Texture = _deleteIconHover;
            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            this.Texture = _deleteIcon;
            base.OnMouseLeft(e);
        }
    }
}

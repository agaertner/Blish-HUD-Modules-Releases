using System;
using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Notes.UI.Controls
{
    internal class ReadOnlyBook : BookBase
    {
        public ReadOnlyBook(Guid id, string title, IList<(string, string)> pages) : base(id, title, pages)
        {
        }

        public ReadOnlyBook(Guid id, string title, IEnumerable<string> contentPages) : base(id, title, contentPages)
        {
        }

        public ReadOnlyBook(Guid id, string title, string content) : base(id, title, content)
        {
        }

        public ReadOnlyBook(Guid id, string title) : base(id, title)
        {
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, this.Pages[this.CurrentPageIndex].Item1, TextFont, this.TitleRegion, Color.Black, false, HorizontalAlignment.Center);

            spriteBatch.DrawStringOnCtrl(this, this.Pages[this.CurrentPageIndex].Item2, TextFont, this.SheetContentRegion, Color.Black);
        }
    }
}

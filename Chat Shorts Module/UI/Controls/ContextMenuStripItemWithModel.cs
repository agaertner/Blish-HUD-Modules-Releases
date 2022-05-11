using Blish_HUD.Controls;
using System;

namespace Nekres.Chat_Shorts.UI.Controls
{
    internal class ContextMenuStripItemWithModel<T> : ContextMenuStripItem
    {
        public T Model { get; }

        public ContextMenuStripItemWithModel(T model)
        {
            this.Model = model;
        }
    }
}

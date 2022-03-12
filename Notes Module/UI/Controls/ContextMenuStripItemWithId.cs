using Blish_HUD.Controls;
using System;

namespace Nekres.Notes.UI.Controls
{
    internal class ContextMenuStripItemWithId : ContextMenuStripItem
    {
        public Guid Id { get; private set; }

        public ContextMenuStripItemWithId(Guid id)
        {
            this.Id = id;
        }
    }
}

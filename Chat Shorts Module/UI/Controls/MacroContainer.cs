using Blish_HUD.Controls;
using Nekres.Chat_Shorts.UI.Models;

namespace Nekres.Chat_Shorts.UI.Controls
{
    internal class MacroContainer : ViewContainer
    {
        public MacroModel MacroModel { get; private set; }

        public MacroContainer(MacroModel model)
        {
            this.MacroModel = model;
        }
    }
}

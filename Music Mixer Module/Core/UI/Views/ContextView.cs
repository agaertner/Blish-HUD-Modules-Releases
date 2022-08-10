using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;

namespace Nekres.Music_Mixer.Core.UI.Views
{
    internal class ContextView : View
    {
        public ViewContainer Library { get; private set; }

        private MainModel _model;

        public ContextView(MainModel model)
        {
            _model = model;
        }

        protected override void Build(Container buildPanel)
        {
            var dayCycle = new DayCycleSwitcher
            {
                Parent = buildPanel,
                Width = buildPanel.ContentRegion.Width - Panel.RIGHT_PADDING,
                Height = 100,
                Location = new Point(buildPanel.ContentRegion.X, 0),
                DayCycle = _model.DayCycle
            };
            dayCycle.Click += (_, _) =>
            {
                GameService.Content.PlaySoundEffectByName($"tab-swap-{RandomUtil.GetRandom(1, 5)}");
                _model.DayCycle = dayCycle.NextCycle();
            };

            this.Library = new ViewContainer
            {
                Parent = buildPanel,
                Width = buildPanel.ContentRegion.Width - Panel.RIGHT_PADDING,
                Height = buildPanel.ContentRegion.Height - dayCycle.Height - Panel.BOTTOM_PADDING,
                Location = new Point(buildPanel.ContentRegion.X, dayCycle.Bottom + Panel.BOTTOM_PADDING)
            };
            this.Library.Show(new MapSelectionView(_model));

            base.Build(buildPanel);
        }
    }
}

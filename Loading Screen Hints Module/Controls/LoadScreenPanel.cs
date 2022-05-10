using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Loading_Screen_Hints.Controls.Hints;

namespace Nekres.Loading_Screen_Hints.Controls
{
    public class LoadScreenPanel : Container {

        public const int TOP_PADDING = 20;
        public const int RIGHT_PADDING = 40;

        private static readonly Texture2D _textureBackgroundLoadScreenPanel;

        static LoadScreenPanel() {
            _textureBackgroundLoadScreenPanel = LoadingScreenHintsModule.Instance.ContentsManager.GetTexture("background_loadscreenpanel.png");
        }

        public Glide.Tween Fade { get; private set; }

        public Control LoadScreenTip;

        public LoadScreenPanel() {
            UpdateLocation(null, null);

            Graphics.SpriteScreen.Resized += UpdateLocation;
            Disposed += OnDisposed;
        }

        public void FadeOut() {

            if (Opacity != 1.0f) return;

            float duration = 2.0f;

            if (LoadScreenTip != null) {
                if (LoadScreenTip is GuessCharacter tip) {
                    tip.Result = true;
                    duration = duration + 3.0f;

                } else if (LoadScreenTip is Narration narration) {
                    duration = duration + narration.ReadingTime;

                } else if (LoadScreenTip is GamingTip selected) {
                    duration = duration + selected.ReadingTime;
                }
            }

            Fade = Animation.Tweener.Tween(this, new { Opacity = 0.0f }, duration);
            Fade.OnComplete(this.Dispose);
        }
        protected override void OnLeftMouseButtonReleased(MouseEventArgs e) {
            if (Opacity != 1.0) return;
            GameService.Animation.Tweener.Tween(this, new { Opacity = 0.0f }, 0.2f);
        }

        protected override void OnRightMouseButtonReleased(MouseEventArgs e)
        {
            if (Opacity != 0.0f) return;
            GameService.Animation.Tweener.Tween(this, new { Opacity = 1.0f }, 0.2f);
            base.OnRightMouseButtonReleased(e);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            if (LoadScreenTip != null)
            {
                if (LoadScreenTip is GuessCharacter)
                {
                    GuessCharacter selected = (GuessCharacter)LoadScreenTip;
                    selected.CharacterImage.Dispose();
                }
                LoadScreenTip.Dispose();
            }
        }

        private void UpdateLocation(object sender, EventArgs e) {
            this.Location = new Point((Graphics.SpriteScreen.Width / 2 - this.Width / 2), (Graphics.SpriteScreen.Height  / 2 - this.Height / 2) + 300);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, _textureBackgroundLoadScreenPanel, bounds);
        }

    }

}

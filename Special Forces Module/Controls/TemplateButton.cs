using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Special_Forces.Core.Services.Persistance;

namespace Nekres.Special_Forces.Controls
{
    internal class TemplateButton : DetailsButton
    {
        public event EventHandler<EventArgs> PlayClick;

        private const int SHEETBUTTON_WIDTH = 327;
        private const int SHEETBUTTON_HEIGHT = 100;

        private const int USER_WIDTH = 75;
        private const int BOTTOMSECTION_HEIGHT = 35;
        private static Texture2D BackgroundSprite = ContentService.Textures.Pixel;
        private static Texture2D ClipboardSprite = SpecialForcesModule.Instance.ContentsManager.GetTexture("clipboard.png");
        private static Texture2D DividerSprite = GameService.Content.GetTexture("157218");
        private static Texture2D GlowClipboardSprite = SpecialForcesModule.Instance.ContentsManager.GetTexture("glow_clipboard.png");
        private static Texture2D GlowPlaySprite = SpecialForcesModule.Instance.ContentsManager.GetTexture("glow_play.png");
        private static Texture2D GlowUtilitySprite = SpecialForcesModule.Instance.ContentsManager.GetTexture("skill_frame.png");
        private static Texture2D IconBoxSprite = GameService.Content.GetTexture("controls/detailsbutton/605003");
        private static Texture2D PlaySprite = SpecialForcesModule.Instance.ContentsManager.GetTexture("play.png");
        private static Texture2D UtilitySprite = SpecialForcesModule.Instance.ContentsManager.GetTexture("skill_frame.png");

        private bool _mouseOverPlay;
        private Rectangle _playBounds;

        private bool _mouseOverTemplate;
        private Rectangle _templateBounds;

        private bool _mouseOverUtility1;
        private Rectangle _utility1Bounds;

        private bool _mouseOverUtility2;
        private Rectangle _utility2Bounds;

        private bool _mouseOverUtility3;
        private Rectangle _utility3Bounds;

        private RawTemplate _template;

        internal TemplateButton(RawTemplate template)
        {
            if (template == null) return;
            Template = template;
            if (Template.Utilitykeys == null) Template.Utilitykeys = new int[3] {1, 2, 3};

            Size = new Point(SHEETBUTTON_WIDTH, SHEETBUTTON_HEIGHT);
        }

        internal string BottomText { get; set; }

        internal RawTemplate Template
        {
            get => _template;
            set
            {
                if (_template == value) return;

                _template = value;
                OnPropertyChanged();
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (_mouseOverTemplate)
            {
                ScreenNotification.ShowNotification("Not yet implemented!");
            }
            else if (_mouseOverPlay)
            {
                this.PlayClick?.Invoke(this, EventArgs.Empty);
                SpecialForcesModule.Instance.Window.Hide();
            } 
            else if (_mouseOverUtility1 || _mouseOverUtility2 || _mouseOverUtility3)
            {
                this.UpdateUtilityKeys();
            }
            base.OnClick(e);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = RelativeMousePosition;
            _mouseOverPlay = _playBounds.Contains(relPos);
            _mouseOverTemplate = _templateBounds.Contains(relPos);
            _mouseOverUtility3 = _utility3Bounds.Contains(relPos);
            _mouseOverUtility2 = _utility2Bounds.Contains(relPos);
            _mouseOverUtility1 = _utility1Bounds.Contains(relPos);

            if (_mouseOverPlay)
                BasicTooltipText = "Practice!";
            else if (_mouseOverTemplate)
                BasicTooltipText = "Copy Template";
            else if (_mouseOverUtility1)
                BasicTooltipText = "Assign Utility Key 1";
            else if (_mouseOverUtility2)
                BasicTooltipText = "Assign Utility Key 2";
            else if (_mouseOverUtility3)
                BasicTooltipText = "Assign Utility Key 3";
            else
                BasicTooltipText = Title;
            base.OnMouseMoved(e);
        }

        private void UpdateUtilityKeys()
        {
            var index = _mouseOverUtility1 ? 0 : _mouseOverUtility2 ? 1 : 2;
            var swap = this.Template.Utilitykeys[index] == 3 ? 1 : this.Template.Utilitykeys[index] + 1;
            
            if (Array.Exists(this.Template.Utilitykeys, e => e == swap))
            {
                this.Template.Utilitykeys[Array.FindIndex(this.Template.Utilitykeys, e => e == swap)] = this.Template.Utilitykeys[index];
            }

            this.Template.Utilitykeys[index] = swap;
            this.Template.Save();
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse | CaptureType.Filter;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var iconSize = IconSize == DetailsIconSize.Large
                ? SHEETBUTTON_HEIGHT
                : SHEETBUTTON_HEIGHT - BOTTOMSECTION_HEIGHT;

            // Draw background
            spriteBatch.DrawOnCtrl(this, BackgroundSprite, bounds, Color.Black * 0.25f);

            // Draw bottom section (overlap to make background darker here)
            spriteBatch.DrawOnCtrl(this, BackgroundSprite,
                new Rectangle(0, bounds.Height - BOTTOMSECTION_HEIGHT, bounds.Width - BOTTOMSECTION_HEIGHT,
                    BOTTOMSECTION_HEIGHT), Color.Black * 0.1f);

            // Draw icons
            #region Icons
            _playBounds = new Rectangle(SHEETBUTTON_WIDTH - 36, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32);
            spriteBatch.DrawOnCtrl(this, _mouseOverPlay ? GlowPlaySprite : PlaySprite, _playBounds, Color.White);

            _templateBounds = new Rectangle(SHEETBUTTON_WIDTH - 73, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32);
            spriteBatch.DrawOnCtrl(this, _mouseOverTemplate ? GlowClipboardSprite : ClipboardSprite, _templateBounds, Color.White);

            _utility3Bounds = new Rectangle(SHEETBUTTON_WIDTH - 109, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32);
            spriteBatch.DrawOnCtrl(this, _mouseOverUtility3 ? GlowUtilitySprite : UtilitySprite, _utility3Bounds, Color.White);

            spriteBatch.DrawStringOnCtrl(this, Template.Utilitykeys[2] + "", Content.DefaultFont14, _utility3Bounds, Color.White, 
                false, true, 2, HorizontalAlignment.Center, VerticalAlignment.Bottom);

            _utility2Bounds = new Rectangle(SHEETBUTTON_WIDTH - 145, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32);
            spriteBatch.DrawOnCtrl(this, _mouseOverUtility2 ? GlowUtilitySprite : UtilitySprite, _utility2Bounds, Color.White);

            spriteBatch.DrawStringOnCtrl(this, Template.Utilitykeys[1] + "", Content.DefaultFont14, _utility2Bounds, Color.White,
                false, true, 2, Blish_HUD.Controls.HorizontalAlignment.Center, VerticalAlignment.Bottom);

            _utility1Bounds = new Rectangle(SHEETBUTTON_WIDTH - 181, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32);
            spriteBatch.DrawOnCtrl(this, _mouseOverUtility1 ? GlowUtilitySprite : UtilitySprite, _utility1Bounds, Color.White);

            spriteBatch.DrawStringOnCtrl(this, Template.Utilitykeys[0] + "", Content.DefaultFont14, _utility1Bounds, Color.White,
                false, true, 2, Blish_HUD.Controls.HorizontalAlignment.Center, VerticalAlignment.Bottom);

            #endregion

            // Draw bottom section seperator
            spriteBatch.DrawOnCtrl(this, DividerSprite, new Rectangle(0, bounds.Height - 40, bounds.Width, 8),
                Color.White);

            // Draw icon
            if (Icon != null)
            {
                spriteBatch.DrawOnCtrl(this, Icon,
                    new Rectangle((bounds.Height - BOTTOMSECTION_HEIGHT) / 2 - 32, (bounds.Height - 35) / 2 - 32, 64,
                        64), Color.White);
                // Draw icon box
                spriteBatch.DrawOnCtrl(this, IconBoxSprite, new Rectangle(0, 0, iconSize, iconSize), Color.White);
            }

            // Wrap text
            var text = Text;
            var wrappedText =
                DrawUtil.WrapText(Content.DefaultFont14, text, SHEETBUTTON_WIDTH - 40 - iconSize - 20);
            spriteBatch.DrawStringOnCtrl(this, wrappedText, Content.DefaultFont14,
                new Rectangle(89, 0, 216, Height - BOTTOMSECTION_HEIGHT), Color.White, false, true, 2);

            // Draw the profession;
            spriteBatch.DrawStringOnCtrl(this, BottomText, Content.DefaultFont14,
                new Rectangle(5, bounds.Height - BOTTOMSECTION_HEIGHT, USER_WIDTH, 35), Color.White, false, false, 0);
        }
    }
}
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    public class TrackBar2 : Control
    {

        private const int BUMPER_WIDTH = 4;

        private readonly List<float> tenIncrements = new List<float>();

        #region Load Static

        private static readonly Texture2D _textureTrack = Content.GetTexture("controls/trackbar/154968");
        private static readonly TextureRegion2D _textureNub = Blish_HUD.Controls.Resources.Control.TextureAtlasControl.GetRegion("trackbar/tb-nub");

        #endregion

        public event EventHandler<ValueEventArgs<float>> ValueChanged;
        public event EventHandler<ValueEventArgs<float>> DraggingStopped;

        protected float _maxValue = 100f;

        /// <summary>
        /// The largest value the <see cref="TrackBar"/> will show.
        /// </summary>
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                if (SetProperty(ref _maxValue, value, true))
                {
                    this.Value = _value;
                }
                MinMaxChanged();
            }
        }

        protected float _minValue = 0f;

        /// <summary>
        /// The smallest value the <see cref="TrackBar"/> will show.
        /// </summary>
        public float MinValue
        {
            get => _minValue;
            set
            {
                if (SetProperty(ref _minValue, value, true))
                {
                    this.Value = _value;
                }
                MinMaxChanged();
            }
        }

        protected float _value = 50;
        public float Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, MathHelper.Clamp(value, _minValue, _maxValue), true))
                {
                    this.ValueChanged?.Invoke(this, new ValueEventArgs<float>(_value));
                }
            }
        }

        protected bool _smallStep = false;

        /// <summary>
        /// If <c>true</c>, values can change in increments less than 1.
        /// If <c>false</c>, values will snap to full integers.
        /// </summary>
        public bool SmallStep
        {
            get => _smallStep;
            set => SetProperty(ref _smallStep, value);
        }

        private float _dragStartValue;
        private bool _dragging = false;
        public bool Dragging => _dragging;

        private int _dragOffset = 0;

        public TrackBar2()
        {
            this.Size = new Point(256, 16);

            Input.Mouse.LeftMouseButtonReleased += InputOnLeftMouseButtonReleased;
        }

        private void InputOnLeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            if (_dragging && Math.Abs(_dragStartValue - this.Value) > 0.01f)
            {
                this.DraggingStopped?.Invoke(this, new ValueEventArgs<float>(this.Value));
            }
            _dragging = false;
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);

            if (_layoutNubBounds.Contains(this.RelativeMousePosition) && !_dragging)
            {
                _dragStartValue = this.Value;
                _dragging = true;
                _dragOffset = this.RelativeMousePosition.X - _layoutNubBounds.X - BUMPER_WIDTH / 2;
            }
        }

        public override void DoUpdate(GameTime gameTime)
        {
            if (_dragging)
            {
                float rawValue = (this.RelativeMousePosition.X - BUMPER_WIDTH - _dragOffset) / (float)(this.Width - BUMPER_WIDTH - _textureNub.Width) * (this.MaxValue - this.MinValue) + this.MinValue;

                this.Value = GameService.Input.Keyboard.ActiveModifiers != ModifierKeys.Ctrl
                                 ? SmallStep ? rawValue : (float)Math.Round(rawValue, 0)
                                 : tenIncrements.Aggregate((x, y) => Math.Abs(x - rawValue) < Math.Abs(y - rawValue) ? x : y);
            }
        }

        private void MinMaxChanged()
        {
            tenIncrements.Clear();
            for (int i = 0; i < 11; i++)
            {
                tenIncrements.Add((this.MaxValue - this.MinValue) * 0.1f * i + this.MinValue);
            }
        }

        private Rectangle _layoutNubBounds;
        private Rectangle _layoutLeftBumper;
        private Rectangle _layoutRightBumper;

        public override void RecalculateLayout()
        {
            _layoutLeftBumper = new Rectangle(0, 0, BUMPER_WIDTH, this.Height);
            _layoutRightBumper = new Rectangle(this.Width - BUMPER_WIDTH, 0, BUMPER_WIDTH, this.Height);

            float valueOffset = (this.Value - this.MinValue) / (this.MaxValue - this.MinValue) * (_size.X - BUMPER_WIDTH - _textureNub.Width);
            _layoutNubBounds = new Rectangle((int)valueOffset + BUMPER_WIDTH / 2, 0, _textureNub.Width, _textureNub.Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, _textureTrack, bounds);

            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _layoutLeftBumper);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _layoutRightBumper);

            spriteBatch.DrawOnCtrl(this, _textureNub, _layoutNubBounds);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonReleased -= InputOnLeftMouseButtonReleased;
        }

    }
}

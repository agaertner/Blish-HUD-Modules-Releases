using System;
using System.Globalization;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Extended;
using Blish_HUD.Input;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using static Blish_HUD.GameService;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Nekres.Mumble_Info.Core.Controls
{
    internal class DataPanel : Container
    {
        private float         _memoryUsage          => MumbleInfoModule.Instance.MemoryUsage;
        private float          _cpuUsage            => MumbleInfoModule.Instance.CpuUsage;
        private string         _cpuName             => MumbleInfoModule.Instance.CpuName;
        private Map            _currentMap          => MumbleInfoModule.Instance.CurrentMap;
        private Specialization _currentSpec         => MumbleInfoModule.Instance.CurrentSpec;

        #region Colors

        private readonly Color _grey        = new Color(168, 168, 168);
        private readonly Color _orange      = new Color(252, 168, 0);
        private readonly Color _red         = new Color(252, 84, 84);
        private readonly Color _softRed     = new Color(250, 148, 148);
        private readonly Color _lemonGreen  = new Color(84, 252, 84);
        private readonly Color _cyan        = new Color(84, 252, 252);
        private readonly Color _blue        = new Color(0, 168, 252);
        private readonly Color _green       = new Color(0, 168, 0);
        private readonly Color _brown       = new Color(158, 81, 44);
        private readonly Color _yellow      = new Color(252, 252, 84);
        private readonly Color _softYellow  = new Color(250, 250, 148);
        private readonly Color _borderColor = Color.AntiqueWhite;
        private readonly Color _clickColor  = Color.AliceBlue;

        #endregion

        private readonly BitmapFont _font;
        private const int           _leftMargin          = 10;
        private const int           _rightMargin         = 10;
        private const int           _topMargin           = 5;
        private const int           _borderSize          = 1;
        private const string        _clipboardMessage    = "Copied!";
        private const string        _decimalFormat       = "0.###";

        private bool _isMousePressed;

        #region Info Elements

        private Rectangle _avatarPositionBounds;
        private bool _mouseOverAvatarPosition;

        private Rectangle _avatarFacingBounds;
        private bool _mouseOverAvatarFacing;

        private Rectangle _mapCoordinatesBounds;
        private bool _mouseOverMapCoordinates;

        private Rectangle _cameraDirectionBounds;
        private bool _mouseOverCameraDirection;

        private Rectangle _cameraPositionBounds;
        private bool _mouseOverCameraPosition;

        private Rectangle _mapHashBounds;
        private bool _mouseOverMapHashBounds;

        #endregion

        public DataPanel() {
            _font = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size36, ContentService.FontStyle.Regular);

            UpdateLocation(null, null);
            Graphics.SpriteScreen.Resized += UpdateLocation;
        }

        protected override void DisposeControl()
        {
            Graphics.SpriteScreen.Resized -= UpdateLocation;
            base.DisposeControl();
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = RelativeMousePosition;
            _mouseOverAvatarFacing = _avatarFacingBounds.Contains(relPos);
            _mouseOverAvatarPosition = _avatarPositionBounds.Contains(relPos);
            _mouseOverMapCoordinates = _mapCoordinatesBounds.Contains(relPos);
            _mouseOverCameraDirection = _cameraDirectionBounds.Contains(relPos);
            _mouseOverCameraPosition = _cameraPositionBounds.Contains(relPos);
            _mouseOverMapHashBounds = _mapHashBounds.Contains(relPos);
            base.OnMouseMoved(e);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            _isMousePressed = false;
            if (_mouseOverAvatarFacing)
            {
                ClipboardUtil.WindowsClipboardService.SetTextAsync(Gw2Mumble.PlayerCharacter.Forward.ToString());
                ScreenNotification.ShowNotification(_clipboardMessage);
            }
            else if (_mouseOverAvatarPosition)
            {
                ClipboardUtil.WindowsClipboardService.SetTextAsync(
                    string.Format("xpos=\"{0}\" ypos=\"{1}\" zpos=\"{2}\"",
                        Gw2Mumble.PlayerCharacter.Position.X.ToString(CultureInfo.InvariantCulture),
                        (MumbleInfoModule.Instance.SwapYZAxes.Value ? Gw2Mumble.PlayerCharacter.Position.Z : Gw2Mumble.PlayerCharacter.Position.Y).ToString(CultureInfo.InvariantCulture),
                        (MumbleInfoModule.Instance.SwapYZAxes.Value ? Gw2Mumble.PlayerCharacter.Position.Y : Gw2Mumble.PlayerCharacter.Position.Z).ToString(CultureInfo.InvariantCulture)));
                
                ScreenNotification.ShowNotification(_clipboardMessage);
            }
            else if (_mouseOverMapCoordinates)
            {
                ClipboardUtil.WindowsClipboardService.SetTextAsync(
                    string.Format("xpos=\"{0}\" ypos=\"{1}\"",
                        Gw2Mumble.RawClient.PlayerLocationMap.X.ToString(CultureInfo.InvariantCulture),
                        Gw2Mumble.RawClient.PlayerLocationMap.Y.ToString(CultureInfo.InvariantCulture)));

                ScreenNotification.ShowNotification(_clipboardMessage);
            }
            else if (_mouseOverCameraDirection)
            {
                ClipboardUtil.WindowsClipboardService.SetTextAsync(Gw2Mumble.PlayerCamera.Forward.ToString());
                ScreenNotification.ShowNotification(_clipboardMessage);
            }
            else if (_mouseOverCameraPosition)
            {
                ClipboardUtil.WindowsClipboardService.SetTextAsync(
                    string.Format("xpos=\"{0}\" ypos=\"{1}\" zpos=\"{2}\"",
                        Gw2Mumble.PlayerCamera.Position.X.ToString(CultureInfo.InvariantCulture),
                        (MumbleInfoModule.Instance.SwapYZAxes.Value ? Gw2Mumble.PlayerCamera.Position.Z : Gw2Mumble.PlayerCamera.Position.Y).ToString(CultureInfo.InvariantCulture),
                        (MumbleInfoModule.Instance.SwapYZAxes.Value ? Gw2Mumble.PlayerCamera.Position.Y : Gw2Mumble.PlayerCamera.Position.Z).ToString(CultureInfo.InvariantCulture)));

                ScreenNotification.ShowNotification(_clipboardMessage);
            }
            else if (_mouseOverMapHashBounds)
            {
                ClipboardUtil.WindowsClipboardService.SetTextAsync($"\"{_currentMap.GetHash()}\": {_currentMap.Id}, // {_currentMap.Name} ({_currentMap.Id})");
                ScreenNotification.ShowNotification(_clipboardMessage);
            } 
            base.OnLeftMouseButtonReleased(e);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            _isMousePressed = true;
            base.OnLeftMouseButtonPressed(e);
        }

        protected override CaptureType CapturesInput() => CaptureType.DoNotBlock;

        private void UpdateLocation(object sender, EventArgs e) => Location = new Point(0, 0);

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if (!GameIntegration.Gw2Instance.Gw2IsRunning || !Gw2Mumble.IsAvailable || !GameIntegration.Gw2Instance.IsInGame) return;

            var calcTopMargin = _topMargin;
            var calcLeftMargin = _leftMargin;

            #region Game
            
            var text = $"{Gw2Mumble.RawClient.Name}  ";
            var width = (int)_font.MeasureString(text).Width;
            var height = (int)_font.MeasureString(text).Height;
            var rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _brown, false, true);

            calcLeftMargin += width;

            text = $"({Gw2Mumble.Info.BuildId})/";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _green, false, true);

            calcLeftMargin += width;

            text = $"(Mumble Link v{Gw2Mumble.Info.Version})";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _green, false, true);

            #endregion

            calcTopMargin += height;
            calcLeftMargin = _leftMargin;

            #region Server

            text = $"{Gw2Mumble.Info.ServerAddress}";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _grey, false, true);

            calcLeftMargin += width;

            text = ":";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

            calcLeftMargin += width;

            text = $"{Gw2Mumble.Info.ServerPort}  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _grey, false, true);

            calcLeftMargin += width;

            text = $"- {Gw2Mumble.Info.ShardId}  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _grey, false, true);

            calcLeftMargin += width;

            text = $"({Gw2Mumble.RawClient.Instance})";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _grey, false, true);

            #endregion

            calcTopMargin += height * 2;
            calcLeftMargin = _leftMargin;

            #region Avatar

            text = "Avatar";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _red, false, true);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            text = $"{Gw2Mumble.PlayerCharacter.Name} - {Gw2Mumble.PlayerCharacter.Race}";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _softRed, false, true);

            calcLeftMargin += width;

            text = $" ({Gw2Mumble.PlayerCharacter.TeamColorId})";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _softYellow, false, true);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            text = "Profession";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _red, false, true);
            
            calcLeftMargin += width;

            text = ":  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);
            
            calcLeftMargin += width;

            text = $"{Gw2Mumble.PlayerCharacter.Profession}";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _yellow, false, true);

            if (_currentSpec is {Elite: true} && _currentSpec.Id == Gw2Mumble.PlayerCharacter.Specialization) {
                
                calcTopMargin += height;
                calcLeftMargin = _leftMargin * 3;

                text = "Elite";
                width = (int)_font.MeasureString(text).Width;
                height = (int)_font.MeasureString(text).Height;
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _red, false, true);
                
                calcLeftMargin += width;

                text = ":  ";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);
                
                calcLeftMargin += width;

                text = $"{_currentSpec.Name}";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _yellow, false, true);
                
                calcLeftMargin += width;

                text = $" ({_currentSpec.Id})";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _softYellow, false, true);
            }

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            var playerPos = Gw2Mumble.PlayerCharacter.Position;

            text = "X";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _red, false, true);

            var infoBounds = rect;
            calcLeftMargin += width;

            text = "Y";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _lemonGreen, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = "Z";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = ":  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = playerPos.X.ToString(_decimalFormat);
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _red, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = $"  {(MumbleInfoModule.Instance.SwapYZAxes.Value ? playerPos.Z : playerPos.Y).ToString(_decimalFormat)}";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _lemonGreen, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = $"  {(MumbleInfoModule.Instance.SwapYZAxes.Value ? playerPos.Y : playerPos.Z).ToString(_decimalFormat)}";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out _avatarPositionBounds);

            if (_mouseOverAvatarPosition) DrawBorder(spriteBatch, _avatarPositionBounds);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            var playerFacing = Gw2Mumble.RawClient.AvatarFront;

            text = "Facing";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _red, false, true);

            infoBounds = rect;
            calcLeftMargin += width;

            text = ":  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);
            
            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = playerFacing.X.ToString(_decimalFormat);
            width = (int) _font.MeasureString(text).Width;
            height = Math.Max(height, (int) _font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _red, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = $"  {(MumbleInfoModule.Instance.SwapYZAxes.Value ? playerFacing.Z : playerFacing.Y).ToString(_decimalFormat)}";
            width = (int) _font.MeasureString(text).Width;
            height = Math.Max(height, (int) _font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _lemonGreen, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = $"  {(MumbleInfoModule.Instance.SwapYZAxes.Value ? playerFacing.Y : playerFacing.Z).ToString(_decimalFormat)}";
            width = (int) _font.MeasureString(text).Width;
            height = Math.Max(height, (int) _font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out _avatarFacingBounds);
            if (_mouseOverAvatarFacing) DrawBorder(spriteBatch, _avatarFacingBounds);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            text = DirectionUtil.IsFacing(playerFacing.SwapYZ()).ToString().SplitAtUpperCase().Trim();
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _softYellow, false, true);

            #endregion

            calcTopMargin += height * 2;
            calcLeftMargin = _leftMargin;

            #region Map

            text = "Map";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _blue, false, true);

            if (_currentMap != null && _currentMap.Id == Gw2Mumble.CurrentMap.Id) {
                calcTopMargin += height;
                calcLeftMargin = _leftMargin * 3;

                text = $"{_currentMap.Name}";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

                calcLeftMargin += width;

                text = $" ({Gw2Mumble.CurrentMap.Id})";
                width = (int)_font.MeasureString(text).Width;
                height = (int)_font.MeasureString(text).Height;
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _yellow, false, true);

                calcTopMargin += height;
                calcLeftMargin = _leftMargin * 3;

                text = "Region";
                width = (int)_font.MeasureString(text).Width;
                height = (int)_font.MeasureString(text).Height;
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _blue, false, true);
                
                calcLeftMargin += width;

                text = ":  ";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);
                
                calcLeftMargin += width;

                text = $"{_currentMap.RegionName}";
                width = (int)_font.MeasureString(text).Width;
                height = (int)_font.MeasureString(text).Height;
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

                calcTopMargin += height;
                calcLeftMargin = _leftMargin * 3;

                text = "Continent";
                width = (int)_font.MeasureString(text).Width;
                height = (int)_font.MeasureString(text).Height;
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _blue, false, true);
                
                calcLeftMargin += width;

                text = ":  ";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

                calcLeftMargin += width;

                text = $"{_currentMap.ContinentName}";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

                calcTopMargin += height;
                calcLeftMargin = _leftMargin * 3;

                text = "Hash";
                width = (int)_font.MeasureString(text).Width;
                height = (int)_font.MeasureString(text).Height;
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _blue, false, true);

                infoBounds = rect;
                calcLeftMargin += width;

                text = ":  ";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

                RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
                calcLeftMargin += width;

                text = $"{_currentMap.GetHash()}";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

                RectangleExtensions.Union(ref rect, ref infoBounds, out _mapHashBounds);
                if (_mouseOverMapHashBounds) DrawBorder(spriteBatch, _mapHashBounds);
            }

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            text = "Type";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _blue, false, true);

            calcLeftMargin += width;

            text = ":  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

            calcLeftMargin += width;

            text = $"{Gw2Mumble.CurrentMap.Type} ({(Gw2Mumble.CurrentMap.IsCompetitiveMode ? "PvP" : "PvE")})";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            var playerLocationMap = Gw2Mumble.RawClient.PlayerLocationMap;

            text = "X";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _red, false, true);

            infoBounds = rect;
            calcLeftMargin += width;

            text = "Y";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _lemonGreen, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = ":  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = playerLocationMap.X.ToString(_decimalFormat);
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _red, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = $"  {playerLocationMap.Y.ToString(_decimalFormat)}";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _lemonGreen, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out _mapCoordinatesBounds);
            if (_mouseOverMapCoordinates) DrawBorder(spriteBatch, _mapCoordinatesBounds);

            #endregion

            calcTopMargin += height * 2;
            calcLeftMargin = _leftMargin;

            #region Camera

            text = "Camera";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _green, false, true);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            var cameraForward = Gw2Mumble.PlayerCamera.Forward;

            text = "Direction";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _green, false, true);

            infoBounds = rect;
            calcLeftMargin += width;

            text = ":  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);
            
            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = cameraForward.X.ToString(_decimalFormat);
            width = (int) _font.MeasureString(text).Width;
            height = Math.Max(height, (int) _font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _red, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = $"  {(MumbleInfoModule.Instance.SwapYZAxes.Value ? cameraForward.Z : cameraForward.Y).ToString(_decimalFormat)}";
            width = (int) _font.MeasureString(text).Width;
            height = Math.Max(height, (int) _font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _lemonGreen, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = $"  {(MumbleInfoModule.Instance.SwapYZAxes.Value ? cameraForward.Y : cameraForward.Z).ToString(_decimalFormat)}";
            width = (int) _font.MeasureString(text).Width;
            height = Math.Max(height, (int) _font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out _cameraDirectionBounds);
            if (_mouseOverCameraDirection) DrawBorder(spriteBatch, _cameraDirectionBounds);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            text = $"{DirectionUtil.IsFacing(new Coordinates3(cameraForward.X, cameraForward.Y, cameraForward.Z)).ToString().SplitAtUpperCase().Trim()}";
            width = (int) _font.MeasureString(text).Width;
            height = Math.Max(height, (int) _font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _lemonGreen, false, true);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            var cameraPosition = Gw2Mumble.PlayerCamera.Position;

            text = "Position";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _green, false, true);

            infoBounds = rect;
            calcLeftMargin += width;

            text = ":  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = cameraPosition.X.ToString(_decimalFormat);
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _red, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = $"  {(MumbleInfoModule.Instance.SwapYZAxes.Value ? cameraPosition.Z : cameraPosition.Y).ToString(_decimalFormat)}";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _lemonGreen, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out infoBounds);
            calcLeftMargin += width;

            text = $"  {(MumbleInfoModule.Instance.SwapYZAxes.Value ? cameraPosition.Y : cameraPosition.Z).ToString(_decimalFormat)}";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

            RectangleExtensions.Union(ref rect, ref infoBounds, out _cameraPositionBounds);
            if (_mouseOverCameraPosition) DrawBorder(spriteBatch, _cameraPositionBounds);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            text = "Field of View";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _green, false, true);

            calcLeftMargin += width;

            text = ":  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);
            
            calcLeftMargin += width;

            text = $"{Gw2Mumble.PlayerCamera.FieldOfView}";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _yellow, false, true);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            text = "Near Plane Render Distance";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _green, false, true);

            calcLeftMargin += width;

            text = ":  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

            calcLeftMargin += width;

            text = $"{Gw2Mumble.PlayerCamera.NearPlaneRenderDistance}";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _yellow, false, true);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            text = "Far Plane Render Distance";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _green, false, true);

            calcLeftMargin += width;

            text = ":  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

            calcLeftMargin += width;

            text = $"{Gw2Mumble.PlayerCamera.FarPlaneRenderDistance}";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _yellow, false, true);

            #endregion

            calcTopMargin += height * 2;
            calcLeftMargin = _leftMargin;

            #region User Interface

            text = "User Interface";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _orange, false, true);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            text = "Size";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _orange, false, true);

            calcLeftMargin += width;

            text = ":  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

            calcLeftMargin += width;

            text = $"{Gw2Mumble.UI.UISize}";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _yellow, false, true);

            calcTopMargin += height;
            calcLeftMargin = _leftMargin * 3;

            text = "Text Input Focused";
            width = (int)_font.MeasureString(text).Width;
            height = (int)_font.MeasureString(text).Height;
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _orange, false, true);

            calcLeftMargin += width;

            text = ":  ";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

            calcLeftMargin += width;

            text = $"{Gw2Mumble.UI.IsTextInputFocused}";
            width = (int)_font.MeasureString(text).Width;
            height = Math.Max(height, (int)_font.MeasureString(text).Height);
            rect = new Rectangle(calcLeftMargin, calcTopMargin, width, height);
            spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _yellow, false, true);

            #endregion

            calcTopMargin = _topMargin;
            var calcRightMargin = _rightMargin;

            #region Computer

            if (MumbleInfoModule.Instance.EnablePerformanceCounters.Value) {
                text = $"{_memoryUsage.ToString(_decimalFormat)} MB";
                width = (int)_font.MeasureString(text).Width;
                height = (int)_font.MeasureString(text).Height;
                rect = new Rectangle(Size.X - width - calcRightMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

                calcRightMargin += width;

                text = ":  ";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(Size.X - width - calcRightMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

                calcRightMargin += width;

                text = "Memory Usage";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(Size.X - width - calcRightMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _orange, false, true);

                calcTopMargin += height;
                calcRightMargin = _rightMargin;

                text = $"{Environment.ProcessorCount}x {_cpuName}";
                width = (int)_font.MeasureString(text).Width;
                height = (int)_font.MeasureString(text).Height;
                rect = new Rectangle(Size.X - width - calcRightMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

                calcRightMargin += width;

                text = ":  ";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(Size.X - width - calcRightMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);
                
                calcRightMargin += width;

                text = "CPU";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(Size.X - width - calcRightMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _orange, false, true);

                calcTopMargin += height;
                calcRightMargin = _rightMargin;

                text = $"{_cpuUsage.ToString(_decimalFormat)}%";
                width = (int)_font.MeasureString(text).Width;
                height = (int)_font.MeasureString(text).Height;
                rect = new Rectangle(Size.X - width - calcRightMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

                calcRightMargin += width;

                text = ":  ";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(Size.X - width - calcRightMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

                calcRightMargin += width;

                text = "CPU Usage";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(Size.X - width - calcRightMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _orange, false, true);

                calcTopMargin += height;
                calcRightMargin = _rightMargin;

                text = $"{Graphics.GraphicsDevice.Adapter.Description}";
                width = (int)_font.MeasureString(text).Width;
                height = (int)_font.MeasureString(text).Height;
                rect = new Rectangle(Size.X - width - calcRightMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _cyan, false, true);

                calcRightMargin += width;

                text = ":  ";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(Size.X - width - calcRightMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, Color.LightGray, false, true);

                calcRightMargin += width;

                text = "GPU";
                width = (int)_font.MeasureString(text).Width;
                height = Math.Max(height, (int)_font.MeasureString(text).Height);
                rect = new Rectangle(Size.X - width - calcRightMargin, calcTopMargin, width, height);
                spriteBatch.DrawStringOnCtrl(this, text, _font, rect, _orange, false, true);
            }

            #endregion
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width + _borderSize, _borderSize), _isMousePressed ? _clickColor : _borderColor);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.X, bounds.Y, _borderSize, bounds.Height + _borderSize), _isMousePressed ? _clickColor : _borderColor);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.X, bounds.Y + bounds.Height + _borderSize, bounds.Width + _borderSize, _borderSize), _isMousePressed ? _clickColor : _borderColor);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.X + bounds.Width + _borderSize, bounds.Y, _borderSize, bounds.Height + _borderSize), _isMousePressed ? _clickColor : _borderColor);
        }
    }
}

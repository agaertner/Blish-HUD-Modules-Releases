using Blish_HUD;
using Microsoft.Xna.Framework;
using Nekres.Mistwar.Entities;
using Nekres.Mistwar.UI.Controls;
using System;
using System.Collections.Generic;

namespace Nekres.Mistwar.Services
{
    internal class MarkerService : IDisposable
    {
        private MarkerBillboard _billboard;

        public MarkerService()
        {
            _billboard = new MarkerBillboard
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = GameService.Graphics.SpriteScreen.AbsoluteBounds.Size,
                Location = new Point(0, 0),
                Visible = false
            };
            GameService.Gw2Mumble.UI.IsMapOpenChanged += OnIsMapOpenChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged += OnIsInGameChanged;
        }

        public void ReloadMarkers(IEnumerable<WvwObjectiveEntity> entities)
        {
            _billboard.WvwObjectives = entities;
        }

        public void Toggle(bool forceHide = false)
        {
            _billboard?.Toggle(forceHide);
        }

        private void OnIsMapOpenChanged(object o, ValueEventArgs<bool> e)
        {
            if (!e.Value)
            {
                this.Toggle();
                return;
            }
            this.Toggle(true);
        }

        private void OnIsInGameChanged(object o, ValueEventArgs<bool> e)
        {
            if (e.Value)
            {
                this.Toggle();
                return;
            }
            this.Toggle(true);
        }

        public void Dispose()
        {
            GameService.Gw2Mumble.UI.IsMapOpenChanged -= OnIsMapOpenChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged -= OnIsInGameChanged;
            _billboard?.Dispose();
        }
    }
}

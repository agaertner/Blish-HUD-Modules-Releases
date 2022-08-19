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

        public MarkerService(IEnumerable<WvwObjectiveEntity> currentObjectives = null)
        {
            _billboard = new MarkerBillboard
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = GameService.Graphics.SpriteScreen.AbsoluteBounds.Size,
                Location = new Point(0, 0),
                Visible = currentObjectives != null,
                WvwObjectives = currentObjectives
            };
            GameService.Gw2Mumble.UI.IsMapOpenChanged += OnIsMapOpenChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged += OnIsInGameChanged;
        }

        public void ReloadMarkers(IEnumerable<WvwObjectiveEntity> entities)
        {
            _billboard.WvwObjectives = entities;
            this.Toggle(true);
        }

        public void Toggle(bool keepOpen = false)
        {
            if (!GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld())
            {
                return;
            }
            if (keepOpen && _billboard.Visible)
            {
                _billboard.Hide();
                _billboard.Show(); // We are already visible, but this fixes no icons being drawn.
                return;
            }
            _billboard?.Toggle();
        }

        private void OnIsMapOpenChanged(object o, ValueEventArgs<bool> e)
        {
            if (!e.Value)
            {
                this.Toggle();
                return;
            }
            _billboard.Hide();
        }

        private void OnIsInGameChanged(object o, ValueEventArgs<bool> e)
        {
            if (e.Value)
            {
                this.Toggle(true);
                return;
            }
            _billboard.Hide();
        }

        public void Dispose()
        {
            GameService.Gw2Mumble.UI.IsMapOpenChanged -= OnIsMapOpenChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged -= OnIsInGameChanged;
            _billboard?.Dispose();
        }
    }
}

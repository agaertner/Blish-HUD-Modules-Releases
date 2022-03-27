using Blish_HUD.Controls.Intern;
using Blish_HUD.Input;
using System;
using System.Drawing;
using Blish_HUD;

namespace Nekres.Inquest_Module.Core.Controllers
{
    internal class AutoClickController : IDisposable
    {
        private KeyBinding AutoClickHoldKey => InquestModule.ModuleInstance.AutoClickHoldKeySetting.Value;
        private KeyBinding AutoClickToggleKey => InquestModule.ModuleInstance.AutoClickToggleKeySetting.Value;

        private DateTime _nextHoldClick = DateTime.UtcNow;

        private DateTime _nextToggleClick = DateTime.UtcNow;

        private Point _togglePos;
        private bool _toggleActive;

        public AutoClickController()
        {
            AutoClickHoldKey.Enabled = true;
            AutoClickToggleKey.Enabled = true;
            AutoClickToggleKey.Activated += OnAutoClickToggleActivate;
        }

        private void OnAutoClickToggleActivate(object o, EventArgs e)
        {
            _togglePos = Mouse.GetPosition();
            _toggleActive = !_toggleActive;
        }

        public void Update()
        {
            if (!_toggleActive && AutoClickHoldKey.IsTriggering && DateTime.UtcNow > _nextHoldClick)
            {
                Mouse.DoubleClick(MouseButton.LEFT, -1, -1, true);
                _nextHoldClick = DateTime.UtcNow.AddMilliseconds(50);
            }

            if (_toggleActive && DateTime.UtcNow > _nextToggleClick)
            {
                Mouse.SetPosition(_togglePos.X, _togglePos.Y, true);
                Mouse.DoubleClick(MouseButton.LEFT, -1, -1, true);
                _nextToggleClick = DateTime.UtcNow.AddMilliseconds(RandomUtil.GetRandom(50,500));
            }
        }

        public void Dispose()
        {
            AutoClickToggleKey.Activated -= OnAutoClickToggleActivate;
        }
    }
}

using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Blish_HUD.Graphics.UI;

namespace Nekres.Music_Mixer.Core.UI.Views
{
    internal class AudioVolumeView : View
    {
        private const int MARGIN = 15;
        private Texture2D _texAudioBtn = GameService.Content.GetTexture("156738");
        private Texture2D _texAudioMuted = GameService.Content.GetTexture("common/154982");

        private TrackBar _volumeTrackBar;
        public AudioVolumeView()
        {
        }

        protected override void Build(Container buildPanel)
        {
            _volumeTrackBar = new TrackBar
            {
                Location = new Point(64 + MARGIN * 2, buildPanel.Height - 16 - MARGIN),
                Size = new Point(buildPanel.Width - MARGIN * 4 - _texAudioBtn.Width, 16),
                Parent = buildPanel
            };
            _volumeTrackBar.ValueChanged += OnValueChanged;
            base.Build(buildPanel);
        }

        private void OnValueChanged(object o, ValueEventArgs<float> e)
        {
            MusicMixer.Instance.MasterVolumeSetting.Value = e.Value;
        }

        private void OnMasterVolumeSettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            this.RefreshValue(e.NewValue);
        }

        private void RefreshValue(float value)
        {
            _volumeTrackBar.MinValue = Math.Min(_volumeTrackBar.MinValue, value);
            _volumeTrackBar.MaxValue = Math.Max(_volumeTrackBar.MaxValue, value);

            _volumeTrackBar.Value = value;
        }

        protected override void Unload()
        {
            _volumeTrackBar.ValueChanged -= OnValueChanged;
            MusicMixer.Instance.MasterVolumeSetting.SettingChanged -= OnMasterVolumeSettingChanged;
            base.Unload();
        }
    }
}

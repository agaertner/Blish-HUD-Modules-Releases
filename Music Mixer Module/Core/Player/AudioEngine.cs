using Nekres.Music_Mixer.Core.Player.API;
using System;

namespace Nekres.Music_Mixer.Core.Player
{
    internal class AudioEngine : IDisposable
    {
        private Soundtrack _soundtrack;

        private float _volume;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if (_soundtrack == null) return;
                _soundtrack.Volume = value;
            }
        }

        public AudioEngine()
        {
        }

        public async void Play(string url)
        {
            
            _soundtrack = Soundtrack.Play(await youtube_dl.Instance.GetAudioOnlyUrl(url), this.Volume);
            _soundtrack.FadeIn();
        }

        public void ToggleSubmerged(bool enable) => _soundtrack?.ToggleSubmergedFx(enable);

        public void Stop()
        {
            _soundtrack?.FadeOut();
        }

        public void Dispose()
        {
            _soundtrack?.Dispose();
        }
    }
}

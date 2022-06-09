using Blish_HUD;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Nekres.Music_Mixer.Core.Player.Source;
using Nekres.Music_Mixer.Core.Player.Source.DSP;
using Nekres.Music_Mixer.Core.Player.Source.Equalizer;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nekres.Music_Mixer.Core.Services;

namespace Nekres.Music_Mixer.Core.Player
{
    internal class Soundtrack : IDisposable
    {
        public event EventHandler<EventArgs> Finished;

        private WasapiOut _outputDevice;
        private MediaFoundationReader _mediaProvider;
        private EndOfStreamProvider _endOfStream;
        private VolumeSampleProvider _volumeProvider;
        private FadeInOutSampleProvider _fadeInOut;
        private BiQuadFilterSource _lowPassFilter;
        private Equalizer _equalizer;


        private float _volume;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if (_volumeProvider == null) return;
                _volumeProvider.Volume = value;
            }
        }

        public TimeSpan CurrentTime => _mediaProvider.CurrentTime;
        public TimeSpan TotalTime => _mediaProvider.TotalTime;
        public bool IsMuted => _volumeProvider.Volume == 0;
        public bool IsPaused => _outputDevice.PlaybackState == PlaybackState.Paused;

        private Soundtrack(string url, float volume)
        {
            _volume = volume;
            _outputDevice = new WasapiOut(GameService.GameIntegration.Audio.AudioDevice, AudioClientShareMode.Shared, false, 100);
            _mediaProvider = new MediaFoundationReader(url);
        }

        public static bool TryGetStream(string url, float volume, out Soundtrack soundTrack)
        {
            var timeout = DateTime.UtcNow.AddMilliseconds(500);
            while (DateTime.UtcNow < timeout)
            {
                try
                {
                    soundTrack = new Soundtrack(url, volume);
                    return true;
                }
                catch (Exception e) when (e is InvalidCastException or UnauthorizedAccessException or COMException)
                {
                    if (DateTime.UtcNow < timeout) continue;
                    MusicMixer.Logger.Error(e, e.Message);
                    break;
                }
            }
            soundTrack = null;
            return false;
        }

        private void Play(int fadeInDuration = 500)
        {
            if (this.IsPaused) {
                _outputDevice.Play();
                return;
            }

            _endOfStream = new EndOfStreamProvider(_mediaProvider.ToSampleProvider());
            _endOfStream.Ended += OnEndOfStreamReached;

            _volumeProvider = new VolumeSampleProvider(_endOfStream)
            {
                Volume = this.Volume
            };

            _fadeInOut = new FadeInOutSampleProvider(_volumeProvider);

            // Filter is toggled when submerged.
            _lowPassFilter = new BiQuadFilterSource(_fadeInOut)
            {
                Filter = new LowPassFilter(_fadeInOut.WaveFormat.SampleRate, 400),
                Enabled = MusicMixer.Instance.Gw2State.CurrentState != Gw2StateService.State.Submerged && MusicMixer.Instance.Gw2State.IsSubmerged
            };
            _equalizer = Equalizer.Create10BandEqualizer(_lowPassFilter);
            
            _outputDevice.Init(_equalizer);
            _outputDevice.Play();

            _fadeInOut.BeginFadeIn(fadeInDuration);
        }

        public void Seek(float seconds)
        {
            _mediaProvider?.SetPosition(seconds);
        }

        public void Pause()
        {
            _outputDevice?.Pause();
        }

        public void ToggleMuted()
        {
            if (this.IsMuted)
            {
                this.Volume = MusicMixer.Instance.MasterVolume;
            }
            else
            {
                this.Volume = 0;
            }
        }

        private void OnEndOfStreamReached(object o, EventArgs e)
        {
            _endOfStream.Ended -= OnEndOfStreamReached;
            this.Finished?.Invoke(this, EventArgs.Empty);
            this.Dispose();
        }

        public void ToggleSubmergedFx(bool enable)
        {
            if (_equalizer == null) return;
            _lowPassFilter.Enabled = enable;
            _equalizer.SampleFilters[1].AverageGainDB = enable ? 19.5f : 0; // Bass
            _equalizer.SampleFilters[9].AverageGainDB = enable ? 13.4f : 0; // Treble
        }

        public void FadeIn(int durationMs = 500)
        {
            this.Play(durationMs);
        }

        public async Task FadeOut(int durationMs = 500)
        {
            _endOfStream.Ended -= OnEndOfStreamReached;
            _fadeInOut.BeginFadeOut(durationMs);
            await Task.Delay(durationMs).ContinueWith(_ => this.Dispose());
        }

        public void Dispose()
        {
            try
            {
                _outputDevice?.Dispose();
                _mediaProvider?.Dispose();
            }
            catch (Exception e) when (e is NullReferenceException or ObjectDisposedException)
            {
                /* NOOP - Module was unloaded */
            }
        }
    }
}

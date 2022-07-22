using Blish_HUD;
using Nekres.Music_Mixer.Core.Player.API;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer.Core.Player
{
    internal class AudioEngine : IDisposable
    {
        private MediaWidget _mediaWidget;

        private Soundtrack _soundtrack;

        private Soundtrack _prevSoundtrack;

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

        public bool Loading { get; private set; }

        private TaskScheduler _scheduler;
        public AudioEngine()
        {
            _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        public async Task Play(MusicContextModel model)
        {
            if (this.Loading || model == null) return;
            this.Loading = true;

            if (string.IsNullOrEmpty(model.AudioUrl))
            {
                if (string.IsNullOrEmpty(model.Uri)) return;
                youtube_dl.GetAudioOnlyUrl(model.Uri, AudioUrlReceived, model);
                return;
            }

            if (!await TryPlay(model.AudioUrl)) return;
            await Notify(model);
        }

        private async Task AudioUrlReceived(string url, MusicContextModel model)
        {
            try
            {
                model.AudioUrl = url;
                await MusicMixer.Instance.DataService.Upsert(model);

                if (!await TryPlay(model.AudioUrl)) return;
                await Notify(model);
            }
            catch (Exception e) when (e is NullReferenceException or ObjectDisposedException)
            {
                /* NOOP - Module was being unloaded while youtube-dl exited and invoked this callback. */
            }
        }

        private async Task<bool> TryPlay(string audioUri)
        {
            // Making sure WasApiOut is initialized in main synchronization context. Otherwise it will fail.
            // https://github.com/naudio/NAudio/issues/425
            return await Task.Factory.StartNew(() => {
                    if (!Soundtrack.TryGetStream(audioUri, this.Volume, out _soundtrack))
                    {
                        this.Loading = false;
                        return false;
                    }

                    _soundtrack.Finished += OnSoundtrackFinished;
                    _soundtrack.FadeIn();

                    this.Loading = false;
                    return true;
            }, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        private async Task Notify(MusicContextModel model)
        {
            _mediaWidget ??= new MediaWidget
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = MusicMixer.Instance.MediaWidgetLocation.Value,
                Visible = false
            };
            _mediaWidget.Model = model;
            _mediaWidget.Soundtrack = _soundtrack;
            _mediaWidget.Show();
            await MusicMixer.Instance.DataService.GetThumbnail(model);
        }

        public void ToggleSubmerged(bool enable) => _soundtrack?.ToggleSubmergedFx(enable);

        public void Stop()
        {
            _mediaWidget?.Hide();
            _soundtrack?.FadeOut();
        }

        private async void OnSoundtrackFinished(object o, EventArgs e)
        {
            _soundtrack.Finished -= OnSoundtrackFinished;
            _mediaWidget?.Hide();
            if (this.Loading) return;
            await this.Play((await MusicMixer.Instance.DataService.GetRandom())?.ToModel());
        }

        public void Pause()
        {
            _prevSoundtrack?.Dispose();
            _prevSoundtrack = null;
            _soundtrack?.Pause();
            _prevSoundtrack = _soundtrack;
        }

        public bool Resume()
        {
            _soundtrack?.Dispose();
            _soundtrack = null;
            _soundtrack = _prevSoundtrack;
            if (_soundtrack == null)
            {
                return false;
            }
            _soundtrack?.FadeIn();
            return true;
        }

        public void Dispose()
        {
            _mediaWidget?.Dispose();
            _soundtrack?.Dispose();
            _prevSoundtrack?.Dispose();
        }
    }
}

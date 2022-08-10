using Blish_HUD;
using Nekres.Music_Mixer.Core.Player.API;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Nekres.Music_Mixer.Core.Player
{
    internal class AudioEngine : IDisposable
    {
        private MediaWidget _mediaWidget;

        private Soundtrack _soundtrack;

        private MusicContextModel _model;

        private TimeSpan _prevTime;

        private MusicContextModel _prevMusicModel;

        public bool Loading { get; private set; }

        private TaskScheduler _scheduler;

        public AudioEngine()
        {
            _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        public static float GetNormalizedVolume(float volume)
        {
            if (volume >= MusicMixer.Instance.MasterVolume) return MusicMixer.Instance.MasterVolume;
            return MathHelper.Clamp(MusicMixer.Instance.MasterVolume - Math.Abs(volume - MusicMixer.Instance.MasterVolume), 0f, 1f);
        }

        public void SetVolume(float volume)
        {
            _soundtrack.Volume = GetNormalizedVolume(volume);
        }

        public void RefreshVolume()
        {
            SetVolume(_model.Volume);
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

            _model = model;

            if (!await TryPlay(model.AudioUrl, GetNormalizedVolume(_model.Volume))) return;
            await Notify(model);
        }

        private async Task AudioUrlReceived(string url, MusicContextModel model)
        {
            try
            {
                model.AudioUrl = url;
                _model = model;
                await MusicMixer.Instance.DataService.Upsert(model);
                if (!await TryPlay(model.AudioUrl, GetNormalizedVolume(_model.Volume))) return;
                await Notify(model);
                
            }
            catch (Exception e) when (e is NullReferenceException or ObjectDisposedException)
            {
                /* NOOP - Module was being unloaded while youtube-dl exited and invoked this callback. */
            }
        }

        private async Task<bool> TryPlay(string audioUri, float volume)
        {
            // Making sure WasApiOut is initialized in main synchronization context. Otherwise it will fail.
            // https://github.com/naudio/NAudio/issues/425
            return await Task.Factory.StartNew(() => {

                    this.Stop();

                    if (!Soundtrack.TryGetStream(audioUri, volume, out _soundtrack))
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

        public void Update()
        {
            if (_soundtrack == null || _model == null) return;
            _soundtrack.Volume = MathHelper.Clamp(Map(GameService.Gw2Mumble.PlayerCamera.Position.Z, -130, 
                    GetNormalizedVolume(0.1f), 0, GetNormalizedVolume(_model.Volume)),0f,0.1f);
        }

        private static float Map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }

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
            _soundtrack?.Pause();
        }

        public void Resume()
        {
            _soundtrack?.FadeIn();
        }

        public void Save()
        {
            if (_soundtrack == null || _model == null || _model.State.IsIntermediate()) return;
            _prevTime = _soundtrack.CurrentTime;
            _prevMusicModel = _model;
        }

        public async Task<bool> PlayFromSave()
        {
            if (_prevMusicModel == null
                || !MusicContextModel.CanPlay(_prevMusicModel)
                || _prevTime > _prevMusicModel.Duration) // Time out of bounds
                return false;

            if (_soundtrack != null && _soundtrack.SourceUri.Equals(_prevMusicModel.AudioUrl)) return true; // Song is already active

            if (!await TryPlay(_prevMusicModel.AudioUrl, GetNormalizedVolume(_prevMusicModel.Volume))) return false;

            await Notify(_prevMusicModel);
            _soundtrack.Seek(_prevTime);

            return true;
        }

        public void Dispose()
        {
            _mediaWidget?.Dispose();
            _soundtrack?.Dispose();
        }
    }
}

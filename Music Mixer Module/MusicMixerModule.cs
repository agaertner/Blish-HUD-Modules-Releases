using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Gw2Sharp.Models;
using Nekres.Music_Mixer.Core.Player;
using Nekres.Music_Mixer.Core.Player.API;
using Nekres.Music_Mixer.Core.Services;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using static Blish_HUD.GameService;
using static Nekres.Music_Mixer.Core.Services.Gw2StateService;
namespace Nekres.Music_Mixer
{

    [Export(typeof(Module))]
    public class MusicMixerModule : Module
    {

        internal static readonly Logger Logger = Logger.GetLogger(typeof(MusicMixerModule));

        internal static MusicMixerModule ModuleInstance;

        #region Service Managers

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        #endregion

        #region Settings

        internal SettingEntry<float> MasterVolumeSetting;
        internal SettingEntry<bool> ToggleSubmergedPlaylistSetting;
        internal SettingEntry<bool> ToggleMountedPlaylistSetting;
        internal SettingEntry<bool> ToggleFourDayCycleSetting;
        internal SettingEntry<bool> ToggleKeepAudioFilesSetting;
        internal SettingEntry<AudioBitrate> AverageBitrateSetting;
        internal SettingEntry<bool> ToggleDebugHelper;

        #endregion

        public float MasterVolume => MathHelper.Clamp(MasterVolumeSetting.Value / 1000f, 0, 1);

        public string ModuleDirectory { get; private set; }

        private const string _FFmpegPath = "bin/ffmpeg.exe";
        private const string _youtubeDLPath = "bin/youtube-dl.exe";

        private DataPanel _debugPanel;
        private WindowTab _moduleTab;
        private Texture2D _tabIconTexture;
        private AudioEngine _audioEngine;
        internal DataService DataService;
        internal Gw2StateService Gw2State;

        [ImportingConstructor]
        public MusicMixerModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { ModuleInstance = this; }


        protected override void DefineSettings(SettingCollection settings) {
            MasterVolumeSetting = settings.DefineSetting("MasterVolume", 50f, 
                () => "Master Volume", 
                () => "Sets the audio volume.");

            ToggleSubmergedPlaylistSetting = settings.DefineSetting("EnableSubmergedPlaylist", false, 
                () => "Use submerged playlist", 
                () => "Whether songs of the underwater playlist should be played while submerged.");
            
            ToggleMountedPlaylistSetting = settings.DefineSetting("EnableMountedPlaylist", true, 
                () => "Use mounted playlist", 
                () => "Whether songs of the mounted playlist should be played while mounted.");
            
            ToggleFourDayCycleSetting = settings.DefineSetting("EnableFourDayCycle", false, 
                () => "Use dusk and dawn day cycles", 
                () => "Whether dusk and dawn track attributes should be interpreted as unique day cycles.\nOtherwise dusk and dawn will be interpreted as night and day respectively.");
            
            ToggleKeepAudioFilesSetting = settings.DefineSetting("KeepAudioFiles", false, 
                () => "Keep audio files on disk",
                () => "Whether streamed audio should be kept on disk.\nReduces delay for all future playback events after the first at the expense of disk space.\nMay also result in better audio quality.");

            AverageBitrateSetting = settings.DefineSetting("AverageBitrate", AudioBitrate.B320, 
                () => "Average bitrate limit", 
                () => "Sets the average bitrate of the audio used in streaming.");

            ToggleDebugHelper = settings.DefineSetting("EnableDebugHelper", false, 
                () => "Developer Mode", 
                () => "Exposes internal information helpful for development.");
        }

        protected override void Initialize() {
            ModuleDirectory = DirectoriesManager.GetFullDirectoryPath("music_mixer");
            Gw2State = new Gw2StateService();
            DataService = new DataService(this.ModuleDirectory);

            _audioEngine = new AudioEngine() { Volume = this.MasterVolume };

            _tabIconTexture = ContentsManager.GetTexture("tab_icon.png");
        }

        protected override void Update(GameTime gameTime) {
            Gw2State.Update();
        }

        protected override async Task LoadAsync() {
            await Task.Run(() => {
                ExtractFile(_FFmpegPath);
                ExtractFile(_youtubeDLPath);
            });

            //TODO: Remove
            await this.DataService.Upsert(new MusicContextModel
            {
                Uri = "https://www.youtube.com/watch?v=ax9JF36iEaI",
                States = new List<Gw2StateService.State> { Gw2StateService.State.Mounted },
                MapIds = new List<int>(),
                MountTypes = new List<MountType> { MountType.Raptor },
                DayTimes = new List<TyrianTime>(),
                SectorIds = new List<int>(),
                Title = "Chocobo Theme"
            });
            await this.DataService.Upsert(new MusicContextModel
            {
                Uri = "https://www.youtube.com/watch?v=YSRbX9bztpk",
                States = new List<Gw2StateService.State> { Gw2StateService.State.Mounted },
                MapIds = new List<int>(),
                MountTypes = new List<MountType> { MountType.Skimmer },
                DayTimes = new List<TyrianTime>(),
                SectorIds = new List<int>(),
                Title = "FFXIV OST Mount BGM ( The Rider's Boon )"
            });
        }

        protected override void OnModuleLoaded(EventArgs e) {
            MasterVolumeSetting.Value = MathHelper.Clamp(MasterVolumeSetting.Value, 0f, 100f);

            ToggleDebugHelper.SettingChanged += OnToggleDebugHelperChanged;
            MasterVolumeSetting.SettingChanged += MasterVolumeSettingChanged;
            Gw2State.IsSubmergedChanged += OnIsSubmergedChanged;
            Gw2State.StateChanged += OnStateChanged;

            if (ToggleDebugHelper.Value)
                BuildDebugPanel();
            
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private async void OnStateChanged(object o, ValueChangedEventArgs<State> e)
        {
            _audioEngine.Stop();
            var track = await this.DataService.GetRandom();
            if (track == null) return;
            _audioEngine.Play(track.Uri);
        }

        private void MasterVolumeSettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _audioEngine.Volume = MathHelper.Clamp(e.NewValue / 1000f, 0, 1);
        }

        private void OnIsSubmergedChanged(object o, ValueEventArgs<bool> e)
        {
            _audioEngine?.ToggleSubmerged(e.Value);
        }

        private void OnToggleDebugHelperChanged(object o, ValueChangedEventArgs<bool> e) {
            if (!GameIntegration.Gw2Instance.Gw2IsRunning) return;
            if (!e.NewValue) {
                _debugPanel?.Dispose();
                _debugPanel = null;
            } else
                BuildDebugPanel();
        }

        private void BuildDebugPanel() {
            _debugPanel?.Dispose();
            _debugPanel = new DataPanel {
                Parent = Graphics.SpriteScreen,
                Size = new Point(Graphics.SpriteScreen.Width, Graphics.SpriteScreen.Height),
                Location = new Point(0,0),
                ZIndex = -9999,
                CurrentState = Gw2State.CurrentState
            };
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            Overlay.BlishHudWindow.RemoveTab(_moduleTab);
            MasterVolumeSetting.SettingChanged -= MasterVolumeSettingChanged;
            Gw2State.IsSubmergedChanged -= OnIsSubmergedChanged;
            Gw2State.StateChanged -= OnStateChanged;
            ToggleDebugHelper.SettingChanged -= OnToggleDebugHelperChanged;
            _audioEngine?.Dispose();
            this.Gw2State?.Dispose();
            this.DataService?.Dispose();
            // All static members must be manually unset
            ModuleInstance = null;
        }

        private void ExtractFile(string filePath) {
            var fullPath = Path.Combine(ModuleDirectory, filePath);
            if (File.Exists(fullPath)) return;
            using (var fs = ContentsManager.GetFileStream(filePath)) {
                fs.Position = 0;
                byte[] buffer = new byte[fs.Length];
                var content = fs.Read(buffer, 0, (int)fs.Length);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllBytes(fullPath, buffer);
            }
        }
    }
}

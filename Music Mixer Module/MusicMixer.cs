using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Music_Mixer.Core.Player;
using Nekres.Music_Mixer.Core.Player.API;
using Nekres.Music_Mixer.Core.Services;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Views;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using static Blish_HUD.GameService;
using static Nekres.Music_Mixer.Core.Services.Gw2StateService;
namespace Nekres.Music_Mixer
{

    [Export(typeof(Module))]
    public class MusicMixer : Module
    {

        internal static readonly Logger Logger = Logger.GetLogger(typeof(MusicMixer));

        internal static MusicMixer Instance;

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

        private AudioEngine _audioEngine;
        internal DataService DataService;
        internal Gw2StateService Gw2State;

        private StandardWindow _moduleWindow;
        private CornerIcon _cornerIcon;

        // Textures
        private Texture2D _cornerTexture;
        private Texture2D _backgroundTexture;

        [ImportingConstructor]
        public MusicMixer([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { Instance = this; }


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
            
            /*ToggleKeepAudioFilesSetting = settings.DefineSetting("KeepAudioFiles", false, 
                () => "Keep audio files on disk",
                () => "Whether streamed audio should be kept on disk.\nReduces delay for all future playback events after the first at the expense of disk space.\nMay also result in better audio quality.");*/

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

            _audioEngine = new AudioEngine { Volume = this.MasterVolume };

            _cornerTexture = ContentsManager.GetTexture("corner_icon.png");
            _backgroundTexture = ContentsManager.GetTexture("background.png");
        }

        protected override void Update(GameTime gameTime) {
            this.Gw2State.Update();
        }

        protected override async Task LoadAsync() {
            await Task.Run(() => {
                ExtractFile(_FFmpegPath);
                ExtractFile(_youtubeDLPath);
            }).ContinueWith(async _ =>
            {
                var ver = await youtube_dl.Instance.Load();
                if (string.IsNullOrEmpty(ver))
                {
                    Logger.Warn($"Failed to update youtube-dl. Version could not be retrieved.");
                    return;
                }
                Logger.Info($"Using youtube-dl version: {ver}");
            });
        }

        protected override void OnModuleLoaded(EventArgs e) {
            MasterVolumeSetting.Value = MathHelper.Clamp(MasterVolumeSetting.Value, 0f, 100f);

            var windowRegion = new Rectangle(40, 26, 423, 780 - 56);
            var contentRegion = new Rectangle(70, 41, 380, 780 - 42);
            _moduleWindow = new StandardWindow(_backgroundTexture, windowRegion, contentRegion)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Emblem = _cornerTexture,
                Location = new Point((GameService.Graphics.SpriteScreen.Width - windowRegion.Width) / 2, (GameService.Graphics.SpriteScreen.Height) / 2),
                SavesPosition = true,
                Title = this.Name,
                Id = $"{nameof(MusicMixer)}_{nameof(LibraryView)}_d42b52ce-74f1-4e6d-ae6b-a8724029f0a3"
            };
            _cornerIcon = new CornerIcon
            {
                Icon = _cornerTexture
            };
            _cornerIcon.Click += OnModuleIconClick;

            ToggleDebugHelper.SettingChanged += OnToggleDebugHelperChanged;
            MasterVolumeSetting.SettingChanged += MasterVolumeSettingChanged;
            Gw2State.IsSubmergedChanged += OnIsSubmergedChanged;
            Gw2State.StateChanged += OnStateChanged;

            if (ToggleDebugHelper.Value)
                BuildDebugPanel();
            
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        public void OnModuleIconClick(object o, MouseEventArgs e)
        {
            _moduleWindow?.ToggleWindow(new LibraryView(new LibraryModel()));
        }

        private async void OnStateChanged(object o, ValueChangedEventArgs<State> e)
        {
            if (_debugPanel != null) _debugPanel.CurrentState = e.NewValue;

            _audioEngine.Stop();
            var track = await this.DataService.GetRandom();
            if (track == null) return;
            await _audioEngine.Play(track.Uri);
        }

        private void MasterVolumeSettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _audioEngine.Volume = MathHelper.Clamp(e.NewValue / 1000f, 0, 1);
        }

        private void OnIsSubmergedChanged(object o, ValueEventArgs<bool> e)
        {
            _audioEngine.ToggleSubmerged(e.Value);
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
            MasterVolumeSetting.SettingChanged -= MasterVolumeSettingChanged;
            Gw2State.IsSubmergedChanged -= OnIsSubmergedChanged;
            Gw2State.StateChanged -= OnStateChanged;
            ToggleDebugHelper.SettingChanged -= OnToggleDebugHelperChanged;
            _moduleWindow?.Dispose();
            _cornerIcon?.Dispose();
            _audioEngine?.Dispose();
            _debugPanel?.Dispose();
            this.Gw2State?.Dispose();
            this.DataService?.Dispose();
            // All static members must be manually unset
            Instance = null;
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

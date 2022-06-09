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
using Nekres.Music_Mixer.Core.UI.Views.StateViews;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;

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
        internal SettingEntry<Point> MediaWidgetLocation;
        private SettingEntry<bool> MuteWhenInBackgroundSetting;
        #endregion

        public float MasterVolume => MathHelper.Clamp(MasterVolumeSetting.Value / 1000f, 0, 1);

        public string ModuleDirectory { get; private set; }

        private const string _FFmpegPath = "bin/ffmpeg.exe";
        private const string _youtubeDLPath = "bin/youtube-dl.exe";

        private DataPanel _debugPanel;

        private AudioEngine _audioEngine;
        internal DataService DataService;
        internal Gw2StateService Gw2State;

        private TabbedWindow2 _moduleWindow;
        private CornerIcon _cornerIcon;

        // Textures
        private Texture2D _cornerTexture;
        private Texture2D _backgroundTexture;

        private Texture2D _mountTabIcon;
        private Texture2D _ambientTabIcon;
        private Texture2D _competitiveTabIcon;
        private Texture2D _submergedTabIcon;
        private Texture2D _battleTabIcon;

        [ImportingConstructor]
        public MusicMixer([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { Instance = this; }


        protected override void DefineSettings(SettingCollection settings) {
            MasterVolumeSetting = settings.DefineSetting("MasterVolume", 50f, 
                () => "Master Volume", 
                () => "Sets the audio volume.");

            MuteWhenInBackgroundSetting = settings.DefineSetting("MuteWhenInBackground", false,
                () => "Mute when GW2 is in the background");

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

            var selfManaged = settings.AddSubCollection("selfManaged", false, false);
            MediaWidgetLocation = selfManaged.DefineSetting("mediaWidgetLocation", new Point((int)(0.82 * GameService.Graphics.SpriteScreen.Size.X), 30));
        }

        protected override void Initialize() {
            ModuleDirectory = DirectoriesManager.GetFullDirectoryPath("music_mixer");
            Gw2State = new Gw2StateService();
            DataService = new DataService(this.ModuleDirectory);

            _audioEngine = new AudioEngine { Volume = this.MasterVolume };

            _cornerTexture = ContentsManager.GetTexture("corner_icon.png");
            _backgroundTexture = GameService.Content.GetTexture("controls/window/502049");

            _mountTabIcon = ContentsManager.GetTexture("tabs/raptor.png");
            _ambientTabIcon = ContentsManager.GetTexture("tabs/campfire.png");
            _competitiveTabIcon = ContentsManager.GetTexture("tabs/arena.png");
            _submergedTabIcon = ContentsManager.GetTexture("tabs/waterdrop.png");
            _battleTabIcon = ContentsManager.GetTexture("tabs/enemy.png");
        }

        protected override void Update(GameTime gameTime) {
            this.Gw2State.Update();
        }

        protected override async Task LoadAsync() {
            await Task.Run(() => {
                ExtractFile(_FFmpegPath);
                ExtractFile(_youtubeDLPath);
            }).ContinueWith(_ => youtube_dl.Load());
        }

        protected override void OnModuleLoaded(EventArgs e) {
            MasterVolumeSetting.Value = MathHelper.Clamp(MasterVolumeSetting.Value, 0f, 100f);

            var windowRegion = new Rectangle(40, 26, 895 + 38, 780 - 56);
            var contentRegion = new Rectangle(70, 41, 895 - 43, 780 - 142);
            _moduleWindow = new TabbedWindow2(_backgroundTexture, windowRegion, contentRegion)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Emblem = _cornerTexture,
                Location = new Point((GameService.Graphics.SpriteScreen.Width - windowRegion.Width) / 2, (GameService.Graphics.SpriteScreen.Height) / 2),
                SavesPosition = true,
                Title = this.Name,
                Id = $"{nameof(MusicMixer)}_d42b52ce-74f1-4e6d-ae6b-a8724029f0a3"
            };   
            _moduleWindow.Tabs.Add(new Tab(_mountTabIcon, () => new StateView(Gw2StateService.State.Mounted), "Mounted"));
            _moduleWindow.Tabs.Add(new Tab(_ambientTabIcon, () => new StateView(Gw2StateService.State.Ambient), "Ambient"));
            _moduleWindow.Tabs.Add(new Tab(_competitiveTabIcon, () => new StateView(Gw2StateService.State.Competitive), "Competitive"));
            _moduleWindow.Tabs.Add(new Tab(_submergedTabIcon, () => new StateView(Gw2StateService.State.Submerged), "Submerged"));
            _moduleWindow.Tabs.Add(new Tab(_battleTabIcon, () => new StateView(Gw2StateService.State.Battle), "Battle"));
            _cornerIcon = new CornerIcon
            {
                Icon = _cornerTexture
            };
            _cornerIcon.Click += OnModuleIconClick;

            ToggleDebugHelper.SettingChanged += OnToggleDebugHelperChanged;
            MasterVolumeSetting.SettingChanged += MasterVolumeSettingChanged;
            Gw2State.IsSubmergedChanged += OnIsSubmergedChanged;
            Gw2State.StateChanged += OnStateChanged;
            GameService.GameIntegration.Gw2Instance.Gw2LostFocus += OnGw2LostFocus;
            GameService.GameIntegration.Gw2Instance.Gw2AcquiredFocus += OnGw2AcquiredFocus;
            GameService.GameIntegration.Gw2Instance.Gw2Closed += OnGw2Closed;

            if (ToggleDebugHelper.Value)
                BuildDebugPanel();
            
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void OnGw2LostFocus(object o, EventArgs e)
        {
            if (_audioEngine == null || !MuteWhenInBackgroundSetting.Value) return;
            _audioEngine.Volume = 0;
        }

        private void OnGw2AcquiredFocus(object o, EventArgs e)
        {
            if (_audioEngine == null) return;
            _audioEngine.Volume = this.MasterVolume;
        }
        private void OnGw2Closed(object o, EventArgs e)
        {
            _audioEngine?.Stop();
        }

        public void OnModuleIconClick(object o, MouseEventArgs e)
        {
            _moduleWindow?.ToggleWindow();
        }

        private async void OnStateChanged(object o, ValueChangedEventArgs<Gw2StateService.State> e)
        {
            if (_debugPanel != null) _debugPanel.CurrentState = e.NewValue;
            _audioEngine.Stop();
            await _audioEngine.Play((await this.DataService.GetRandom())?.ToModel());
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
            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning) return;
            if (!e.NewValue) {
                _debugPanel?.Dispose();
                _debugPanel = null;
            } else
                BuildDebugPanel();
        }

        private void BuildDebugPanel() {
            _debugPanel?.Dispose();
            _debugPanel = new DataPanel {
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(GameService.Graphics.SpriteScreen.Width, GameService.Graphics.SpriteScreen.Height),
                Location = new Point(0,0),
                ZIndex = -9999,
                CurrentState = Gw2State.CurrentState
            };
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            GameService.GameIntegration.Gw2Instance.Gw2LostFocus -= OnGw2LostFocus;
            GameService.GameIntegration.Gw2Instance.Gw2AcquiredFocus -= OnGw2AcquiredFocus;
            GameService.GameIntegration.Gw2Instance.Gw2Closed -= OnGw2Closed;
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

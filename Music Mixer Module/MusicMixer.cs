using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Music_Mixer.Core.Player;
using Nekres.Music_Mixer.Core.Player.API;
using Nekres.Music_Mixer.Core.Services;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Views;
using Nekres.Music_Mixer.Core.UI.Views.StateViews;
using System;
using System.Collections.Generic;
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
        internal SettingEntry<bool> ToggleMountedPlaylistSetting;
        internal SettingEntry<bool> ToggleFourDayCycleSetting;
        internal SettingEntry<bool> ToggleKeepAudioFilesSetting;
        internal SettingEntry<AudioBitrate> AverageBitrateSetting;
        internal SettingEntry<bool> ToggleDebugHelper;
        internal SettingEntry<Point> MediaWidgetLocation;
        private SettingEntry<bool> MuteWhenInBackgroundSetting;
        #endregion

        public float MasterVolume => MathHelper.Clamp(MasterVolumeSetting.Value / 1000f, 0f, 1f);

        public string ModuleDirectory { get; private set; }

        private const string _FFmpegPath = "bin/ffmpeg.exe";
        private const string _youtubeDLPath = "bin/youtube-dl.exe";

        private DataPanel _debugPanel;

        internal AudioEngine AudioEngine;
        internal MapService MapService;
        internal DataService DataService;
        internal Gw2StateService Gw2State;

        private TabbedWindow2 _moduleWindow;
        private CornerIcon _cornerIcon;

        private Dictionary<Gw2StateService.State, MainModel> _tabModels;

        // Textures
        private Texture2D _cornerTexture;
        private Texture2D _backgroundTexture;

        private Texture2D _mountTabIcon;
        private Texture2D _ambientTabIcon;
        private Texture2D _competitiveTabIcon;
        private Texture2D _battleTabIcon;

        [ImportingConstructor]
        public MusicMixer([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { Instance = this; }

        protected override void DefineSettings(SettingCollection settings) {
            MasterVolumeSetting = settings.DefineSetting("MasterVolume", 50f, 
                () => "Master Volume", 
                () => "Sets the audio volume.");

            MuteWhenInBackgroundSetting = settings.DefineSetting("MuteWhenInBackground", false,
                () => "Mute when GW2 is in the background");

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

        protected override void Initialize()
        {
            ModuleDirectory = DirectoriesManager.GetFullDirectoryPath("music_mixer");
            MapService = new MapService(this.ContentsManager, GetModuleProgressHandler());
            DataService = new DataService(this.ModuleDirectory);
            Gw2State = new Gw2StateService();
            AudioEngine = new AudioEngine();

            _cornerTexture = ContentsManager.GetTexture("corner_icon.png");
            _backgroundTexture = GameService.Content.GetTexture("controls/window/502049");

            _mountTabIcon = ContentsManager.GetTexture("tabs/raptor.png");
            _ambientTabIcon = ContentsManager.GetTexture("tabs/campfire.png");
            _competitiveTabIcon = ContentsManager.GetTexture("tabs/arena.png");
            _battleTabIcon = ContentsManager.GetTexture("tabs/enemy.png");

            _tabModels = new Dictionary<Gw2StateService.State, MainModel>
            {
                {Gw2StateService.State.Mounted, new MainModel(Gw2StateService.State.Mounted){MountType = MountType.Raptor, MapId = 0}},
                {Gw2StateService.State.Ambient, new MainModel(Gw2StateService.State.Ambient)},
                {Gw2StateService.State.Competitive, new MainModel(Gw2StateService.State.Competitive){ContinentId = 2, RegionId = 6, MapId = 350}},
                {Gw2StateService.State.Battle, new MainModel(Gw2StateService.State.Battle)},
            };
        }

        protected override void Update(GameTime gameTime) {
            this.Gw2State.Update();
            this.AudioEngine.Update();
        }

        protected override async Task LoadAsync()
        {
            this.MapService.DownloadRegions();
            await Task.Run(() => {
                //ExtractFile(_FFmpegPath);
                ExtractFile(_youtubeDLPath);
            }).ContinueWith(_ => youtube_dl.Load());
        }

        private void UpdateModuleLoading(string loadingMessage)
        {
            if (_cornerIcon == null) return;
            _cornerIcon.LoadingMessage = loadingMessage;
        }

        public IProgress<string> GetModuleProgressHandler()
        {
            return new Progress<string>(UpdateModuleLoading);
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

            _moduleWindow.Tabs.Add(new Tab(_mountTabIcon, () => new MountView(_tabModels[Gw2StateService.State.Mounted]), "Mounted"));
            _moduleWindow.Tabs.Add(new Tab(_ambientTabIcon, () => new MainView(_tabModels[Gw2StateService.State.Ambient]), "Ambient"));
            _moduleWindow.Tabs.Add(new Tab(_competitiveTabIcon, () => new MainView(_tabModels[Gw2StateService.State.Competitive]), "Competitive"));
            //_moduleWindow.Tabs.Add(new Tab(_submergedTabIcon, () => new MainView(_tabModels[Gw2StateService.State.Submerged]), "Submerged"));
            _moduleWindow.Tabs.Add(new Tab(_battleTabIcon, () => new MainView(_tabModels[Gw2StateService.State.Battle]), "Battle"));
            _cornerIcon = new CornerIcon
            {
                Icon = _cornerTexture,
                BasicTooltipText = $"{this.Name}\nRight-Click: Toggle Media Widget"
            };
            _cornerIcon.LeftMouseButtonReleased += OnModuleIconClick;
            _cornerIcon.RightMouseButtonReleased += OnModuleIconRightMouseButtonReleased;

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
            if (!MuteWhenInBackgroundSetting.Value) return;
            AudioEngine?.Pause();
        }

        private void OnGw2AcquiredFocus(object o, EventArgs e)
        {
            AudioEngine?.Resume();
        }
        private void OnGw2Closed(object o, EventArgs e)
        {
            AudioEngine?.Stop();
        }

        public void OnModuleIconClick(object o, MouseEventArgs e)
        {
            if (this.MapService.IsLoading) return;
            _moduleWindow?.ToggleWindow();
        }

        public void OnModuleIconRightMouseButtonReleased(object o, MouseEventArgs e)
        {
            this.AudioEngine.ToggleMediaWidget();
        }

        private async void OnStateChanged(object o, ValueChangedEventArgs<Gw2StateService.State> e)
        {
            if (_debugPanel != null) _debugPanel.CurrentState = e.NewValue;

            if (e.PreviousValue == Gw2StateService.State.Ambient)
            {
                switch (e.NewValue)
                {
                    // Save the ambient music when we are in an intermediate state.
                    case Gw2StateService.State.Mounted:
                    case Gw2StateService.State.Battle:
                    case Gw2StateService.State.Submerged:
                    case Gw2StateService.State.Victory:
                        AudioEngine.Save();
                        break;
                }
            }

            // Resume ambient music.
            if (e.NewValue == Gw2StateService.State.Ambient)
            {
                if (await AudioEngine.PlayFromSave()) return;
            }

            // Select new song if nothing is playing.
            await AudioEngine.Play(this.DataService.GetRandom()?.ToModel());
        }

        private void MasterVolumeSettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            AudioEngine.RefreshVolume();
        }

        private void OnIsSubmergedChanged(object o, ValueEventArgs<bool> e)
        {
            AudioEngine.ToggleSubmerged(e.Value);
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
            if (_cornerIcon != null)
            {
                _cornerIcon.LeftMouseButtonReleased -= OnModuleIconClick;
                _cornerIcon.RightMouseButtonReleased -= OnModuleIconRightMouseButtonReleased;
                _cornerIcon.Dispose();
            }
            AudioEngine?.Dispose();
            _debugPanel?.Dispose();
            this.Gw2State?.Dispose();
            this.DataService?.Dispose();
            _mountTabIcon?.Dispose();
            _battleTabIcon?.Dispose();
            _ambientTabIcon?.Dispose();
            _competitiveTabIcon?.Dispose();
            _backgroundTexture?.Dispose();
            _cornerTexture?.Dispose();

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

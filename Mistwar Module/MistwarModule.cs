using Blish_HUD;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nekres.Mistwar.UI.Controls;
using Nekres.Mistwar.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Nekres.Mistwar.UI.Models;
using Nekres.Mistwar.UI.Views;
using static Blish_HUD.GameService;
namespace Nekres.Mistwar
{
    [Export(typeof(Module))]
    public class MistwarModule : Module
    {

        internal static readonly Logger Logger = Logger.GetLogger<MistwarModule>();

        internal static MistwarModule ModuleInstance;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public MistwarModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { ModuleInstance = this; }

        internal SettingEntry<KeyBinding> ToggleKeySetting;
        internal SettingEntry<ColorType> ColorTypeSetting;
        internal SettingEntry<float> ColorIntensitySetting;
        internal SettingEntry<bool> DrawSectorsSetting;
        internal SettingEntry<bool> UseCustomIconsSetting;
        internal SettingEntry<float> ScaleRatioSetting;
        internal SettingEntry<bool> DrawObjectiveNamesSetting;
        internal SettingEntry<float> OpacitySetting;
        protected override void DefineSettings(SettingCollection settings)
        {
            ToggleKeySetting = settings.DefineSetting("ToggleKey", new KeyBinding(Keys.N), () => "Toggle Key", () => "Key used to show and hide the tactical map overlay.");
            ColorTypeSetting = settings.DefineSetting("ColorType", ColorType.Normal, () => "Color Type", () => "Select a different color type if you have a color deficiency.");
            ColorIntensitySetting = settings.DefineSetting("ColorIntensity", 80f, () => "Color Intensity", () => "Intensity of the background color.");
            OpacitySetting = settings.DefineSetting("Opacity", 80f, () => "Opacity", () => "Changes the opacity of the tactical map interface.");
            ScaleRatioSetting = settings.DefineSetting("ScaleRatio", 80f, () => "Scale Ratio", () => "Changes the size of the tactical map interface");
            DrawSectorsSetting = settings.DefineSetting("DrawSectors", true, () => "Draw Sector Boundaries", () => "Indicates if the sector boundaries should be drawn.");
            DrawObjectiveNamesSetting = settings.DefineSetting("DrawObjectiveNames", true, () => "Draw Objective Names", () => "Indicates if the names of the objectives should be drawn.");
            UseCustomIconsSetting = settings.DefineSetting("UseCustomIcons", false, () => "Use Custom Icons", () => "Indicates if icons provided by the module should be used for objective states.");
        }

        public override IView GetSettingsView()
        {
            return new CustomSettingsView(new CustomSettingsModel(SettingsManager.ModuleSettings));
        }

        public int WorldId { get; private set; }

        private MapImage _mapControl;
        private Dictionary<int, IEnumerable<WvwObjectiveEntity>> _wvwObjectiveCache;
        private bool _enabled;
        protected override void Initialize()
        {
            _wvwObjectiveCache = new Dictionary<int, IEnumerable<WvwObjectiveEntity>>();
            _mapControl = new MapImage
            {
                Parent = Graphics.SpriteScreen,
                Size = new Point(0, 0),
                Location = new Point(0, 0)
            };
            _mapControl.Hide();
        }

        protected override async Task LoadAsync()
        {

        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            Gw2ApiManager.SubtokenUpdated += OnSubtokenUpdated;
            Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
            ColorIntensitySetting.SettingChanged += OnColorIntensitySettingChanged;
            OnColorIntensitySettingChanged(null, new ValueChangedEventArgs<float>(0, ColorIntensitySetting.Value));
            OnOpacitySettingChanged(null, new ValueChangedEventArgs<float>(0,OpacitySetting.Value));
            ToggleKeySetting.Value.Activated += OnToggleKeyActivated;
            Gw2Mumble.UI.IsMapOpenChanged += OnIsMapOpenChanged;
            GameIntegration.Gw2Instance.IsInGameChanged += OnIsInGameChanged;
            OpacitySetting.SettingChanged += OnOpacitySettingChanged;
            ToggleKeySetting.Value.Enabled = true;
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void OnIsInGameChanged(object o, ValueEventArgs<bool> e)
        {
            if (!e.Value)
                ToggleMapControl(false, 0.1f, true);
        }

        private bool IsUiAvailable() => Gw2Mumble.IsAvailable && GameIntegration.Gw2Instance.IsInGame && !Gw2Mumble.UI.IsMapOpen;

        private void OnIsMapOpenChanged(object o, ValueEventArgs<bool> e)
        {
            if (e.Value)
                ToggleMapControl(false, 0.1f, true);
        }

        private void OnOpacitySettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _mapControl.SetOpacity(MathHelper.Clamp(e.NewValue / 100f, 0, 1));
        }

        private void OnToggleKeyActivated(object o, EventArgs e)
        {
            if (!IsUiAvailable() || !Gw2Mumble.CurrentMap.Type.IsWorldVsWorld()) return;
            ToggleMapControl(!_enabled, 0.1f);
        }

        private void ToggleMapControl(bool enabled, float tDuration, bool silent = false)
        {
            _enabled = enabled;
            if (enabled)
            {
                _mapControl.Show();
                if (silent) return;
                Content.PlaySoundEffectByName("page-open-" + RandomUtil.GetRandom(1, 3));
                Animation.Tweener.Tween(_mapControl, new { Opacity = 1.0f }, 0.35f);
            }
            else
            {
                if (silent) {
                    _mapControl?.Hide();
                    return;
                }
                Animation.Tweener.Tween(_mapControl, new {Opacity = 0.0f}, tDuration).OnComplete(() => _mapControl?.Hide());
                Content.PlaySoundEffectByName("window-close");
            }

        }

        private void OnColorIntensitySettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _mapControl.GrayscaleIntensity = (100 - e.NewValue) / 100f;
        }

        private async void OnSubtokenUpdated(object o, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            if (!e.Value.Contains(TokenPermission.Account)) return;
            await Gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync().ContinueWith(task =>
            {
                if (task.IsFaulted) return;
                WorldId = task.Result.World;
                OnMapChanged(null,new ValueEventArgs<int>(Gw2Mumble.CurrentMap.Id));
            });
        }

        private async void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            if (!Gw2Mumble.CurrentMap.Type.IsWorldVsWorld())
            {
                ToggleKeySetting.Value.Enabled = false;
                ToggleMapControl(false, 0.1f, true);
                return;
            }
            ToggleKeySetting.Value.Enabled = true;

            using (var imageStream = await MapUtil.DrawMapImage(e.Value, true, $"{DirectoriesManager.GetFullDirectoryPath("mistwar")}/{e.Value}.png"))
            {
                if (imageStream == null)
                    return;
                var tex = Texture2D.FromStream(Graphics.GraphicsDevice, imageStream);
                _mapControl.Texture.SwapTexture(tex);
            }

            if (_wvwObjectiveCache.TryGetValue(e.Value, out var currentObjectives))
            {
                _mapControl.WvwObjectives = currentObjectives;
                return;
            }

            await Gw2ApiManager.Gw2ApiClient.V2.Wvw.Objectives.AllAsync().ContinueWith(async task =>
            {
                if (task.IsFaulted) return;
                var map = await MapUtil.RequestMap(e.Value);
                var sectors = await MapUtil.RequestSectorsForFloor(map.ContinentId, map.DefaultFloor, map.RegionId, e.Value);
                if (sectors == null) return;

                var newObjectives = new List<WvwObjectiveEntity>();
                foreach (var sector in sectors)
                {
                    var obj = task.Result.FirstOrDefault(x => x.SectorId == sector.Id);
                    if (obj == null) continue;
                    newObjectives.Add(new WvwObjectiveEntity(obj, map, sector));
                }
                _mapControl.WvwObjectives = newObjectives;
                _wvwObjectiveCache.Add(map.Id, _mapControl.WvwObjectives);
            });
        }

        private DateTime? _prevApiRequestTime;
        protected override async void Update(GameTime gameTime)
        {
            if (!Gw2Mumble.CurrentMap.Type.IsWorldVsWorld() || !_wvwObjectiveCache.ContainsKey(Gw2Mumble.CurrentMap.Id)) return;

            if (_prevApiRequestTime.HasValue && DateTime.UtcNow.Subtract(_prevApiRequestTime.Value).TotalSeconds < 15)
                return;

            _prevApiRequestTime = DateTime.UtcNow;

            await Task.Run(() => UpdateObjectives(WorldId, Gw2Mumble.CurrentMap.Id));
        }
        private async Task UpdateObjectives(int worldId, int mapId)
        {
            var objectives = await RequestObjectives(worldId, mapId);
            if (objectives == null) return;

            if (!_wvwObjectiveCache.TryGetValue(mapId, out var objEntities)) return;

            foreach (var objEntity in objEntities)
            {
                var obj = objectives.First(v => v.Id.Equals(objEntity.Id, StringComparison.InvariantCultureIgnoreCase));
                objEntity.LastFlipped = obj.LastFlipped ?? DateTime.MinValue;
                objEntity.Owner = obj.Owner.Value;
                objEntity.ClaimedBy = obj.ClaimedBy ?? Guid.Empty;
                objEntity.GuildUpgrades = obj.GuildUpgrades;
                objEntity.YaksDelivered = obj.YaksDelivered ?? 0;
            }
        }

        private async Task<IReadOnlyList<WvwMatchMapObjective>> RequestObjectives(int worldId, int mapId)
        {
            return await Gw2ApiManager.Gw2ApiClient.V2.Wvw.Matches.World(worldId).GetAsync()
                .ContinueWith(task =>
                {
                    if (task.IsFaulted) return null;
                    return task.Result.Maps.FirstOrDefault(x => x.Id == mapId)?.Objectives;
                });
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            ToggleKeySetting.Value.Enabled = false;
            Gw2ApiManager.SubtokenUpdated -= OnSubtokenUpdated;
            Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
            ColorIntensitySetting.SettingChanged -= OnColorIntensitySettingChanged;
            Gw2Mumble.UI.IsMapOpenChanged -= OnIsMapOpenChanged;
            GameIntegration.Gw2Instance.IsInGameChanged -= OnIsInGameChanged;
            OpacitySetting.SettingChanged -= OnOpacitySettingChanged;
            _mapControl?.Texture?.Dispose();
            _mapControl?.Dispose();
            // All static members must be manually unset
            ModuleInstance = null;
        }

    }

}

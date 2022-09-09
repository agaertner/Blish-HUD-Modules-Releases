using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Extended.Core.Views;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nekres.Mumble_Info.Core.Controls;
using Nekres.Mumble_Info.Core.Services;
using static Blish_HUD.GameService;
using Color = Microsoft.Xna.Framework.Color;

namespace Nekres.Mumble_Info
{
    [Export(typeof(Module))]
    public class MumbleInfoModule : Module
    {

        internal static readonly Logger Logger = Logger.GetLogger(typeof(MumbleInfoModule));

        internal static MumbleInfoModule Instance;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public MumbleInfoModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { Instance = this; }

        #region Settings

        private SettingEntry<KeyBinding> _toggleInfoBinding;
        private SettingEntry<bool>       _showCursorPosition;
        internal SettingEntry<bool> EnablePerformanceCounters;
        internal SettingEntry<bool> SwapYZAxes;

        #endregion

        internal Map            CurrentMap  { get; private set; }
        internal Specialization CurrentSpec { get; private set; }
        internal float          MemoryUsage { get; private set; }
        internal float          CpuUsage    { get; private set; }
        internal string         CpuName     { get; private set; }

        private DataPanel          _dataPanel;
        private Label              _cursorPos;
        private PerformanceCounter _ramCounter;
        private PerformanceCounter _cpuCounter;
        private DateTime           _timeOutPc;

        private MockService _mockService;

        protected override void DefineSettings(SettingCollection settings) {
            _toggleInfoBinding = settings.DefineSetting("ToggleInfoBinding", new KeyBinding(Keys.OemPlus),
                () => "Toggle display", 
                () => "Toggles the display of data.");

            _showCursorPosition = settings.DefineSetting("ShowCursorPosition", false,
                () => "Show cursor position",
                () => "Whether the cursor's current interface-relative position should be displayed.\nUse [Left Alt] to copy it.");

            EnablePerformanceCounters = settings.DefineSetting("PerfCountersEnabled", false,
                () => "Show performance counters",
                () => "Whether performance counters such as RAM and CPU utilization of the Guild Wars 2 process should be displayed.");

            SwapYZAxes = settings.DefineSetting("SwapYZAxes", true, 
                () => "Swap YZ Axes",
                () => "Swaps the values of the Y and Z axes if enabled.");
        }

        protected override void Initialize() {
            _timeOutPc = DateTime.UtcNow;
            CpuName    = string.Empty;
            _mockService = new MockService();
        }

        public override IView GetSettingsView()
        {
            return new SocialsSettingsView(new SocialsSettingsModel(this.SettingsManager.ModuleSettings, "https://pastebin.com/raw/Kk9DgVmL"));
        }

        protected override async Task LoadAsync()
        {
            if (!EnablePerformanceCounters.Value) return;
            await LoadPerformanceCounters();
            await QueryManagementObjects();
        }

        protected override void Update(GameTime gameTime) {
            UpdateCounter();
            UpdateCursorPos();
            _mockService.Update();
        }

        protected override void OnModuleLoaded(EventArgs e) {
            _toggleInfoBinding.Value.Enabled = true;
            _toggleInfoBinding.Value.Activated += OnToggleInfoBindingActivated;
            _showCursorPosition.SettingChanged += OnShowCursorPositionSettingChanged;
            Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
            Gw2Mumble.PlayerCharacter.SpecializationChanged += OnSpecializationChanged;
            GameIntegration.Gw2Instance.Gw2Closed += OnGw2Closed;
            GameIntegration.Gw2Instance.Gw2Started += OnGw2Started;
            EnablePerformanceCounters.SettingChanged += OnEnablePerformanceCountersSettingChanged;
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private Task LoadPerformanceCounters() {
            return Task.Run(() => { 
                if (!GameIntegration.Gw2Instance.Gw2IsRunning) return;
                _ramCounter = new PerformanceCounter() {
                    CategoryName = "Process",
                    CounterName = "Working Set - Private",
                    InstanceName = GameIntegration.Gw2Instance.Gw2Process.ProcessName,
                    ReadOnly = true
                };
                _cpuCounter = new PerformanceCounter() {
                    CategoryName = "Process",
                    CounterName = "% Processor Time",
                    InstanceName = GameIntegration.Gw2Instance.Gw2Process.ProcessName,
                    ReadOnly = true
                };
            });
        }

        private Task QueryManagementObjects() {
            return Task.Run(() =>
            {
                using var mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                foreach (var o in mos.Get()) {
                    var mo = (ManagementObject) o;
                    var name = (string)mo["Name"];
                    if (name.Length < CpuName.Length) continue;
                    CpuName = name.Trim();
                }
            });
        }

        private async void OnEnablePerformanceCountersSettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (e.PreviousValue == e.NewValue) return;
            if (!e.NewValue) {
                _ramCounter?.Dispose();
                _cpuCounter?.Dispose();
                return;
            }
            await LoadPerformanceCounters();
            await QueryManagementObjects();
        }

        private void OnGw2Closed(object o, EventArgs e) {
            _dataPanel?.Dispose();
            _cursorPos?.Dispose();
            _ramCounter?.Dispose();
            _cpuCounter?.Dispose();
        }

        private async void OnGw2Started(object o, EventArgs e) => await LoadPerformanceCounters();

        private void UpdateCounter() {
            if (!GameIntegration.Gw2Instance.Gw2IsRunning || _ramCounter == null || _cpuCounter == null) return;
            if (DateTime.UtcNow < _timeOutPc) return;

            _timeOutPc = DateTime.UtcNow.AddMilliseconds(1000);
            MemoryUsage = _ramCounter.NextValue() / 1024 / 1024;
            CpuUsage = _cpuCounter.NextValue() / Environment.ProcessorCount;
        }

        private void UpdateCursorPos()
        {
            if (!GameIntegration.Gw2Instance.Gw2IsRunning || _cursorPos == null) return;
            _cursorPos.Visible = !Input.Mouse.CameraDragging;
            _cursorPos.Text = PInvoke.IsLControlPressed() ? 
                $"X: {Input.Mouse.Position.X - Graphics.SpriteScreen.Width / 2}, Y: {Math.Abs(Input.Mouse.Position.Y - Graphics.SpriteScreen.Height)}" :
                $"X: {Input.Mouse.Position.X}, Y: {Input.Mouse.Position.Y}";
            _cursorPos.Location = new Point(Input.Mouse.Position.X + 50, Input.Mouse.Position.Y);
        }

        private void OnToggleInfoBindingActivated(object o, EventArgs e) {
            if (!GameIntegration.Gw2Instance.Gw2IsRunning || Gw2Mumble.UI.IsTextInputFocused) return;
            if (_dataPanel != null) {
                _dataPanel.Dispose();
                _dataPanel = null;
            } else {
                BuildDisplay();
            }
            if (_cursorPos != null) {
                _cursorPos.Dispose();
                _cursorPos = null;
            } else {
                BuildCursorPosTooltip();
            }
        }

        private void OnShowCursorPositionSettingChanged(object o, ValueChangedEventArgs<bool> e)
        {
            if (!e.NewValue)
                _cursorPos?.Dispose();
            else if (_dataPanel != null)
                BuildCursorPosTooltip();
        }

        private void BuildCursorPosTooltip()
        {
            if (!_showCursorPosition.Value) return;
            _cursorPos?.Dispose();
            _cursorPos = new Label()
            {
                Parent = Graphics.SpriteScreen,
                Size = new Point(130, 20),
                StrokeText = true,
                ShowShadow = true,
                Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y),
                VerticalAlignment = VerticalAlignment.Top,
                ZIndex = -9999
            };
            Input.Keyboard.KeyPressed += OnKeyPressed;
            Input.Keyboard.KeyReleased += OnKeyReleased;
            _cursorPos.Disposed += (o, e) =>
            {
                Input.Keyboard.KeyPressed -= OnKeyPressed;
                Input.Keyboard.KeyReleased -= OnKeyReleased;
            };
        }

        private void OnKeyPressed(object o, KeyboardEventArgs e)
        {
            if (_cursorPos == null || Input.Mouse.CameraDragging || e.Key != Keys.LeftAlt) return;
            _cursorPos.TextColor = new Color(252, 252, 84);
        }

        private void OnKeyReleased(object o, KeyboardEventArgs e)
        {
            if (_cursorPos == null || Input.Mouse.CameraDragging || e.Key != Keys.LeftAlt) return;
            _cursorPos.TextColor = Color.White;
            ClipboardUtil.WindowsClipboardService.SetTextAsync(_cursorPos.Text);
            ScreenNotification.ShowNotification("Copied!");
        }

        private void BuildDisplay() {
            _dataPanel?.Dispose();
            _dataPanel = new DataPanel() {
                Parent = Graphics.SpriteScreen,
                Size = new Point(Graphics.SpriteScreen.Width, Graphics.SpriteScreen.Height),
                Location = new Point(0,0),
                ZIndex = -9999
            };

            GetCurrentMap(Gw2Mumble.CurrentMap.Id);
            GetCurrentElite(Gw2Mumble.PlayerCharacter.Specialization);
        }

        private void OnMapChanged(object o, ValueEventArgs<int> e) => GetCurrentMap(e.Value);
        private void OnSpecializationChanged(object o, ValueEventArgs<int> e) => GetCurrentElite(e.Value);

        private async void GetCurrentMap(int id) {
            await Gw2ApiManager.Gw2ApiClient.V2.Maps.GetAsync(id)
                    .ContinueWith(response => {
                        if (response.Exception != null || response.IsFaulted || response.IsCanceled) return;
                        var result = response.Result;
                        if (_dataPanel == null) return;
                        CurrentMap = result;
                    });
        }

        private async void GetCurrentElite(int id) {
            await Gw2ApiManager.Gw2ApiClient.V2.Specializations.GetAsync(id)
                    .ContinueWith(response => {
                        if (response.Exception != null || response.IsFaulted || response.IsCanceled) return;
                        var result = response.Result;
                        if (_dataPanel == null) return;
                        CurrentSpec = result;
                    });
        }

        /// <inheritdoc />
        protected override void Unload() {
            _mockService?.Dispose();
            _dataPanel?.Dispose();
            _cursorPos?.Dispose();
            _ramCounter?.Dispose();
            _cpuCounter?.Dispose();
            _toggleInfoBinding.Value.Activated -= OnToggleInfoBindingActivated;
            Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
            Gw2Mumble.PlayerCharacter.SpecializationChanged -= OnSpecializationChanged;
            GameIntegration.Gw2Instance.Gw2Closed -= OnGw2Closed;
            GameIntegration.Gw2Instance.Gw2Started -= OnGw2Started;
            // All static members must be manually unset
            Instance = null;
        }
    }
}

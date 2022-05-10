﻿using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Nekres.Loading_Screen_Hints.Controls;
using Nekres.Loading_Screen_Hints.Controls.Hints;

namespace Nekres.Loading_Screen_Hints
{

    [Export(typeof(Module))]
    public class LoadingScreenHintsModule : Module {

        internal static readonly Logger Logger = Logger.GetLogger(typeof(LoadingScreenHintsModule));

        internal static LoadingScreenHintsModule Instance;

        // Service Managers
        internal SettingsManager    SettingsManager    => this.ModuleParameters.SettingsManager;
        internal ContentsManager    ContentsManager    => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager      Gw2ApiManager      => this.ModuleParameters.Gw2ApiManager;

        // Settings
        private SettingEntry<HashSet<int>[]> SeenHints;

        // Controls
        private LoadScreenPanel LoadScreenPanel;
        private bool Created;

        // Shuffle
        private HashSet<int> ShuffledHints;
        private HashSet<int> SeenGamingTips;
        private HashSet<int> SeenNarrations;
        private HashSet<int> SeenGuessCharacters;

        private Random Randomize;

        [ImportingConstructor]
        public LoadingScreenHintsModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) {
            Instance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            var selfManagedSettings = settings.AddSubCollection("selfManaged", false, false);
            SeenHints = selfManagedSettings.DefineSetting("seenHints", new HashSet<int>[3]);
        }

        protected override void Initialize() {
            this.Randomize = new Random();
            this.ShuffledHints = new HashSet<int>();
            this.SeenGamingTips = SeenHints.Value[0] ?? new HashSet<int>();
            this.SeenNarrations = SeenHints.Value[1] ?? new HashSet<int>();
            this.SeenGuessCharacters = SeenHints.Value[2] ?? new HashSet<int>();
        }

        protected override async Task LoadAsync()
        {
             /* NOOP */
        }

        protected override void OnModuleLoaded(EventArgs e) {
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime) {

            if (!GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                if (!this.Created)
                {
                    NextHint(); 
                    this.Created = true;
                }
            }
            else
            {
                if (LoadScreenPanel != null && LoadScreenPanel.Fade == null) {
                    if (LoadScreenPanel.Opacity == 0.0f) {
                        LoadScreenPanel.Dispose();
                        LoadScreenPanel = null;
                    } else {
                        LoadScreenPanel.FadeOut();
                    }
                }
                Created = false;
            }
        }

        protected override void Unload() {
            Instance = null;
            LoadScreenPanel?.Dispose();
        }

        private void Save() {
            SeenHints.Value = new []{ SeenGamingTips, SeenNarrations, SeenGuessCharacters };
        }

        public void NextHint()
        {
            LoadScreenPanel?.Dispose();

            int total = 3;
            int count = ShuffledHints.Count;
            if (count >= total) { ShuffledHints.Clear(); count = 0; }
            var range = Enumerable.Range(1, total).Where(i => !ShuffledHints.Contains(i));
            int index = Randomize.Next(0, total - count - 1);
            int hint = range.ElementAt(index);

            ShuffledHints.Add(hint);

            LoadScreenPanel = new LoadScreenPanel
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(600, 200),
                Location = new Point((GameService.Graphics.SpriteScreen.Width / 2 - 300), (GameService.Graphics.SpriteScreen.Height / 2 - 100) + 300)
            };

            switch (hint)
            {
                case 1:

                    total = GamingTip.Tips.Count;
                    count = SeenGamingTips.Count;
                    if (count >= total) { SeenGamingTips.Clear(); count = 0; }
                    range = Enumerable.Range(0, total).Where(i => !SeenGamingTips.Contains(i));
                    index = Randomize.Next(0, total - count);
                    hint = range.ElementAt(index);

                    SeenGamingTips.Add(hint);
                    LoadScreenPanel.LoadScreenTip = new GamingTip(hint) { Parent = LoadScreenPanel, Size = LoadScreenPanel.Size, Location = new Point(0, 0) };

                    break;

                case 2:

                    total = Narration.Narratives.Count;
                    count = SeenNarrations.Count;
                    if (count >= total) { SeenNarrations.Clear(); count = 0; }
                    range = Enumerable.Range(0, total).Where(i => !SeenNarrations.Contains(i));
                    index = Randomize.Next(0, total - count);
                    hint = range.ElementAt(index);

                    SeenNarrations.Add(hint);
                    LoadScreenPanel.LoadScreenTip = new Narration(hint) { Parent = LoadScreenPanel, Size = LoadScreenPanel.Size, Location = new Point(0, 0) };

                    break;

                case 3:

                    total = GuessCharacter.Characters.Count;
                    count = SeenGuessCharacters.Count;
                    if (count >= total) { SeenGuessCharacters.Clear(); count = 0; }
                    range = Enumerable.Range(0, total).Where(i => !SeenGuessCharacters.Contains(i));
                    index = Randomize.Next(0, total - count);
                    hint = range.ElementAt(index);

                    SeenGuessCharacters.Add(hint);
                    LoadScreenPanel.LoadScreenTip = new GuessCharacter(hint, LoadScreenPanel) { Location = new Point(0, 0) };

                    break;

                default:
                    throw new NotSupportedException();
            }
            this.Save();
        }
    }
}

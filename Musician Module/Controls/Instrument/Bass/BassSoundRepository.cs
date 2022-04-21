using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace Nekres.Musician_Module.Controls.Instrument
{
    public class BassSoundRepository : IDisposable
    {
        private readonly Dictionary<string, string> _map = new()
        {
            // Low Octave
            {$"{GuildWarsControls.WeaponSkill1}{FluteNote.Octaves.Low}", "C1"},
            {$"{GuildWarsControls.WeaponSkill2}{FluteNote.Octaves.Low}", "D1"},
            {$"{GuildWarsControls.WeaponSkill3}{FluteNote.Octaves.Low}", "E1"},
            {$"{GuildWarsControls.WeaponSkill4}{FluteNote.Octaves.Low}", "F1"},
            {$"{GuildWarsControls.WeaponSkill5}{FluteNote.Octaves.Low}", "G1"},
            {$"{GuildWarsControls.HealingSkill}{FluteNote.Octaves.Low}", "A1"},
            {$"{GuildWarsControls.UtilitySkill1}{FluteNote.Octaves.Low}", "B1"},
            {$"{GuildWarsControls.UtilitySkill2}{FluteNote.Octaves.Low}", "C2"},
            // High Octave
            {$"{GuildWarsControls.WeaponSkill1}{FluteNote.Octaves.High}", "C2"},
            {$"{GuildWarsControls.WeaponSkill2}{FluteNote.Octaves.High}", "D2"},
            {$"{GuildWarsControls.WeaponSkill3}{FluteNote.Octaves.High}", "E2"},
            {$"{GuildWarsControls.WeaponSkill4}{FluteNote.Octaves.High}", "F2"},
            {$"{GuildWarsControls.WeaponSkill5}{FluteNote.Octaves.High}", "G2"},
            {$"{GuildWarsControls.HealingSkill}{FluteNote.Octaves.High}", "A2"},
            {$"{GuildWarsControls.UtilitySkill1}{FluteNote.Octaves.High}", "B2"},
            {$"{GuildWarsControls.UtilitySkill2}{FluteNote.Octaves.High}", "C3"}
        };

        private readonly Dictionary<string, SoundEffectInstance> _sound = new()
        {
            {"C1", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\C1.wav").CreateInstance()},
            {"D1", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\D1.wav").CreateInstance()},
            {"E1", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\E1.wav").CreateInstance()},
            {"F1", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\F1.wav").CreateInstance()},
            {"G1", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\G1.wav").CreateInstance()},
            {"A1", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\A1.wav").CreateInstance()},
            {"B1", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\B1.wav").CreateInstance()},
            {"C2", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\C2.wav").CreateInstance()},
            {"D2", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\D2.wav").CreateInstance()},
            {"E2", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\E2.wav").CreateInstance()},
            {"F2", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\F2.wav").CreateInstance()},
            {"G2", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\G2.wav").CreateInstance()},
            {"A2", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\A2.wav").CreateInstance()},
            {"B2", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\B2.wav").CreateInstance()},
            {"C3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bass\C3.wav").CreateInstance()}

        };

        public SoundEffectInstance Get(string id)
        {
            return _sound[id];
        }

        public SoundEffectInstance Get(GuildWarsControls key, BassNote.Octaves octave)
        {
            return _sound[_map[$"{key}{octave}"]];
        }

        public void Dispose() {
            _map?.Clear();
            foreach (var snd in _sound)
                snd.Value?.Dispose();
        }
    }
}
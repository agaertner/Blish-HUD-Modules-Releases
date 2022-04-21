using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace Nekres.Musician_Module.Controls.Instrument
{
    public class FluteSoundRepository : IDisposable
    {
        private readonly Dictionary<string, string> _map = new()
        {
            // Low Octave
            {$"{GuildWarsControls.WeaponSkill1}{FluteNote.Octaves.Low}", "E4"},
            {$"{GuildWarsControls.WeaponSkill2}{FluteNote.Octaves.Low}", "F4"},
            {$"{GuildWarsControls.WeaponSkill3}{FluteNote.Octaves.Low}", "G4"},
            {$"{GuildWarsControls.WeaponSkill4}{FluteNote.Octaves.Low}", "A4"},
            {$"{GuildWarsControls.WeaponSkill5}{FluteNote.Octaves.Low}", "B4"},
            {$"{GuildWarsControls.HealingSkill}{FluteNote.Octaves.Low}", "C5"},
            {$"{GuildWarsControls.UtilitySkill1}{FluteNote.Octaves.Low}", "D5"},
            {$"{GuildWarsControls.UtilitySkill2}{FluteNote.Octaves.Low}", "E5"},
            //{$"{GuildWarsControls.UtilitySkill3}{FluteNote.Octaves.Low}", "Increase Octave"},
            //{$"{GuildWarsControls.EliteSkill}{FluteNote.Octaves.Low}", "Stop Playing"},

            // High Octave
            {$"{GuildWarsControls.WeaponSkill1}{FluteNote.Octaves.High}", "E5"},
            {$"{GuildWarsControls.WeaponSkill2}{FluteNote.Octaves.High}", "F5"},
            {$"{GuildWarsControls.WeaponSkill3}{FluteNote.Octaves.High}", "G5"},
            {$"{GuildWarsControls.WeaponSkill4}{FluteNote.Octaves.High}", "A5"},
            {$"{GuildWarsControls.WeaponSkill5}{FluteNote.Octaves.High}", "B5"},
            {$"{GuildWarsControls.HealingSkill}{FluteNote.Octaves.High}", "C6"},
            {$"{GuildWarsControls.UtilitySkill1}{FluteNote.Octaves.High}", "D6"},
            {$"{GuildWarsControls.UtilitySkill2}{FluteNote.Octaves.High}", "E6"}
            //{$"{GuildWarsControls.UtilitySkill3}{FluteNote.Octaves.Low}", "Decrease Octave"},
            //{$"{GuildWarsControls.EliteSkill}{FluteNote.Octaves.Low}", "Stop Playing"},
        };

        private readonly Dictionary<string, SoundEffectInstance> _sound = new()
        {
            {"E4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\E4.wav").CreateInstance()},
            {"F4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\F4.wav").CreateInstance()},
            {"G4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\G4.wav").CreateInstance()},
            {"A4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\A4.wav").CreateInstance()},
            {"B4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\B4.wav").CreateInstance()},
            {"C5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\C5.wav").CreateInstance()},
            {"D5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\D5.wav").CreateInstance()},
            {"E5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\E5.wav").CreateInstance()},
            {"F5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\F5.wav").CreateInstance()},
            {"G5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\G5.wav").CreateInstance()},
            {"A5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\A5.wav").CreateInstance()},
            {"B5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\B5.wav").CreateInstance()},
            {"C6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\C6.wav").CreateInstance()},
            {"D6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\D6.wav").CreateInstance()},
            {"E6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Flute\E6.wav").CreateInstance()}

        };

        public SoundEffectInstance Get(string id)
        {
            return _sound[id];
        }

        public SoundEffectInstance Get(GuildWarsControls key, FluteNote.Octaves octave)
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
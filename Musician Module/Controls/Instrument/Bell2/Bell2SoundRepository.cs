using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace Nekres.Musician_Module.Controls.Instrument
{
    public class Bell2SoundRepository : IDisposable
    {
        private readonly Dictionary<string, string> _map = new()
        {
            // Low Octave
            {$"{GuildWarsControls.WeaponSkill1}{Bell2Note.Octaves.Low}", "C5"},
            {$"{GuildWarsControls.WeaponSkill2}{Bell2Note.Octaves.Low}", "D5"},
            {$"{GuildWarsControls.WeaponSkill3}{Bell2Note.Octaves.Low}", "E5"},
            {$"{GuildWarsControls.WeaponSkill4}{Bell2Note.Octaves.Low}", "F5"},
            {$"{GuildWarsControls.WeaponSkill5}{Bell2Note.Octaves.Low}", "G5"},
            {$"{GuildWarsControls.HealingSkill}{Bell2Note.Octaves.Low}", "A5"},
            {$"{GuildWarsControls.UtilitySkill1}{Bell2Note.Octaves.Low}", "B5"},
            {$"{GuildWarsControls.UtilitySkill2}{Bell2Note.Octaves.Low}", "C6"},
            // High Octave
            {$"{GuildWarsControls.WeaponSkill1}{Bell2Note.Octaves.High}", "C6"},
            {$"{GuildWarsControls.WeaponSkill2}{Bell2Note.Octaves.High}", "D6"},
            {$"{GuildWarsControls.WeaponSkill3}{Bell2Note.Octaves.High}", "E6"},
            {$"{GuildWarsControls.WeaponSkill4}{Bell2Note.Octaves.High}", "F6"},
            {$"{GuildWarsControls.WeaponSkill5}{Bell2Note.Octaves.High}", "G6"},
            {$"{GuildWarsControls.HealingSkill}{Bell2Note.Octaves.High}", "A6"},
            {$"{GuildWarsControls.UtilitySkill1}{Bell2Note.Octaves.High}", "B6"},
            {$"{GuildWarsControls.UtilitySkill2}{Bell2Note.Octaves.High}", "C7"}
        };

        private readonly Dictionary<string, SoundEffectInstance> _sound = new()
        {
            {"C5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\C5.wav").CreateInstance()},
            {"D5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\D5.wav").CreateInstance()},
            {"E5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\E5.wav").CreateInstance()},
            {"F5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\F5.wav").CreateInstance()},
            {"G5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\G5.wav").CreateInstance()},
            {"A5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\A5.wav").CreateInstance()},
            {"B5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\B5.wav").CreateInstance()},
            {"C6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\C6.wav").CreateInstance()},
            {"D6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\D6.wav").CreateInstance()},
            {"E6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\E6.wav").CreateInstance()},
            {"F6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\F6.wav").CreateInstance()},
            {"G6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\G6.wav").CreateInstance()},
            {"A6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\A6.wav").CreateInstance()},
            {"B6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\B6.wav").CreateInstance()},
            {"C7", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell2\C7.wav").CreateInstance()}
        };

        public SoundEffectInstance Get(string id)
        {
            return _sound[id];
        }

        public SoundEffectInstance Get(GuildWarsControls key, Bell2Note.Octaves octave)
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
using System;
using Blish_HUD.Controls.Intern;

namespace Nekres.Musician.Core.Instrument
{
    public interface IInstrumentPreview : IDisposable
    {
        void PlaySoundByKey(GuildWarsControls key);
    }
}
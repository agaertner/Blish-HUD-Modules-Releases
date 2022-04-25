using SQLite;
using System;
using Nekres.Musician.Core.Models;

namespace Nekres.Musician.UI.Models
{
    internal class MusicSheetModel
    {
        [PrimaryKey, AutoIncrement]
        public int InternalId { get; set; }

        public Guid Id { get; set; }

        public string Artist { get; set; }

        public string Title { get; set; }

        public string User { get; set; }

        public Instrument Instrument { get; set; }

        public string Tempo { get; set; }

        public Algorithm Algorithm { get; set; }

        [MaxLength(1000)]
        public string Melody { get; set; }
    }
}

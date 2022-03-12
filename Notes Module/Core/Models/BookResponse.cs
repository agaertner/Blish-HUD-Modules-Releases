using System;
using System.Collections.Generic;
namespace Nekres.Notes.Core.Models
{
    internal class BookResponse
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public IEnumerable<(string, string)> Pages { get; set; }

        public string Author { get; set; }

        public Guid AuthorId { get; set; }

        public int PositiveRatings { get; set; }

        public int NegativeRatings { get; set; }

        public int Views { get; set; }

        public class PageResponse
        {
            public string Title { get; set; }

            public int Index { get; set; }

            public string Content { get; set; }
        }
    }
}

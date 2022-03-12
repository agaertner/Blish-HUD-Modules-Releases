using System;
using Nekres.Notes.Core.Models;

namespace Nekres.Notes.UI.Models
{
    internal class OnlineBookModel : BookModel
    {
        public string Author { get; set; }

        public Guid AuthorId { get; set; }

        public int PositiveRatings { get; set; }

        public int NegativeRatings { get; set; }

        public int Views { get; set; }

        public static OnlineBookModel FromResponse(BookResponse response)
        {
            return new OnlineBookModel
            {
                Id = response.Id,
                Title = response.Title,
                Pages = response.Pages,
                Author = response.Author,
                AuthorId = response.AuthorId,
                PositiveRatings = response.PositiveRatings,
                NegativeRatings = response.NegativeRatings,
                Views = response.Views
            };
        }
    }
}

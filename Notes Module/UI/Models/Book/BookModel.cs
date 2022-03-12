using System;
using System.Collections.Generic;
using System.Linq;
using Nekres.Notes.UI.Controls;
using Newtonsoft.Json;
namespace Nekres.Notes.UI.Models
{
    internal class BookModel
    {
        [JsonProperty("id")] public Guid Id { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("pages")] public IList<PageModel> Pages { get; set; }

        public static BookModel FromControl(BookBase book)
        {
            return new BookModel
            {
                Id = book.Guid,
                Title = book.Title,
                Pages = book.Pages.Select(p => new PageModel(p)).ToList()
            };
        }
    }
}

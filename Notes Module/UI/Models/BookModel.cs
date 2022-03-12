using System;
using System.Collections.Generic;
using System.Linq;
using Nekres.Notes.UI.Controls;
using Newtonsoft.Json;
namespace Nekres.Notes.UI.Models
{
    internal class BookModel
    {
        public BookModel(){}

        internal BookModel(BookBase book)
        {
            Id = book.Guid;

            Title = book.Title;

            Pages = book.Pages.Select(p => new PageModel(p)).ToList();
        }

        [JsonProperty("id")] public Guid Id { get; set; }

        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("pages")] public IList<PageModel> Pages { get; set; }
    }
}

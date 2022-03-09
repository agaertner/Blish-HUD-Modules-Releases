using System;
using System.Collections.Generic;
using System.Linq;
using Nekres.Notes.UI.Controls;
using Newtonsoft.Json;
namespace Nekres.Notes.UI.Models
{
    public class BookModel
    {
        public BookModel(){}

        public BookModel(Book book)
        {
            Guid = book.Guid;
            AllowEdit = book.AllowEdit;
            Title = book.BookTitle;
            UseChapters = book.UseChapters;
            Pages = book.Pages.Select(p => new PageModel(p)).ToList();
        }

        [JsonProperty("id")] public Guid Guid { get; set; }
        [JsonProperty("useChapters")] public bool UseChapters { get; set; }
        [JsonProperty("allowEdit")] public bool AllowEdit { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("pages")] public IList<PageModel> Pages { get; set; }
    }

}

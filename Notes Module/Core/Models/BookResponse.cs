using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Nekres.Notes.Core.Models
{
    internal sealed class BookResponse
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("pages")]
        public IEnumerable<PageResponse> Pages { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("authorId")]
        public Guid AuthorId { get; set; }

        [JsonProperty("byArenaNet")]
        public bool ByArenaNet { get; set; }

        [JsonProperty("positiveRatings")]
        public int PositiveRatings { get; set; }

        [JsonProperty("negativeRatings")]
        public int NegativeRatings { get; set; }

        [JsonProperty("views")]
        public int Views { get; set; }
    }
}

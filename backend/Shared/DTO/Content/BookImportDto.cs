using System;
using System.Collections.Generic;

namespace Shared.DTO.Content
{
    public class BookImportDto
    {
        public string ExternalID { get; set; }
        public string ExternalSource { get; set; } = "GoogleBooks";
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public double AverageRating { get; set; }
        public string PosterURL { get; set; }

        public string Author { get; set; }
        public int Pages { get; set; }
        public string ISBN { get; set; }
        public string Publisher { get; set; }
        public string BookSeries { get; set; }

        public List<string> Genres { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Languages { get; set; } = new List<string>();
    }
}

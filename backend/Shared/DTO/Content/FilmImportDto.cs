using System;
using System.Collections.Generic;

namespace Shared.DTO.Content
{
    public class FilmImportDto
    {
        public string ExternalID { get; set; }
        public string ExternalSource { get; set; } = "TMDB";
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public double AverageRating { get; set; }
        public string PosterURL { get; set; }

        public string Director { get; set; }
        public int DurationMinutes { get; set; }
        public decimal BoxOffice { get; set; }

        public List<string> Genres { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Languages { get; set; } = new List<string>();
    }

    public class SeriesImportDto
    {
        public string ExternalID { get; set; }
        public string ExternalSource { get; set; } = "TMDB";
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public double AverageRating { get; set; }
        public string PosterURL { get; set; }

        public int SeasonCount { get; set; }
        public int EpisodesCount { get; set; }
        public string Status { get; set; }
        public string Network { get; set; }

        public List<string> Genres { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Languages { get; set; } = new List<string>();
    }
}

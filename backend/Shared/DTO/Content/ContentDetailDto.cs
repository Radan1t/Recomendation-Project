namespace Shared.DTO.Content
{
    public class ContentDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ContentType { get; set; }

        // common
        public string Genres { get; set; }
        public string ReleaseDate { get; set; }
        public string PosterUrl { get; set; }
        public double AverageRating { get; set; }
        public string Pegi { get; set; }

        // games
        public string Developer { get; set; }
        public string Publisher { get; set; }

        // films
        public string Director { get; set; }
        public string Cast { get; set; }
        public int DurationMinutes { get; set; }
        public string Country { get; set; }

        // series
        public string Creator { get; set; }
        public int SeasonCount { get; set; }
        public int EpisodesCount { get; set; }
        public string Status { get; set; }
        public string Network { get; set; }

        // books
        public string Author { get; set; }
        public string PublisherName { get; set; }
        public int Pages { get; set; }
        public string ISBN { get; set; }

        public string SteamRatingText { get; set; }
    }
}

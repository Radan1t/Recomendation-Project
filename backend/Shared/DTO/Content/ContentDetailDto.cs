namespace Shared.DTO.Content
{
    public class ContentDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Developer { get; set; }
        public string Genres { get; set; }
        public string ReleaseDate { get; set; }
        public string Pegi { get; set; }
        public string PosterUrl { get; set; }
        
        public string SteamRatingText { get; set; }
    }
}

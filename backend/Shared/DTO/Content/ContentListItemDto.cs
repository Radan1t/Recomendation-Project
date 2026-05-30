using System;

namespace Shared.DTO.Content
{
    public class ContentListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }
}

namespace Shared.DTO.Content;
public class ContentDto
{
    public int ContentId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Genre { get; set; }
    public DateTime ReleaseDate { get; set; }
    public double AvarageRating { get; set; }
    public string ContentType { get; set; }
    public string PosterUrl { get; set; }
}

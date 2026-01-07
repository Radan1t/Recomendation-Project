namespace Shared.DTO.Content;
public class FilmDto : ContentDto
{
    public string Director { get; set; }
    public List<string> Cast { get; set; }
    public int DurationMinutes { get; set; }
    public string Country { get; set; }
    public string AgeRating { get; set; }
}
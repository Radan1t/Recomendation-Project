namespace Shared.DTO.Content;
public class FilmDto : ContentDto
{
    public string Director { get; set; }
    public List<string> Cast { get; set; }
    public int DurationMinutes { get; set; }
    public string Country { get; set; }
    public string AgeRating { get; set; }
}

public class GenreDto
{
    public int GenreID { get; set; }
    public string Name { get; set; }
}

public class LanguageDto
{
    public int LanguageID { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
}

namespace Shared.DTO.Content;
public class GameDto: ContentDto
{
    public string Developer { get; set; }
    public string Platform { get; set; }
    public string PEGIRating { get; set; }
    public bool Multiplayer { get; set; }
}
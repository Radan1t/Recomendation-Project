namespace Shared.DTO.Content;
public class SeriesDto : ContentDto
{
    public string Creator { get; set; }
    public int SeasonsCount { get; set; }
    public int EpisodesCount { get; set; }
    public string Status { get; set; } 
    public string Network { get; set; }
}

namespace Shared.DTO.Recommendation;
public class RecommendedItemDto
{
    public int ContentId { get; set; }
    public string Title { get; set; }
    public int Rank { get; set; }
    public double RelevanceScore { get; set; }
}

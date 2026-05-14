using System.Text.Json.Serialization;

namespace Shared.DTO.Recommendation;

public class RecommendedItemDto
{
    
    [JsonPropertyName("id")]
    public int ContentId { get; set; }

    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    
    [JsonPropertyName("posterUrl")]
    public string? PosterUrl { get; set; }

    
    [JsonPropertyName("rank")]
    public int Rank { get; set; }

    [JsonPropertyName("score")]
    public double RelevanceScore { get; set; }
}

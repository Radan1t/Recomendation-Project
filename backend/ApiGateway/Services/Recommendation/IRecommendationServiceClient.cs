using Shared.DTO.Recommendation;

namespace ApiGateway.Services.Recommendation;

public interface IRecommendationServiceClient
{
    
    Task<List<RecommendedItemDto>> GetSimilarAsync(int contentId);
    
    
    Task<object> GenerateAsync(int userId);

    
    Task<List<RecommendedItemDto>> GetAsync(RecommendationRequestDto dto);
}

using Shared.DTO.Recommendation;

namespace ApiGateway.Services.Recommendation;

public interface IRecommendationServiceClient
{
    Task<List<RecommendedItemDto>> GetAsync(
        RecommendationRequestDto dto);
}

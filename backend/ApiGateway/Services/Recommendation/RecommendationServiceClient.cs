using ApiGateway.Config;
using Microsoft.Extensions.Options;
using Shared.DTO.Recommendation;

namespace ApiGateway.Services.Recommendation;

public class RecommendationServiceClient
    : IRecommendationServiceClient
{
    private readonly HttpClient _http;
    private readonly ServiceUrls _urls;

    public RecommendationServiceClient(HttpClient http, IOptions<ServiceUrls> urls)
    {
        _http = http;
        _urls = urls.Value;
    }

    public async Task<List<RecommendedItemDto>> GetAsync(
        RecommendationRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync(
            $"{_urls.RecommendationService}/recommend", dto);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<RecommendedItemDto>>();
    }
}

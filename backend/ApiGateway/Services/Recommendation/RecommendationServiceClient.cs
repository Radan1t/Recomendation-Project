using ApiGateway.Config;
using Microsoft.Extensions.Options;
using Shared.DTO.Recommendation;
using System.Net.Http.Json;

namespace ApiGateway.Services.Recommendation;

public class RecommendationServiceClient : IRecommendationServiceClient
{
    private readonly HttpClient _http;
    private readonly ServiceUrls _urls;

    public RecommendationServiceClient(HttpClient http, IOptions<ServiceUrls> urls)
    {
        _http = http;
        _urls = urls.Value;
    }

    
    public async Task<List<RecommendedItemDto>> GetSimilarAsync(int contentId)
    {
        
        var response = await _http.GetAsync($"{_urls.RecommendationService}/api/v1/recommendations/{contentId}");
        
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<RecommendedItemDto>>() 
               ?? new List<RecommendedItemDto>();
    }

    
    public async Task<object> GenerateAsync(int userId)
    {
        
        var response = await _http.PostAsync($"{_urls.RecommendationService}/api/v1/recommendations/generate/{userId}", null);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<object>();
    }

    
    public async Task<List<RecommendedItemDto>> GetAsync(RecommendationRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{_urls.RecommendationService}/recommend", dto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<RecommendedItemDto>>() 
               ?? new List<RecommendedItemDto>();
    }
}

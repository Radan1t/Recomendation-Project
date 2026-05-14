using ApiGateway.Config;
using Microsoft.Extensions.Options;
using Shared.DTO.Content;

namespace ApiGateway.Services.Content;

public class ContentServiceClient : IContentServiceClient
{
    private readonly HttpClient _http;
    private readonly ServiceUrls _urls;

    public ContentServiceClient(HttpClient http, IOptions<ServiceUrls> urls)
    {
        _http = http;
        _urls = urls.Value;
    }

    public async Task<List<ContentDto>> GetAllAsync()
    {
        return await _http.GetFromJsonAsync<List<ContentDto>>(
            $"{_urls.ContentService}/content");
    }
}


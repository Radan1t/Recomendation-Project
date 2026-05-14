using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/v1/interactions")]
[Authorize]
public class InteractionGatewayController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly string _interactionUrl;
    private readonly ILogger<InteractionGatewayController> _logger;

    public InteractionGatewayController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<InteractionGatewayController> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        var baseUrl = configuration["ServiceUrls:InteractionService"] ?? "http://localhost:5004";
        _interactionUrl = $"{baseUrl}/api/v1/interactions";
    }

    
    [HttpPost("favorite")]
    public async Task<IActionResult> ProxyFavorite([FromBody] JsonElement data)
    {
        _logger.LogInformation(">>>> GATEWAY: Отримано запит FAVORITE <<<<");
        return await ForwardToInteraction("favorite", data);
    }

    
    [HttpPost("rate")]
    public async Task<IActionResult> ProxyRate([FromBody] JsonElement data)
    {
        _logger.LogInformation(">>>> GATEWAY: Отримано запит RATE <<<<");
        return await ForwardToInteraction("rate", data);
    }

    [HttpGet("status/{contentId}")]
    public async Task<IActionResult> ProxyGetStatus(int contentId)
    {
        _logger.LogInformation("--- GATEWAY GET STATUS: {ContentId} ---", contentId);
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_interactionUrl}/status/{contentId}");
        AddUserIdHeader(request);
        var response = await _httpClient.SendAsync(request);
        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private async Task<IActionResult> ForwardToInteraction(string subPath, JsonElement data)
    {
        var targetUrl = $"{_interactionUrl}/{subPath}";
        _logger.LogInformation("Forwarding POST to: {TargetUrl}", targetUrl);

        var request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
        request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        AddUserIdHeader(request);

        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("InteractionService returned: {StatusCode}", response.StatusCode);
        return StatusCode((int)response.StatusCode, result);
    }

    private void AddUserIdHeader(HttpRequestMessage request)
    {
        var userId = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId)) request.Headers.Add("X-User-Id", userId);
    }
}

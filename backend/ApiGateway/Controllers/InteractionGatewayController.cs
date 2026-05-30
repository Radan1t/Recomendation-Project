using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/v1/interactions")]
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
    [Authorize]
    public async Task<IActionResult> ProxyFavorite([FromBody] JsonElement data)
    {
        _logger.LogInformation(">>>> GATEWAY: Отримано запит FAVORITE <<<<");
        return await ForwardToInteraction("favorite", data);
    }

    
    [HttpPost("rate")]
    [Authorize]
    public async Task<IActionResult> ProxyRate([FromBody] JsonElement data)
    {
        _logger.LogInformation(">>>> GATEWAY: Отримано запит RATE <<<<");
        return await ForwardToInteraction("rate", data);
    }

    [HttpGet("content/{contentId}/average")]
    [Authorize]
    public async Task<IActionResult> ProxyGetContentAverage(int contentId)
    {
        _logger.LogInformation("--- GATEWAY GET CONTENT AVERAGE: {ContentId} ---", contentId);
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_interactionUrl}/content/{contentId}/average");
        AddUserIdHeader(request);
        var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        return new ContentResult { Content = body, ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json", StatusCode = (int)response.StatusCode };
    }

    [HttpGet("status/{contentId}")]
    [Authorize]
    public async Task<IActionResult> ProxyGetStatus(int contentId)
    {
        _logger.LogInformation("--- GATEWAY GET STATUS: {ContentId} ---", contentId);
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_interactionUrl}/status/{contentId}");
        AddUserIdHeader(request);
        var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        return new ContentResult { Content = body, ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json", StatusCode = (int)response.StatusCode };
    }

    [HttpGet("user/ratings")]
    [Authorize]
    public async Task<IActionResult> ProxyGetUserRatings()
    {
        _logger.LogInformation("--- GATEWAY GET USER RATINGS ---");
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_interactionUrl}/user/ratings");
        AddUserIdHeader(request);
        var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        return new ContentResult { Content = body, ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json", StatusCode = (int)response.StatusCode };
    }

    [HttpGet("user/favorites")]
    [Authorize]
    public async Task<IActionResult> ProxyGetUserFavorites()
    {
        _logger.LogInformation("--- GATEWAY GET USER FAVORITES ---");
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_interactionUrl}/user/favorites");
        AddUserIdHeader(request);
        var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        return new ContentResult { Content = body, ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json", StatusCode = (int)response.StatusCode };
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
        return new ContentResult { Content = result, ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json", StatusCode = (int)response.StatusCode };
    }

    private void AddUserIdHeader(HttpRequestMessage request)
    {
        // If the incoming HTTP request already included X-User-Id (from client localStorage), forward it.
        if (Request.Headers.TryGetValue("X-User-Id", out var incomingUserId) && !string.IsNullOrEmpty(incomingUserId))
        {
            request.Headers.Add("X-User-Id", incomingUserId.ToString());
            return;
        }

        // Otherwise, try to extract user id from the authenticated user principal
        var userId = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId)) request.Headers.Add("X-User-Id", userId);
    }
}

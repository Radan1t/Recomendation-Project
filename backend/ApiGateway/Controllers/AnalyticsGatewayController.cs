using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/v1/analytics")]
    public class AnalyticsGatewayController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _interactionUrl;

        public AnalyticsGatewayController(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _httpClient = clientFactory.CreateClient();
            
            _interactionUrl = config["Services:Interaction"]; 
        }

        
        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity()
        {
            var response = await _httpClient.GetAsync($"{_interactionUrl}/api/v1/analytics/activity");
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, content);
            }
            return Content(content, "application/json");
        }

        
        [HttpGet("genres-distribution")]
        public async Task<IActionResult> GetGenresDistribution()
        {
            var response = await _httpClient.GetAsync($"{_interactionUrl}/api/v1/analytics/genres-distribution");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiGateway.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize] 
    public class RecommendationsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public RecommendationsController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();
            _config = config;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateRecommendations()
        {
            
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { message = "Недійсний токен: UserID не знайдено" });
            }

            
            
            var pythonServiceUrl = _config["Services:Recommendation"]; 

            try
            {
                
                var targetUrl = $"{pythonServiceUrl}/api/v1/recommendations/generate/{userIdString}";
                var response = await _httpClient.PostAsync(targetUrl, null);
                
                
                var data = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Content(data, "application/json"); 
                }
                
                return StatusCode((int)response.StatusCode, data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Помилка зв'язку з сервісом рекомендацій", details = ex.Message });
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/v1/auth")] 
    public class AuthGatewayController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthGatewayController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] object registrationData)
        {
            
            var userServiceUrl = _configuration["ServiceUrls:UserService"];
            
            if (string.IsNullOrEmpty(userServiceUrl))
            {
                return StatusCode(500, "URL для UserService не знайдено в конфігурації.");
            }

            try
            {
                
                
                var response = await _httpClient.PostAsJsonAsync($"{userServiceUrl}/api/v1/auth/register", registrationData);
                
                var data = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Content(data, "application/json");
                }
                
                return StatusCode((int)response.StatusCode, data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Помилка з'єднання з UserService", details = ex.Message });
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] object loginData)
        {
            var userServiceUrl = _configuration["ServiceUrls:UserService"];
            if (string.IsNullOrEmpty(userServiceUrl)) return StatusCode(500, "URL не знайдено");

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{userServiceUrl}/api/v1/auth/login", loginData);
                var data = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode) return Content(data, "application/json");
                return StatusCode((int)response.StatusCode, data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Помилка Gateway", details = ex.Message });
            }
        }
    }
}

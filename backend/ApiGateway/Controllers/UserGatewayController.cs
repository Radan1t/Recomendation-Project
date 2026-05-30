using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/v1/user")]
    public class UserGatewayController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public UserGatewayController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var userServiceUrl = _configuration["ServiceUrls:UserService"];
            if (string.IsNullOrEmpty(userServiceUrl)) return StatusCode(500, "UserService URL not configured");

            try
            {
                var req = new HttpRequestMessage(HttpMethod.Get, $"{userServiceUrl}/api/v1/user/profile/{id}");
                if (Request.Headers.ContainsKey("Authorization")) req.Headers.TryAddWithoutValidation("Authorization", Request.Headers["Authorization"].ToString());
                var response = await _httpClient.SendAsync(req);
                var data = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) return Content(data, "application/json");
                return StatusCode((int)response.StatusCode, data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Gateway error", details = ex.Message });
            }
        }

        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] object dto)
        {
            var userServiceUrl = _configuration["ServiceUrls:UserService"];
            if (string.IsNullOrEmpty(userServiceUrl)) return StatusCode(500, "UserService URL not configured");

            try
            {
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var req = new HttpRequestMessage(HttpMethod.Put, $"{userServiceUrl}/api/v1/user/profile/{id}") { Content = content };
                if (Request.Headers.ContainsKey("Authorization")) req.Headers.TryAddWithoutValidation("Authorization", Request.Headers["Authorization"].ToString());
                var response = await _httpClient.SendAsync(req);
                var data = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) return Content(data, "application/json");
                return StatusCode((int)response.StatusCode, data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Gateway error", details = ex.Message });
            }
        }
    }
}

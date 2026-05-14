using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    
    [ApiController]
    [Route("api/v1")] 
    public class ImportGatewayController : ControllerBase
    {
        private readonly HttpClient _httpClient;


        private readonly string _contentServiceUrl = "http://content-service:8080";

        public ImportGatewayController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("Games/import-from-rawg")]
        public async Task<IActionResult> ImportGames([FromQuery] int page = 1)
        {
            var response = await _httpClient.PostAsync($"{_contentServiceUrl}/api/v1/Games/import-from-rawg?page={page}", null);
            var content = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, content);
        }

        [HttpPost("TmdbImport/import-movies")]
        public async Task<IActionResult> ImportMovies([FromQuery] int page = 1)
        {
            var response = await _httpClient.PostAsync($"{_contentServiceUrl}/api/v1/TmdbImport/import-movies?page={page}", null);
            var content = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, content);
        }

        [HttpPost("TmdbImport/import-series")]
        public async Task<IActionResult> ImportSeries([FromQuery] int page = 1)
        {
            var response = await _httpClient.PostAsync($"{_contentServiceUrl}/api/v1/TmdbImport/import-series?page={page}", null);
            var content = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, content);
        }

        [HttpPost("BooksImport/import")]
        public async Task<IActionResult> ImportBooks([FromQuery] string subject = "fiction", [FromQuery] int startIndex = 0)
        {
            var response = await _httpClient.PostAsync($"{_contentServiceUrl}/api/v1/BooksImport/import?subject={subject}&startIndex={startIndex}", null);
            var content = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, content);
        }
    }
}

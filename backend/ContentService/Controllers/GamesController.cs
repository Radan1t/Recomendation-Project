using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ContentService.Services;

namespace ContentService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly RawgIntegrationService _rawgIntegrationService;

        public GamesController(RawgIntegrationService rawgIntegrationService)
        {
            _rawgIntegrationService = rawgIntegrationService;
        }

        [HttpPost("import-from-rawg")]
        public async Task<IActionResult> ImportFromRawg([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                await _rawgIntegrationService.ImportGamesFromRawgAsync(page, pageSize);
                return Ok(new { message = $"Успішно імпортовано ігри зі сторінки {page}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Помилка імпорту", details = ex.Message });
            }
        }
    }
}

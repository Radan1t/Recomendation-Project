using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ContentService.Services;
using System;
using Microsoft.Extensions.Logging;

namespace ContentService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BooksImportController : ControllerBase
    {
        private readonly GoogleBooksIntegrationService _booksIntegrationService;
        private readonly ILogger<BooksImportController> _logger;

        public BooksImportController(GoogleBooksIntegrationService booksIntegrationService, ILogger<BooksImportController> logger)
        {
            _booksIntegrationService = booksIntegrationService;
            _logger = logger;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportBooks([FromQuery] string subject = "fiction", [FromQuery] int startIndex = 0)
        {
            try
            {
                _logger.LogInformation($"[BooksImportController] Запит на імпорт книг. Subject: {subject}, StartIndex: {startIndex}");
                var results = await _booksIntegrationService.ImportBooksAsync(subject, startIndex, 20);

                _logger.LogInformation("[BooksImportController] Імпорт завершено. Повертаю результати");

                return Ok(new { message = $"Книги жанру '{subject}' оброблено (починаючи з індексу {startIndex}).", results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BooksImportController] КРИТИЧНА ПОМИЛКА при імпорті книг");
                return StatusCode(500, new { message = "Помилка при імпорті книг", details = ex.Message });
            }
        }
    }
}
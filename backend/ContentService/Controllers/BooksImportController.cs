using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ContentService.Services;
using System;

namespace ContentService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BooksImportController : ControllerBase
    {
        private readonly GoogleBooksIntegrationService _booksIntegrationService;

        public BooksImportController(GoogleBooksIntegrationService booksIntegrationService)
        {
            _booksIntegrationService = booksIntegrationService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportBooks([FromQuery] string subject = "fiction", [FromQuery] int startIndex = 0)
        {
            try
            {
                
                await _booksIntegrationService.ImportBooksAsync(subject, startIndex, 20);
                return Ok(new { message = $"Книги жанру '{subject}' успішно імпортовано (починаючи з індексу {startIndex})." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Помилка при імпорті книг", details = ex.Message });
            }
        }
    }
}

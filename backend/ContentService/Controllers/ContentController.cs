using Microsoft.AspNetCore.Mvc;
using Shared.DTO.Content;
using ContentService.Services;

namespace ContentService.Controllers;

[ApiController]
[Route("api/v1/content")]
public class ContentController : ControllerBase
{
    private readonly IContentService _service;
    private readonly ILogger<ContentController> _logger; 

    public ContentController(IContentService service, ILogger<ContentController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("random-posters")]
    public async Task<ActionResult<List<string>>> GetRandomPosters([FromQuery] int count = 6)
    {
        _logger.LogInformation($"[ContentService] Запит random-posters. Count: {count}");
        try
        {
            var result = await _service.GetRandomPostersAsync(count);
            _logger.LogInformation($"[ContentService] Успішно повернуто {result.Count} постерів");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ContentService] КРИТИЧНА ПОМИЛКА БАЗИ ДАНИХ у GetRandomPosters");
            return StatusCode(500, $"Помилка БД: {ex.Message} | Inner: {ex.InnerException?.Message}");
        }
    }

    [HttpGet("genres")]
    public async Task<ActionResult<List<GenreDto>>> GetGenres()
    {
        _logger.LogInformation("[ContentService] Запит genres");
        try
        {
            var result = await _service.GetGenresAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ContentService] КРИТИЧНА ПОМИЛКА БАЗИ ДАНИХ у GetGenres");
            return StatusCode(500, $"Помилка БД: {ex.Message}");
        }
    }

    [HttpGet("languages")]
    public async Task<ActionResult<List<LanguageDto>>> GetLanguages()
    {
        _logger.LogInformation("[ContentService] Запит languages");
        try
        {
            var result = await _service.GetLanguagesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ContentService] КРИТИЧНА ПОМИЛКА БАЗИ ДАНИХ у GetLanguages");
            return StatusCode(500, $"Помилка БД: {ex.Message}");
        }
    }

    [HttpGet("details")]
    public async Task<IActionResult> GetContentDetails([FromQuery] int[] ids)
    {
        _logger.LogInformation($"[ContentService] Запит details. IDs count: {ids?.Length}");
        if (ids == null || ids.Length == 0) return BadRequest("Список ID порожній");

        try
        {
            var details = await _service.GetContentDetailsAsync(ids);
            return Ok(details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ContentService] КРИТИЧНА ПОМИЛКА БАЗИ ДАНИХ у GetContentDetails");
            return StatusCode(500, $"Помилка БД: {ex.Message}");
        }
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSingleContent(int id)
    {
        _logger.LogInformation($"[ContentService] Запит на детальну інформацію для ID: {id}");
        
        try
        {
            
            var content = await _service.GetSingleContentAsync(id);
            
            if (content == null)
            {
                _logger.LogWarning($"[ContentService] Контент з ID {id} не знайдено в базі.");
                return NotFound(new { message = $"Контент з ID {id} не знайдено." });
            }

            return Ok(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[ContentService] КРИТИЧНА ПОМИЛКА БАЗИ ДАНИХ при отриманні ID {id}");
            return StatusCode(500, $"Помилка БД: {ex.Message}");
        }
    }
    [HttpPost("genres-batch")]
    public async Task<IActionResult> GetGenresBatch([FromBody] List<int> contentIds)
    {
        _logger.LogInformation($"[ContentService] Запит genres-batch. Кількість ID: {contentIds?.Count ?? 0}");
        
        if (contentIds == null || !contentIds.Any()) 
            return BadRequest("Список ID порожній");

        try
        {
            
            var result = await _service.GetGenresBatchAsync(contentIds);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ContentService] КРИТИЧНА ПОМИЛКА БАЗИ ДАНИХ у GetGenresBatch");
            return StatusCode(500, $"Помилка БД: {ex.Message}");
        }
    }
    [HttpGet("admin/list")]
        public async Task<IActionResult> GetAdminList([FromQuery] string type)
{
    try
    {
        var result = await _service.GetContentListForAdminAsync(type);
        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "[ContentService] Помилка отримання списку для адмінки.");
        return StatusCode(500, ex.Message);
    }
}

        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> DeleteContent(int id)
        {
            try
            {
                var success = await _service.DeleteContentAsync(id);
                if (!success) return NotFound(new { message = $"Контент з ID {id} не знайдено." });

                return Ok(new { message = "Успішно видалено." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ContentService] Помилка видалення ID {id}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("admin/{id}")]
        public async Task<IActionResult> UpdateContent(int id, [FromBody] ContentUpdateDto dto)
        {
            try
            {
                var success = await _service.UpdateContentAsync(id, dto);
                if (!success) return NotFound(new { message = $"Контент з ID {id} не знайдено." });

                return Ok(new { message = "Успішно оновлено." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ContentService] Помилка оновлення ID {id}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("search")]
    public async Task<IActionResult> SearchGlobalContent([FromQuery] string q)
    {
        _logger.LogInformation($"[ContentService] Запит глобального пошуку. Текст: '{q}'");
        
        if (string.IsNullOrWhiteSpace(q)) 
            return Ok(new List<object>()); 

        try
        {
            var results = await _service.SearchGlobalContentAsync(q);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ContentService] КРИТИЧНА ПОМИЛКА БАЗИ ДАНИХ у SearchGlobalContent");
            return StatusCode(500, $"Помилка БД: {ex.Message}");
        }
    }
}

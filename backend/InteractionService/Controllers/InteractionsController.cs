using Microsoft.AspNetCore.Mvc;
using InteractionService.Services;
using Shared.DTO.Interactions;

namespace InteractionService.Controllers;

[ApiController]

[Route("api/v1/interactions")] 
public class InteractionsController : ControllerBase
{
    private readonly IInteractionService _interactionService;
    private readonly ILogger<InteractionsController> _logger;

    public InteractionsController(IInteractionService interactionService, ILogger<InteractionsController> logger)
    {
        _interactionService = interactionService;
        _logger = logger;
    }

    [HttpPost("rate")]
    public async Task<IActionResult> Rate([FromBody] RatingRequestDto dto)
    {
        _logger.LogInformation("--- INTERACTION SERVICE: Rate called for ContentId={ContentId} ---", dto.ContentId);
        
        var userId = GetUserId();
        _logger.LogInformation("UserId from header: {UserId}", userId);

        if (userId == 0) return Unauthorized();

        var success = await _interactionService.RateContentAsync(userId, dto);
        return success ? Ok(new { message = "Оцінка збережена" }) : BadRequest("Помилка при збереженні");
    }

    [HttpPost("favorite")]
    public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteRequestDto dto)
    {
        _logger.LogInformation("--- INTERACTION SERVICE: Favorite called for ContentId={ContentId} ---", dto.ContentId);
        
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var isFavorite = await _interactionService.ToggleFavoriteAsync(userId, dto.ContentId);
        return Ok(new { isFavorite });
    }

    [HttpGet("status/{contentId}")]
    public async Task<IActionResult> GetStatus(int contentId)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var status = await _interactionService.GetStatusAsync(userId, contentId);
        return Ok(status);
    }

    private int GetUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdStr))
        {
            if (int.TryParse(userIdStr, out var userId)) return userId;
        }
        return 0;
    }
}

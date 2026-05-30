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

    [HttpGet("content/{contentId}/average")]
    public async Task<IActionResult> GetContentAverage(int contentId)
    {
        try
        {
            var avg = await _interactionService.GetContentAverageAsync(contentId);
            return Ok(avg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while calculating content average for {ContentId}", contentId);
            return StatusCode(500, "Error calculating average");
        }
    }

    [HttpGet("user/ratings")]
    public async Task<IActionResult> GetUserRatings()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var ratings = await _interactionService.GetUserRatingsAsync(userId);
        return Ok(ratings);
    }

    [HttpGet("user/favorites")]
    public async Task<IActionResult> GetUserFavorites()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var favorites = await _interactionService.GetUserFavoritesAsync(userId);
        return Ok(favorites);
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

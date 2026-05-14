using Microsoft.EntityFrameworkCore;
using InteractionService.Data;
using InteractionService.Entities;
using Shared.DTO.Interactions;
using Microsoft.Extensions.Logging;

namespace InteractionService.Services;

public class InteractionServiceImplementation : IInteractionService
{
    private readonly InteractionDbContext _context;
    private readonly ILogger<InteractionServiceImplementation> _logger;

    public InteractionServiceImplementation(InteractionDbContext context, ILogger<InteractionServiceImplementation> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> RateContentAsync(int userId, RatingRequestDto dto)
    {
        var rating = await _context.UserRatings.FindAsync(userId, dto.ContentId);

        if (rating != null)
        {
            rating.Score = dto.Score;
            rating.DateRated = DateTime.UtcNow;
        }
        else
        {
            _context.UserRatings.Add(new UserRating
            {
                UserID = userId,
                ContentID = dto.ContentId,
                Score = dto.Score,
                DateRated = DateTime.UtcNow
            });
        }
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ToggleFavoriteAsync(int userId, int contentId)
    {
        var favorite = await _context.UserFavorites.FindAsync(userId, contentId);

        if (favorite != null)
        {
            _context.UserFavorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return false; 
        }

        _context.UserFavorites.Add(new UserFavorite
        {
            UserID = userId,
            ContentID = contentId,
            DateAdded = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true; 
    }

    public async Task<InteractionStatusDto> GetStatusAsync(int userId, int contentId)
    {
        var isFavorite = await _context.UserFavorites
            .AnyAsync(f => f.UserID == userId && f.ContentID == contentId);

        var rating = await _context.UserRatings
            .FirstOrDefaultAsync(r => r.UserID == userId && r.ContentID == contentId);

        return new InteractionStatusDto
        {
            ContentId = contentId,
            IsFavorite = isFavorite,
            UserScore = rating?.Score
        };
    }
}

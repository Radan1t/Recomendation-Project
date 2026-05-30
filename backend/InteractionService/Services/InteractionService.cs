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

    public async Task<List<UserRatingDto>> GetUserRatingsAsync(int userId)
    {
        var ratings = await _context.UserRatings
            .Where(r => r.UserID == userId)
            .OrderByDescending(r => r.DateRated)
            .Select(r => new UserRatingDto
            {
                ContentId = r.ContentID,
                Score = r.Score,
                DateRated = r.DateRated
            })
            .ToListAsync();

        return ratings;
    }

    public async Task<List<UserFavoriteDto>> GetUserFavoritesAsync(int userId)
    {
        var favorites = await _context.UserFavorites
            .Where(f => f.UserID == userId)
            .OrderByDescending(f => f.DateAdded)
            .Select(f => new UserFavoriteDto
            {
                ContentId = f.ContentID,
                DateAdded = f.DateAdded
            })
            .ToListAsync();

        return favorites;
    }

    public async Task<Shared.DTO.Interactions.ContentAverageDto> GetContentAverageAsync(int contentId)
    {
        var stats = await _context.UserRatings
            .Where(r => r.ContentID == contentId)
            .GroupBy(r => 1)
            .Select(g => new { Avg = g.Average(r => r.Score), Count = g.Count() })
            .FirstOrDefaultAsync();

        if (stats == null) return new Shared.DTO.Interactions.ContentAverageDto { Average = 0.0, Count = 0 };

        return new Shared.DTO.Interactions.ContentAverageDto { Average = Math.Round(stats.Avg, 2), Count = stats.Count };
    }
}

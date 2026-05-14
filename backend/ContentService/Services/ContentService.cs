using ContentService.Data;
using ContentService.Entities; 

using Microsoft.EntityFrameworkCore;
using Shared.DTO.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentService.Services;

public class ContentService : IContentService
{
    private readonly ContentDbContext _context;

    public ContentService(ContentDbContext context)
    {
        _context = context;
    }



    public async Task<List<string>> GetRandomPostersAsync(int count)
    {
        return await _context.Contents
            .AsNoTracking() 
            .Where(c => !string.IsNullOrEmpty(c.PosterURL))
            .OrderBy(c => EF.Functions.Random()) 
            .Take(count)
            .Select(c => c.PosterURL)
            .ToListAsync();
    }

    public async Task<List<GenreDto>> GetGenresAsync()
    {
        return await _context.Genres
            .Select(g => new GenreDto 
            { 
                GenreID = g.GenreID, 
                Name = g.Name 
            })
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<List<LanguageDto>> GetLanguagesAsync()
    {
        return await _context.Languages
            .Select(l => new LanguageDto 
            { 
                LanguageID = l.LanguageID, 
                Code = l.Code,
                Name = l.Name 
            })
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<List<object>> GetContentDetailsAsync(int[] ids)
    {
        
        var contents = await _context.Contents
            .AsNoTracking()
            .Where(c => ids.Contains(c.ContentID))
            .ToListAsync();

        
        return contents.Select(c => (object)new
        {
            ContentID = c.ContentID,
            Title = c.Title,
            PosterUrl = c.PosterURL, 
            
            ContentType = c.GetType().Name.EndsWith("Entity") 
                          ? c.GetType().Name.Replace("Entity", "s") 
                          : c.GetType().Name + "s", 
            Rating = c.AverageRating
        }).ToList();
    }

    
    
    
    public async Task<ContentDetailDto?> GetSingleContentAsync(int id)
    {
        
        string searchId = id.ToString();

        
        var content = await _context.Contents
            .AsNoTracking()
            .Include(c => c.ContentGenres)
                .ThenInclude(cg => cg.Genre)
            .FirstOrDefaultAsync(c => c.ContentID == id || c.ExternalID == searchId);

        if (content == null) return null;

        
        
        int actualInternalId = content.ContentID;

        
        string developer = "Невідомо";
        string pegi = "0+";

        
        var game = await _context.Games.AsNoTracking().FirstOrDefaultAsync(g => g.ContentID == actualInternalId);
        if (game != null)
        {
            developer = game.Developer;
            pegi = game.PEGIRating;
        }
        else 
        {
            
            var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.ContentID == actualInternalId);
            if (book != null) developer = book.Author;
        }

        return new ContentDetailDto
        {
            Id = actualInternalId, 
            Title = content.Title,
            Description = content.Description ?? "Опис відсутній",
            Developer = developer,
            Pegi = pegi,
            
            Genres = content.ContentGenres != null && content.ContentGenres.Any()
                ? string.Join(", ", content.ContentGenres.Select(cg => cg.Genre.Name))
                : "Не вказано",

            ReleaseDate = content.ReleaseDate.ToString("dd MMM yyyy"),
            PosterUrl = content.PosterURL,
            SteamRatingText = content.AverageRating > 0 ? $"Рейтинг: {content.AverageRating:F1}" : "Немає оцінок"
        };
    }
    

    public async Task<Dictionary<int, List<string>>> GetGenresBatchAsync(List<int> contentIds)
    {
        var genresMap = await _context.ContentGenres
            .Include(cg => cg.Genre)
            .Where(cg => contentIds.Contains(cg.ContentID))
            .GroupBy(cg => cg.ContentID)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(x => x.Genre.Name).ToList()
            );

        return genresMap;
    }

    public async Task<List<object>> GetContentListForAdminAsync(string contentType)
{
    if (string.IsNullOrWhiteSpace(contentType)) return new List<object>();

    var type = contentType.ToLower().Trim();

    if (type == "games")
    {
        return await _context.Games
            .OrderByDescending(g => g.ContentID)
            .Select(g => new {
                Id = g.ContentID,
                Title = g.Title,
                Developer = g.Developer,
                Publisher = g.Publisher,
                AverageRating = g.AverageRating,
                ReleaseDate = g.ReleaseDate
            }).Cast<object>().ToListAsync();
    }
    else if (type == "movies")
    {
        return await _context.Films
            .OrderByDescending(f => f.ContentID)
            .Select(f => new {
                Id = f.ContentID,
                Title = f.Title,
                Director = f.Director,
                DurationMinutes = f.DurationMinutes,
                AverageRating = f.AverageRating,
                ReleaseDate = f.ReleaseDate
            }).Cast<object>().ToListAsync();
    }
    else if (type == "series")
    {
        return await _context.Series
            .OrderByDescending(s => s.ContentID)
            .Select(s => new {
                Id = s.ContentID,
                Title = s.Title,
                SeasonCount = s.SeasonCount,
                Status = s.Status,
                AverageRating = s.AverageRating,
                ReleaseDate = s.ReleaseDate
            }).Cast<object>().ToListAsync();
    }
    else if (type == "books")
    {
        return await _context.Books
            .OrderByDescending(b => b.ContentID)
            .Select(b => new {
                Id = b.ContentID,
                Title = b.Title,
                Author = b.Author,
                Pages = b.Pages,
                AverageRating = b.AverageRating,
                ReleaseDate = b.ReleaseDate
            }).Cast<object>().ToListAsync();
    }

    return new List<object>();
}

    public async Task<bool> DeleteContentAsync(int id)
    {
        var content = await _context.Contents.FindAsync(id);
        if (content == null) return false;

        _context.Contents.Remove(content);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateContentAsync(int id, ContentUpdateDto dto)
    {
        var content = await _context.Contents.FindAsync(id);
        if (content == null) return false;

        content.Title = dto.Title;
        content.Description = dto.Description;

        _context.Contents.Update(content);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<object>> SearchGlobalContentAsync(string query)
    {
        var q = query.ToLower().Trim();

        
        var games = await _context.Games
            .Where(x => x.Title.ToLower().Contains(q))
            .Take(5)
            .Select(x => new {
                Id = x.ContentID, 
                ContentID = x.ExternalID ?? x.ContentID.ToString(),
                Title = x.Title,
                PosterUrl = x.PosterURL,
                ReleaseDate = x.ReleaseDate,
                ContentType = "Games"
            }).ToListAsync();

        
        var films = await _context.Films
            .Where(x => x.Title.ToLower().Contains(q))
            .Take(5)
            .Select(x => new {
                Id = x.ContentID, 
                ContentID = x.ExternalID ?? x.ContentID.ToString(),
                Title = x.Title,
                PosterUrl = x.PosterURL,
                ReleaseDate = x.ReleaseDate,
                ContentType = "Films"
            }).ToListAsync();

        
        var series = await _context.Series
            .Where(x => x.Title.ToLower().Contains(q))
            .Take(5)
            .Select(x => new {
                Id = x.ContentID, 
                ContentID = x.ExternalID ?? x.ContentID.ToString(),
                Title = x.Title,
                PosterUrl = x.PosterURL,
                ReleaseDate = x.ReleaseDate,
                ContentType = "Series"
            }).ToListAsync();

        
        var books = await _context.Books
            .Where(x => x.Title.ToLower().Contains(q))
            .Take(5)
            .Select(x => new {
                Id = x.ContentID, 
                ContentID = x.ExternalID ?? x.ContentID.ToString(),
                Title = x.Title,
                PosterUrl = x.PosterURL,
                ReleaseDate = x.ReleaseDate,
                ContentType = "Books"
            }).ToListAsync();

        
        var combinedResults = games.Cast<object>()
            .Concat(films)
            .Concat(series)
            .Concat(books)
            .ToList();

        return combinedResults;
    }
}

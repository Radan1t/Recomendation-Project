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

    public async Task<List<GenreDto>> GetGenresAsync(string? contentType = null)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return await _context.Genres
                .Select(g => new GenreDto { GenreID = g.GenreID, Name = g.Name })
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        var type = contentType.ToLower().Trim();

        IQueryable<int> contentIds = null;
        if (type == "games") contentIds = _context.Games.Select(g => g.ContentID);
        else if (type == "movies" || type == "films") contentIds = _context.Films.Select(f => f.ContentID);
        else if (type == "series") contentIds = _context.Series.Select(s => s.ContentID);
        else if (type == "books") contentIds = _context.Books.Select(b => b.ContentID);

        if (contentIds == null) return new List<GenreDto>();

        var genres = await _context.ContentGenres
            .Include(cg => cg.Genre)
            .Where(cg => contentIds.Contains(cg.ContentID))
            .Select(cg => cg.Genre)
            .Distinct()
            .Select(g => new GenreDto { GenreID = g.GenreID, Name = g.Name })
            .OrderBy(g => g.Name)
            .ToListAsync();

        return genres;
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

        var film = await _context.Films.AsNoTracking().FirstOrDefaultAsync(f => f.ContentID == actualInternalId);
        if (film != null)
        {
            // map film-specific fields
            return new ContentDetailDto
            {
                Id = actualInternalId,
                Title = content.Title,
                Description = content.Description ?? "Опис відсутній",
                ContentType = "Film",
                Director = film.Director,
                DurationMinutes = film.DurationMinutes,
                Country = null,
                Pegi = pegi,
                Genres = content.ContentGenres != null && content.ContentGenres.Any() ? string.Join(", ", content.ContentGenres.Select(cg => cg.Genre.Name)) : "Не вказано",
                ReleaseDate = content.ReleaseDate.ToString("dd MMM yyyy"),
                PosterUrl = content.PosterURL,
                AverageRating = content.AverageRating,
                SteamRatingText = content.AverageRating > 0 ? $"Рейтинг: {content.AverageRating:F1}" : "Немає оцінок"
            };
        }

        var series = await _context.Series.AsNoTracking().FirstOrDefaultAsync(s => s.ContentID == actualInternalId);
        if (series != null)
        {
            return new ContentDetailDto
            {
                Id = actualInternalId,
                Title = content.Title,
                Description = content.Description ?? "Опис відсутній",
                ContentType = "Series",
                Creator = series.Network,
                SeasonCount = series.SeasonCount,
                EpisodesCount = series.EpisodesCount,
                Status = series.Status,
                Network = series.Network,
                Genres = content.ContentGenres != null && content.ContentGenres.Any() ? string.Join(", ", content.ContentGenres.Select(cg => cg.Genre.Name)) : "Не вказано",
                ReleaseDate = content.ReleaseDate.ToString("dd MMM yyyy"),
                PosterUrl = content.PosterURL,
                AverageRating = content.AverageRating,
                SteamRatingText = content.AverageRating > 0 ? $"Рейтинг: {content.AverageRating:F1}" : "Немає оцінок"
            };
        }

        var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.ContentID == actualInternalId);
        if (book != null)
        {
            developer = book.Author;
            return new ContentDetailDto
            {
                Id = actualInternalId,
                Title = content.Title,
                Description = content.Description ?? "Опис відсутній",
                ContentType = "Book",
                Author = book.Author,
                PublisherName = book.Publisher,
                Pages = book.Pages,
                ISBN = book.ISBN,
                Genres = content.ContentGenres != null && content.ContentGenres.Any() ? string.Join(", ", content.ContentGenres.Select(cg => cg.Genre.Name)) : "Не вказано",
                ReleaseDate = content.ReleaseDate.ToString("dd MMM yyyy"),
                PosterUrl = content.PosterURL,
                AverageRating = content.AverageRating,
                SteamRatingText = content.AverageRating > 0 ? $"Рейтинг: {content.AverageRating:F1}" : "Немає оцінок"
            };
        }

        // default / game or fallback
        return new ContentDetailDto
        {
            Id = actualInternalId,
            Title = content.Title,
            Description = content.Description ?? "Опис відсутній",
            ContentType = game != null ? "Game" : "Unknown",
            Developer = developer,
            Pegi = pegi,
            Genres = content.ContentGenres != null && content.ContentGenres.Any() ? string.Join(", ", content.ContentGenres.Select(cg => cg.Genre.Name)) : "Не вказано",
            ReleaseDate = content.ReleaseDate.ToString("dd MMM yyyy"),
            PosterUrl = content.PosterURL,
            AverageRating = content.AverageRating,
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

public async Task<Shared.DTO.Content.ContentListResultDto> GetContentListAsync(string contentType, string? q = null, int? genreId = null, int page = 1, int pageSize = 20)
{
    if (string.IsNullOrWhiteSpace(contentType)) return new Shared.DTO.Content.ContentListResultDto();

    var type = contentType.ToLower().Trim();
    q = string.IsNullOrWhiteSpace(q) ? null : q.ToLower().Trim();

    if (type == "all")
    {
        var gamesQ = _context.Games.AsNoTracking().AsQueryable();
        var filmsQ = _context.Films.AsNoTracking().AsQueryable();
        var seriesQ = _context.Series.AsNoTracking().AsQueryable();
        var booksQ = _context.Books.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            gamesQ = gamesQ.Where(x => x.Title.ToLower().Contains(q));
            filmsQ = filmsQ.Where(x => x.Title.ToLower().Contains(q));
            seriesQ = seriesQ.Where(x => x.Title.ToLower().Contains(q));
            booksQ = booksQ.Where(x => x.Title.ToLower().Contains(q));
        }

        if (genreId.HasValue)
        {
            gamesQ = gamesQ.Where(g => _context.ContentGenres.Any(cg => cg.ContentID == g.ContentID && cg.GenreID == genreId.Value));
            filmsQ = filmsQ.Where(f => _context.ContentGenres.Any(cg => cg.ContentID == f.ContentID && cg.GenreID == genreId.Value));
            seriesQ = seriesQ.Where(s => _context.ContentGenres.Any(cg => cg.ContentID == s.ContentID && cg.GenreID == genreId.Value));
            booksQ = booksQ.Where(b => _context.ContentGenres.Any(cg => cg.ContentID == b.ContentID && cg.GenreID == genreId.Value));
        }

        var selectGames = gamesQ.Select(x => new Shared.DTO.Content.ContentListItemDto { Id = x.ContentID, Title = x.Title, PosterUrl = x.PosterURL, AverageRating = x.AverageRating, ContentType = "Games" });
        var selectFilms = filmsQ.Select(x => new Shared.DTO.Content.ContentListItemDto { Id = x.ContentID, Title = x.Title, PosterUrl = x.PosterURL, AverageRating = x.AverageRating, ContentType = "Films" });
        var selectSeries = seriesQ.Select(x => new Shared.DTO.Content.ContentListItemDto { Id = x.ContentID, Title = x.Title, PosterUrl = x.PosterURL, AverageRating = x.AverageRating, ContentType = "Series" });
        var selectBooks = booksQ.Select(x => new Shared.DTO.Content.ContentListItemDto { Id = x.ContentID, Title = x.Title, PosterUrl = x.PosterURL, AverageRating = x.AverageRating, ContentType = "Books" });

        var combined = selectGames.Cast<Shared.DTO.Content.ContentListItemDto>()
            .Concat(selectFilms)
            .Concat(selectSeries)
            .Concat(selectBooks);

        var total = await combined.CountAsync();
        var items = await combined.OrderBy(x => x.Title).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new Shared.DTO.Content.ContentListResultDto { Items = items, Total = total };
    }

    if (type == "games")
    {
        var qg = _context.Games.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) qg = qg.Where(x => x.Title.ToLower().Contains(q));
        if (genreId.HasValue) qg = qg.Where(g => _context.ContentGenres.Any(cg => cg.ContentID == g.ContentID && cg.GenreID == genreId.Value));
        var projected = qg.Select(x => new Shared.DTO.Content.ContentListItemDto { Id = x.ContentID, Title = x.Title, PosterUrl = x.PosterURL, AverageRating = x.AverageRating, ContentType = "Games" });
        var totalCount = await projected.CountAsync();
        var itemsList = await projected.OrderBy(x => x.Title).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new Shared.DTO.Content.ContentListResultDto { Items = itemsList, Total = totalCount };
    }
    else if (type == "movies" || type == "films")
    {
        var qf = _context.Films.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) qf = qf.Where(x => x.Title.ToLower().Contains(q));
        if (genreId.HasValue) qf = qf.Where(f => _context.ContentGenres.Any(cg => cg.ContentID == f.ContentID && cg.GenreID == genreId.Value));
        var projected = qf.Select(x => new Shared.DTO.Content.ContentListItemDto { Id = x.ContentID, Title = x.Title, PosterUrl = x.PosterURL, AverageRating = x.AverageRating, ContentType = "Films" });
        var totalCount = await projected.CountAsync();
        var itemsList = await projected.OrderBy(x => x.Title).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new Shared.DTO.Content.ContentListResultDto { Items = itemsList, Total = totalCount };
    }
    else if (type == "series")
    {
        var qs = _context.Series.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) qs = qs.Where(x => x.Title.ToLower().Contains(q));
        if (genreId.HasValue) qs = qs.Where(s => _context.ContentGenres.Any(cg => cg.ContentID == s.ContentID && cg.GenreID == genreId.Value));
        var projected = qs.Select(x => new Shared.DTO.Content.ContentListItemDto { Id = x.ContentID, Title = x.Title, PosterUrl = x.PosterURL, AverageRating = x.AverageRating, ContentType = "Series" });
        var totalCount = await projected.CountAsync();
        var itemsList = await projected.OrderBy(x => x.Title).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new Shared.DTO.Content.ContentListResultDto { Items = itemsList, Total = totalCount };
    }
    else if (type == "books")
    {
        var qb = _context.Books.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) qb = qb.Where(x => x.Title.ToLower().Contains(q));
        if (genreId.HasValue) qb = qb.Where(b => _context.ContentGenres.Any(cg => cg.ContentID == b.ContentID && cg.GenreID == genreId.Value));
        var projected = qb.Select(x => new Shared.DTO.Content.ContentListItemDto { Id = x.ContentID, Title = x.Title, PosterUrl = x.PosterURL, AverageRating = x.AverageRating, ContentType = "Books" });
        var totalCount = await projected.CountAsync();
        var itemsList = await projected.OrderBy(x => x.Title).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new Shared.DTO.Content.ContentListResultDto { Items = itemsList, Total = totalCount };
    }

    return new Shared.DTO.Content.ContentListResultDto();
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

    public async Task<List<GenreDto>> GetBookGenresAsync()
    {
        var bookIds = await _context.Books.Select(b => b.ContentID).ToListAsync();
        
        var genres = await _context.ContentGenres
            .Include(cg => cg.Genre)
            .Where(cg => bookIds.Contains(cg.ContentID))
            .Select(cg => cg.Genre)
            .Distinct()
            .Select(g => new GenreDto { GenreID = g.GenreID, Name = g.Name })
            .OrderBy(g => g.Name)
            .ToListAsync();

        return genres;
    }
}

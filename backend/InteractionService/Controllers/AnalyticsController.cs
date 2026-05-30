using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InteractionService.Data;
using Shared.DTO.Interactions;


namespace InteractionService.Controllers
{
    [ApiController]
    [Route("api/v1/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly InteractionDbContext _context;
        private readonly HttpClient _httpClient;

        public AnalyticsController(InteractionDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        
        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity()
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-30);

                var ratingsRaw = await _context.UserRatings
                    .Where(r => r.DateRated >= startDate)
                    .Select(r => new { r.DateRated })
                    .ToListAsync();

                var favoritesRaw = await _context.UserFavorites
                    .Where(f => f.DateAdded >= startDate)
                    .Select(f => new { f.DateAdded })
                    .ToListAsync();

                var ratings = ratingsRaw
                    .GroupBy(r => r.DateRated.Date)
                    .Select(g => new { date = g.Key.ToString("o"), count = g.Count(), type = "Оцінки" })
                    .ToList();

                var favorites = favoritesRaw
                    .GroupBy(f => f.DateAdded.Date)
                    .Select(g => new { date = g.Key.ToString("o"), count = g.Count(), type = "Обране" })
                    .ToList();

                var combined = ratings.Concat(favorites)
                                      .OrderBy(x => x.date)
                                      .ToList();

                return Ok(combined);
            }
            catch (Exception ex)
            {
                // log and return safe response
                Console.Error.WriteLine(ex);
                return StatusCode(500, "Error building activity dynamics");
            }
        }

        [HttpGet("genres-distribution")]
public async Task<IActionResult> GetGenreStats()
{
    try 
    {
        
        
        var ratingsRaw = await _context.UserRatings
            .Where(r => r.Score >= 4)
            .Select(r => new { r.ContentID }) 
            .ToListAsync();

        if (!ratingsRaw.Any()) return Ok(new List<object>());

        
        var topItems = ratingsRaw
            .GroupBy(r => r.ContentID)
            .Select(g => new { Id = g.Key, Weight = g.Count() })
            .ToList();

        var ids = topItems.Select(x => x.Id).ToList();



                var response = await _httpClient.PostAsJsonAsync("http://content-service:8080/api/v1/content/genres-batch", ids);

                if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, $"ContentService помилка: {errorText}");
        }
            
        var genreMapping = await response.Content.ReadFromJsonAsync<Dictionary<int, List<string>>>();

        
        var stats = new Dictionary<string, int>();
        foreach (var item in topItems)
        {
            if (genreMapping != null && genreMapping.TryGetValue(item.Id, out var genres))
            {
                foreach (var g in genres)
                {
                    if (!stats.ContainsKey(g)) stats[g] = 0;
                    stats[g] += item.Weight;
                }
            }
        }

        var result = stats.Select(s => new { name = s.Key, value = s.Value })
                          .OrderByDescending(x => x.value)
                          .ToList();

        return Ok(result);
    }
    catch (Exception ex)
    {
        
        return StatusCode(500, ex.ToString());
    }
}
    }
}

using ContentService.Providers;
using Shared.DTO.Content;
using System.Text.Json;

namespace ContentService.Providers;

public class SteamContentProvider : IExternalContentProvider
{
    private readonly HttpClient _httpClient;

    public SteamContentProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Реалізація методу інтерфейсу
    public async Task<List<ContentDto>> GetContentsAsync()
    {
        var games = await GetPopularGamesAsync();
        // Конвертуємо GameDto → ContentDto
        return games.Select(g => new ContentDto
        {
            ContentId = g.ContentId,
            Title = g.Title,
            Description = g.Description,
            Genre = g.Genre,
            ReleaseDate = g.ReleaseDate,
            AvarageRating = g.AvarageRating,
            ContentType = g.ContentType,
            PosterUrl = g.PosterUrl,
 
        }).ToList();
    }
    private static readonly Random _random = new Random();

    // Приватний метод для отримання ігор
private async Task<List<GameDto>> GetPopularGamesAsync()
{
    var popularIds = new List<int> { 570, 730, 440, 1790930 }; // Dota 2, CS:GO, Team Fortress 2, Crisol
    var tasks = popularIds.Select(GetGame);
    var results = await Task.WhenAll(tasks);
    return results.Where(x => x != null).ToList()!;
}



    private async Task<GameDto?> GetGame(int appId)
    {
    var url = $"https://store.steampowered.com/api/appdetails?appids={appId}";
    var response = await _httpClient.GetAsync(url);

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"AppId {appId} failed: HTTP {response.StatusCode}");
        return null;
    }

    var json = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement.GetProperty(appId.ToString());

    if (!root.GetProperty("success").GetBoolean())
    {
        Console.WriteLine($"AppId {appId} failed: API success=false");
        return null;
    }

    var data = root.GetProperty("data");

        DateTime releaseDate = DateTime.Now;
        if (data.TryGetProperty("release_date", out var rd) &&
            rd.TryGetProperty("date", out var dateStr))
        {
            DateTime.TryParse(dateStr.GetString(), out releaseDate);
        }

        return new GameDto
        {
            ContentId = appId,
            Title = data.GetProperty("name").GetString(),
            Description = data.GetProperty("short_description").GetString(),
            Genre = data.TryGetProperty("genres", out var genres)
                ? genres[0].GetProperty("description").GetString()
                : "Unknown",
            ReleaseDate = releaseDate,
            AvarageRating = 0,
            ContentType = "Game",
            PosterUrl = data.GetProperty("header_image").GetString(),
            Developer = data.TryGetProperty("developers", out var devs)
                ? devs[0].GetString()
                : "Unknown",
            Platform = "PC",
            PEGIRating = "N/A"
        };
    }
}

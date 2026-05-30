using ContentService.Models.GoogleBooks;
using Shared.DTO.Content;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration; // Необхідно для роботи зі змінними середовища (.env)
using System.IO; // Для читання .env файлів

namespace ContentService.Services
{
    public class GoogleBooksIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly BookImportService _bookImportService;
        private readonly ILogger<GoogleBooksIntegrationService> _logger;
        private readonly string _apiKey;

        public GoogleBooksIntegrationService(
            HttpClient httpClient, 
            BookImportService bookImportService, 
            ILogger<GoogleBooksIntegrationService> logger,
            IConfiguration configuration) // Інжектимо конфігурацію
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://www.googleapis.com/books/v1/");
            _bookImportService = bookImportService;
            _logger = logger;
            
            // Читаємо ключ зі стандартного джерела конфігурації або з ENV
            _apiKey = configuration["GOOGLE_BOOKS_API_KEY"] ?? Environment.GetEnvironmentVariable("GOOGLE_BOOKS_API_KEY");

            // Якщо ще не знайдено, спробуємо знайти .env файли у робочому дереві і прочитати ключ
            if (string.IsNullOrEmpty(_apiKey))
            {
                try
                {
                    var currentDir = Directory.GetCurrentDirectory();
                    // Шукаємо .env у поточній директорії та до 6 рівнів вгору
                    for (int up = 0; up < 6 && !string.IsNullOrEmpty(currentDir); up++)
                    {
                        try
                        {
                            var envFiles = Directory.EnumerateFiles(currentDir, "*.env", SearchOption.TopDirectoryOnly);
                            foreach (var f in envFiles)
                            {
                        try
                        {
                            var lines = File.ReadAllLines(f);
                            foreach (var line in lines)
                            {
                                if (line == null) continue;
                                var trimmed = line.Trim();
                                if (trimmed.StartsWith("#")) continue;
                                if (trimmed.StartsWith("GOOGLE_BOOKS_API_KEY="))
                                {
                                    var parts = trimmed.Split('=', 2);
                                    if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]))
                                    {
                                        _apiKey = parts[1].Trim();
                                        _logger.LogInformation($"[GoogleBooks] API key loaded from file: {f}");
                                        break;
                                    }
                                }
                            }
                        }
                        catch { /* ignore individual file read errors */ }
                        if (!string.IsNullOrEmpty(_apiKey)) break;
                            }
                        }
                        catch { /* ignore individual file read errors */ }
                        if (!string.IsNullOrEmpty(_apiKey)) break;
                        var parent = Directory.GetParent(currentDir);
                        currentDir = parent?.FullName;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"[GoogleBooks] Error searching .env files: {ex.Message}");
                }
            }
        }

        public async Task<System.Collections.Generic.List<Shared.DTO.Content.ImportResultDto>> ImportBooksAsync(string subject = "fiction", int startIndex = 0, int maxResults = 20)
        {
            var results = new System.Collections.Generic.List<Shared.DTO.Content.ImportResultDto>();
            try
            {
                _logger.LogInformation($"[GoogleBooks] Початок імпорту. Subject: {subject}, StartIndex: {startIndex}, MaxResults: {maxResults}");
                
                string requestUri = $"volumes?q=subject:{subject}&startIndex={startIndex}&maxResults={maxResults}";

                // Якщо ключ успішно зчитано з .env, додаємо його в рядок запиту
                if (!string.IsNullOrEmpty(_apiKey))
                {
                    requestUri += $"&key={_apiKey}";
                }
                else
                {
                    _logger.LogWarning("[GoogleBooks] УВАГА: Ключ GOOGLE_BOOKS_API_KEY не знайдено у змінних середовища (.env)! Запит піде без ключа.");
                }

                // Perform request with simple retry/backoff for rate limiting (429)
                System.Net.Http.HttpResponseMessage response = null;
                int maxRetriesHttp = 3;
                int attempt = 0;
                while (attempt < maxRetriesHttp)
                {
                    response = await _httpClient.GetAsync(requestUri);
                    if (response.IsSuccessStatusCode) break;
                    if ((int)response.StatusCode == 429)
                    {
                        _logger.LogWarning($"[GoogleBooks] Rate limited (429). Retry {attempt + 1}/{maxRetriesHttp} after backoff.");
                        int delayMs = (int)Math.Pow(2, attempt) * 1000; // 1s, 2s, 4s
                        await Task.Delay(delayMs);
                        attempt++;
                        continue;
                    }
                    break;
                }

                if (response == null)
                {
                    _logger.LogError("[GoogleBooks] No response from Google Books API.");
                    results.Add(new Shared.DTO.Content.ImportResultDto { ExternalID = null, Title = null, Success = false, ErrorMessage = "No response from API" });
                    return results;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var respBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"[GoogleBooks] Google Books API повернув статус {response.StatusCode}. Body: {respBody}");
                    results.Add(new Shared.DTO.Content.ImportResultDto { ExternalID = null, Title = null, Success = false, ErrorMessage = $"Status {(int)response.StatusCode} - {response.StatusCode}: {respBody}" });
                    return results;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"[GoogleBooks] Отримано {jsonString.Length} байт даних");
                
                var booksData = JsonSerializer.Deserialize<GoogleBooksResponse>(jsonString);

                if (booksData?.Items == null || !booksData.Items.Any())
                {
                    _logger.LogWarning($"[GoogleBooks] Немає результатів для запиту. Items: {booksData?.Items?.Count ?? 0}");
                    return results;
                }

                _logger.LogInformation($"[GoogleBooks] Обробка {booksData.Items.Count} книг");

                foreach (var item in booksData.Items)
                {
                    try
                    {
                        var info = item.VolumeInfo;
                        if (info == null) continue;

                        DateTime releaseDate = DateTime.UtcNow;
                        if (!string.IsNullOrEmpty(info.PublishedDate))
                        {
                            if (info.PublishedDate.Length == 4) info.PublishedDate += "-01-01";
                            else if (info.PublishedDate.Length == 7) info.PublishedDate += "-01";
                            if (!DateTime.TryParse(info.PublishedDate, out releaseDate))
                            {
                                releaseDate = DateTime.UtcNow;
                            }
                        }

                        string isbn = "Невідомо";
                        if (info.IndustryIdentifiers != null)
                        {
                            var isbn13 = info.IndustryIdentifiers.FirstOrDefault(i => i.Type == "ISBN_13");
                            var isbn10 = info.IndustryIdentifiers.FirstOrDefault(i => i.Type == "ISBN_10");
                            isbn = isbn13?.Identifier ?? isbn10?.Identifier ?? "Невідомо";
                        }

                        var genresList = new System.Collections.Generic.List<string>();
                        var tagsList = new System.Collections.Generic.List<string>();

                        if (info.Categories != null && info.Categories.Any())
                        {
                            for (int i = 0; i < info.Categories.Count; i++)
                            {
                                var categoryString = info.Categories[i];
                                
                                var parts = categoryString.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);

                                if (parts.Length > 0)
                                {
                                    if (i == 0)
                                    {
                                        genresList.Add(parts[0].Trim());
                                    }
                                    else
                                    {
                                        tagsList.Add(parts[0].Trim());
                                    }

                                    for (int j = 1; j < parts.Length; j++)
                                    {
                                        tagsList.Add(parts[j].Trim());
                                    }
                                }
                            }
                        }

                        if (!tagsList.Any() && !string.IsNullOrWhiteSpace(subject))
                        {
                            tagsList.Add(subject.ToLower());
                        }

                        var dto = new BookImportDto
                        {
                            ExternalID = item.Id,
                            ExternalSource = "GoogleBooks",
                            Title = info.Title ?? "Без назви",
                            Description = string.IsNullOrWhiteSpace(info.Description) ? "Опис відсутній" : info.Description,
                            ReleaseDate = releaseDate,

                            AverageRating = (info.AverageRating ?? 0.0) * 2,

                            PosterURL = info.ImageLinks?.Thumbnail ?? string.Empty,
                            Author = info.Authors != null && info.Authors.Any() ? string.Join(", ", info.Authors) : "Невідомий автор",
                            Pages = info.PageCount ?? 0,
                            ISBN = isbn,
                            Publisher = info.Publisher ?? "Невідомий видавець",
                            BookSeries = "",

                            Genres = genresList.Distinct().ToList(),
                            Tags = tagsList.Distinct().ToList(),

                            Languages = string.IsNullOrEmpty(info.Language)
                                ? new System.Collections.Generic.List<string>()
                                : new System.Collections.Generic.List<string> { info.Language }
                        };

                        var importResult = await _bookImportService.ImportBookWithResultAsync(dto);
                        results.Add(importResult);
                        _logger.LogDebug($"[GoogleBooks] Результат імпорту: {importResult.ExternalID} => Success={importResult.Success}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"[GoogleBooks] Помилка при імпорті однієї книги: {ex.Message}");
                        results.Add(new Shared.DTO.Content.ImportResultDto { ExternalID = item.Id, Title = item.VolumeInfo?.Title ?? "?", Success = false, ErrorMessage = ex.Message });
                    }
                }

                _logger.LogInformation("[GoogleBooks] Імпорт завершено успішно");
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GoogleBooks] КРИТИЧНА ПОМИЛКА при імпорті книг");
                return results;
            }
        }
    }
}
using ContentService.Models.GoogleBooks;
using Shared.DTO.Content;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ContentService.Services
{
    public class GoogleBooksIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly BookImportService _bookImportService;

        public GoogleBooksIntegrationService(HttpClient httpClient, BookImportService bookImportService)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://www.googleapis.com/books/v1/");
            _bookImportService = bookImportService;
        }

        public async Task ImportBooksAsync(string subject = "fiction", int startIndex = 0, int maxResults = 20)
        {
            string requestUri = $"volumes?q=subject:{subject}&startIndex={startIndex}&maxResults={maxResults}";

            var response = await _httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode) return;

            var jsonString = await response.Content.ReadAsStringAsync();
            var booksData = JsonSerializer.Deserialize<GoogleBooksResponse>(jsonString);

            if (booksData?.Items == null) return;

            foreach (var item in booksData.Items)
            {
                var info = item.VolumeInfo;
                if (info == null) continue;

                
                DateTime releaseDate = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(info.PublishedDate))
                {
                    if (info.PublishedDate.Length == 4) info.PublishedDate += "-01-01";
                    else if (info.PublishedDate.Length == 7) info.PublishedDate += "-01";
                    DateTime.TryParse(info.PublishedDate, out releaseDate);
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
                    Author = info.Authors != null ? string.Join(", ", info.Authors) : "Невідомий автор",
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

                await _bookImportService.ImportBookAsync(dto);
            }
        }
    }
}

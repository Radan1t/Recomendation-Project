using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ContentService.Services
{
    
    public interface IDictionarySyncService
    {
        Task SyncGenreAsync(string genreName);
        Task SyncTagAsync(string tagName);
        Task SyncLanguageAsync(string languageCode); 
    }

    
    public class DictionarySyncService : IDictionarySyncService
    {
        private readonly HttpClient _httpClient;

        public DictionarySyncService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            
            _httpClient.BaseAddress = new System.Uri(config["UserService:BaseUrl"]);
        }

        public async Task SyncGenreAsync(string genreName)
        {
            try
            {
                await _httpClient.PostAsJsonAsync("/api/genres/sync", new { Name = genreName });
            }
            catch {  }
        }

        public async Task SyncTagAsync(string tagName)
        {
            try
            {
                await _httpClient.PostAsJsonAsync("/api/tags/sync", new { Name = tagName });
            }
            catch {  }
        }

        public async Task SyncLanguageAsync(string languageCode)
        {
            try
            {
                
                await _httpClient.PostAsJsonAsync("/api/languages/sync", new { Code = languageCode, Name = languageCode });
            }
            catch {  }
        }
    }
}

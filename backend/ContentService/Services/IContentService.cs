using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTO.Content;

namespace ContentService.Services
{
    public interface IContentService
    {
        Task<List<string>> GetRandomPostersAsync(int count);
        Task<List<GenreDto>> GetGenresAsync();
        Task<List<LanguageDto>> GetLanguagesAsync();
        Task<List<object>> GetContentDetailsAsync(int[] ids);
        Task<ContentDetailDto?> GetSingleContentAsync(int id);
        Task<Dictionary<int, List<string>>> GetGenresBatchAsync(List<int> contentIds);
        Task<List<object>> GetContentListForAdminAsync(string contentType);
        Task<bool> DeleteContentAsync(int id);
        Task<bool> UpdateContentAsync(int id, ContentUpdateDto dto);
        Task<List<object>> SearchGlobalContentAsync(string query);
    }
}

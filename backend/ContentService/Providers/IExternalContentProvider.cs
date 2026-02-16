using Shared.DTO.Content;

namespace ContentService.Providers;

public interface IExternalContentProvider
{
    Task<List<ContentDto>> GetContentsAsync();
}

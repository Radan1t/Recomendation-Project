using ContentService.Providers;
using Shared.DTO.Content;

namespace ContentService.Services;

public class ContentService : IContentService
{
    private readonly IExternalContentProvider _provider;

    public ContentService(IExternalContentProvider provider)
    {
        _provider = provider;
    }

    public async Task<List<ContentDto>> GetAllAsync()
    {
        return await _provider.GetContentsAsync();
    }
}

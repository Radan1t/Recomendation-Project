using Shared.DTO.Content;

public interface IContentService
{
    Task<List<ContentDto>> GetAllAsync();
}

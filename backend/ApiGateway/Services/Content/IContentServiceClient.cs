using Shared.DTO.Content;

namespace ApiGateway.Services.Content;

public interface IContentServiceClient
{
    Task<List<ContentDto>> GetAllAsync();
}

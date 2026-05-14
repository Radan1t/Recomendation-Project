using Shared.DTO.Interactions;
using System.Threading.Tasks;

namespace InteractionService.Services;

public interface IInteractionService
{
    Task<bool> RateContentAsync(int userId, RatingRequestDto dto);
    Task<bool> ToggleFavoriteAsync(int userId, int contentId);
    Task<InteractionStatusDto> GetStatusAsync(int userId, int contentId);
}

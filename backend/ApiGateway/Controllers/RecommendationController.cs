using ApiGateway.Services.Recommendation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTO.Recommendation;

namespace ApiGateway.Controllers;

[Authorize]
[ApiController]
[Route("api/recommendations")]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationServiceClient _recs;

    public RecommendationController(IRecommendationServiceClient recs)
    {
        _recs = recs;
    }

    [HttpPost]
    public async Task<ActionResult<List<RecommendedItemDto>>> Get(
        RecommendationRequestDto dto)
    {
        return Ok(await _recs.GetAsync(dto));
    }
}

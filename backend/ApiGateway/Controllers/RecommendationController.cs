using ApiGateway.Services.Recommendation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTO.Recommendation;

namespace ApiGateway.Controllers;

[Authorize]
[ApiController]

[Route("api/v1/recommendations")] 
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationServiceClient _recs;

    public RecommendationController(IRecommendationServiceClient recs)
    {
        _recs = recs;
    }

    
    [HttpPost("generate/{userId}")]
    public async Task<ActionResult> Generate(int userId)
    {
        return Ok(await _recs.GenerateAsync(userId));
    }

    
    [HttpGet("{id}")]
    public async Task<ActionResult<List<RecommendedItemDto>>> GetSimilar(int id)
    {
        
        var results = await _recs.GetSimilarAsync(id);
        return Ok(results);
    }
}

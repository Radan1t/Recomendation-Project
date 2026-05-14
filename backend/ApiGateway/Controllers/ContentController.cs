using ApiGateway.Services.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTO.Content;

namespace ApiGateway.Controllers;

[Authorize]
[ApiController]
[Route("api/content")]
public class ContentController : ControllerBase
{
    private readonly IContentServiceClient _content;

    public ContentController(IContentServiceClient content)
    {
        _content = content;
    }

    [HttpGet]
    public async Task<ActionResult<List<ContentDto>>> GetAll()
    {
        return Ok(await _content.GetAllAsync());
    }
}


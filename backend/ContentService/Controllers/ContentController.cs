using Microsoft.AspNetCore.Mvc;
using Shared.DTO.Content;
using ContentService.Services;

namespace ContentService.Controllers;

[ApiController]
[Route("content")]
public class ContentController : ControllerBase
{
    private readonly IContentService _service;

    public ContentController(IContentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<ContentDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }
}

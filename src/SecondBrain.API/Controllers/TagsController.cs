using Microsoft.AspNetCore.Mvc;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Interfaces;

namespace SecondBrain.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TagsController(ITagService tagService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<TagDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TagDto>>> GetAll(CancellationToken cancellationToken)
    {
        var tags = await tagService.GetAllAsync(cancellationToken);
        return Ok(tags);
    }

    [HttpPost]
    [ProducesResponseType<TagDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagDto>> Create([FromBody] CreateTagRequest request, CancellationToken cancellationToken)
    {
        // Sem GetById dedicado (Tag é só nome — a listagem já serve) — 201 sem Location de item único.
        var created = await tagService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await tagService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

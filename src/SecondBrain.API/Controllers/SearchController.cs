using Microsoft.AspNetCore.Mvc;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Interfaces;

namespace SecondBrain.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    /// <summary>Busca full-text (Postgres) em Concepts, Notes e Projects.</summary>
    [HttpGet]
    [ProducesResponseType<SearchResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchResponse>> Search([FromQuery] string q, CancellationToken cancellationToken)
    {
        var result = await searchService.SearchAsync(q ?? string.Empty, cancellationToken);
        return Ok(result);
    }
}

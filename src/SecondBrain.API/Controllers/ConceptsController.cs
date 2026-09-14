using Microsoft.AspNetCore.Mvc;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Interfaces;

namespace SecondBrain.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConceptsController(IConceptService conceptService) : ControllerBase
{
    /// <summary>Lista todos os conceitos, ordenados por nome.</summary>
    [HttpGet]
    [ProducesResponseType<List<ConceptDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ConceptDto>>> GetAll(CancellationToken cancellationToken)
    {
        var concepts = await conceptService.GetAllAsync(cancellationToken);
        return Ok(concepts);
    }

    /// <summary>Busca um conceito pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ConceptDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConceptDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var concept = await conceptService.GetByIdAsync(id, cancellationToken);
        return Ok(concept);
    }

    /// <summary>Cria um novo conceito.</summary>
    [HttpPost]
    [ProducesResponseType<ConceptDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ConceptDto>> Create(
        [FromBody] CreateConceptRequest request, CancellationToken cancellationToken)
    {
        var created = await conceptService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza um conceito existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ConceptDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ConceptDto>> Update(
        Guid id, [FromBody] UpdateConceptRequest request, CancellationToken cancellationToken)
    {
        var updated = await conceptService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Remove um conceito.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await conceptService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

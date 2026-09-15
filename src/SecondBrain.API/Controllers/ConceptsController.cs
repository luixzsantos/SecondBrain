using Microsoft.AspNetCore.Mvc;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Interfaces;

namespace SecondBrain.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConceptsController(IConceptService conceptService) : ControllerBase
{
    /// <summary>Lista os conceitos, ordenados por nome — opcionalmente filtrados por Tag (ex: uma linguagem).</summary>
    [HttpGet]
    [ProducesResponseType<List<ConceptDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ConceptDto>>> GetAll([FromQuery] Guid? tagId, CancellationToken cancellationToken)
    {
        var concepts = tagId.HasValue
            ? await conceptService.GetAllByTagAsync(tagId.Value, cancellationToken)
            : await conceptService.GetAllAsync(cancellationToken);
        return Ok(concepts);
    }

    /// <summary>"Página do conceito": dados do conceito + notas/projetos/tags relacionados.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ConceptDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConceptDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var concept = await conceptService.GetDetailByIdAsync(id, cancellationToken);
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

    /// <summary>Relaciona uma Note a este Concept ("explicado em").</summary>
    [HttpPost("{conceptId:guid}/notes/{noteId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LinkNote(Guid conceptId, Guid noteId, CancellationToken cancellationToken)
    {
        await conceptService.LinkNoteAsync(conceptId, noteId, cancellationToken);
        return NoContent();
    }

    /// <summary>Remove a relação entre este Concept e uma Note.</summary>
    [HttpDelete("{conceptId:guid}/notes/{noteId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkNote(Guid conceptId, Guid noteId, CancellationToken cancellationToken)
    {
        await conceptService.UnlinkNoteAsync(conceptId, noteId, cancellationToken);
        return NoContent();
    }

    /// <summary>Relaciona um Project a este Concept ("usado em").</summary>
    [HttpPost("{conceptId:guid}/projects/{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LinkProject(Guid conceptId, Guid projectId, CancellationToken cancellationToken)
    {
        await conceptService.LinkProjectAsync(conceptId, projectId, cancellationToken);
        return NoContent();
    }

    /// <summary>Remove a relação entre este Concept e um Project.</summary>
    [HttpDelete("{conceptId:guid}/projects/{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkProject(Guid conceptId, Guid projectId, CancellationToken cancellationToken)
    {
        await conceptService.UnlinkProjectAsync(conceptId, projectId, cancellationToken);
        return NoContent();
    }

    /// <summary>Relaciona uma Tag a este Concept.</summary>
    [HttpPost("{conceptId:guid}/tags/{tagId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LinkTag(Guid conceptId, Guid tagId, CancellationToken cancellationToken)
    {
        await conceptService.LinkTagAsync(conceptId, tagId, cancellationToken);
        return NoContent();
    }

    /// <summary>Remove a relação entre este Concept e uma Tag.</summary>
    [HttpDelete("{conceptId:guid}/tags/{tagId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkTag(Guid conceptId, Guid tagId, CancellationToken cancellationToken)
    {
        await conceptService.UnlinkTagAsync(conceptId, tagId, cancellationToken);
        return NoContent();
    }
}

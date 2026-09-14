using Microsoft.AspNetCore.Mvc;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Interfaces;

namespace SecondBrain.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NotesController(INoteService noteService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<NoteDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NoteDto>>> GetAll(CancellationToken cancellationToken)
    {
        var notes = await noteService.GetAllAsync(cancellationToken);
        return Ok(notes);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<NoteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NoteDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var note = await noteService.GetByIdAsync(id, cancellationToken);
        return Ok(note);
    }

    /// <summary>Cria uma nova nota, opcionalmente já relacionada a Concepts (ConceptIds).</summary>
    [HttpPost]
    [ProducesResponseType<NoteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NoteDto>> Create([FromBody] CreateNoteRequest request, CancellationToken cancellationToken)
    {
        var created = await noteService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<NoteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NoteDto>> Update(Guid id, [FromBody] UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        var updated = await noteService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await noteService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

using System.ComponentModel.DataAnnotations;

namespace SecondBrain.Application.DTOs;

public record NoteDto(
    Guid Id,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public class CreateNoteRequest
{
    [Required(ErrorMessage = "Title é obrigatório.")]
    [MaxLength(200, ErrorMessage = "Title deve ter no máximo 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content é obrigatório.")]
    public string Content { get; set; } = string.Empty;

    // Concepts já relacionados no momento da criação (opcional — também dá pra
    // linkar depois via POST /api/concepts/{conceptId}/notes/{noteId}).
    public List<Guid> ConceptIds { get; set; } = [];
}

public class UpdateNoteRequest
{
    [Required(ErrorMessage = "Title é obrigatório.")]
    [MaxLength(200, ErrorMessage = "Title deve ter no máximo 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content é obrigatório.")]
    public string Content { get; set; } = string.Empty;
}

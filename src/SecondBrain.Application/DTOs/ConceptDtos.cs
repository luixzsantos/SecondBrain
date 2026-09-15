using System.ComponentModel.DataAnnotations;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.DTOs;

public record ConceptDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public class CreateConceptRequest
{
    [Required(ErrorMessage = "Name é obrigatório.")]
    [MaxLength(150, ErrorMessage = "Name deve ter no máximo 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(4000, ErrorMessage = "Description deve ter no máximo 4000 caracteres.")]
    public string? Description { get; set; }
}

public class UpdateConceptRequest
{
    [Required(ErrorMessage = "Name é obrigatório.")]
    [MaxLength(150, ErrorMessage = "Name deve ter no máximo 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(4000, ErrorMessage = "Description deve ter no máximo 4000 caracteres.")]
    public string? Description { get; set; }
}

public class LinkConceptRelationRequest
{
    [Required(ErrorMessage = "Type é obrigatório.")]
    public ConceptRelationType Type { get; set; }
}

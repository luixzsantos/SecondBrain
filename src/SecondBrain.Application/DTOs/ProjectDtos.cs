using System.ComponentModel.DataAnnotations;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.DTOs;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public class CreateProjectRequest
{
    [Required(ErrorMessage = "Name é obrigatório.")]
    [MaxLength(150, ErrorMessage = "Name deve ter no máximo 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(4000, ErrorMessage = "Description deve ter no máximo 4000 caracteres.")]
    public string? Description { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
}

public class UpdateProjectRequest
{
    [Required(ErrorMessage = "Name é obrigatório.")]
    [MaxLength(150, ErrorMessage = "Name deve ter no máximo 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(4000, ErrorMessage = "Description deve ter no máximo 4000 caracteres.")]
    public string? Description { get; set; }

    public ProjectStatus Status { get; set; }
}

namespace SecondBrain.Application.DTOs;

// Visão completa de um Concept — "página do conceito": o que ele é + tudo que já
// foi relacionado a ele (ver visão do produto no README: notas, projetos e tags relacionados).
public record ConceptDetailDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<NoteDto> Notes,
    List<ProjectDto> Projects,
    List<TagDto> Tags
);

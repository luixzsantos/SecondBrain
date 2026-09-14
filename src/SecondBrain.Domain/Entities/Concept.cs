namespace SecondBrain.Domain.Entities;

// Representa algo que o usuário está aprendendo (ex: "Redis", "Goroutines").
// É o núcleo do dicionário técnico pessoal — as demais entidades (Note, Project, Tag)
// vão se relacionar com Concept nas próximas versões.
public class Concept
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ConceptNote> ConceptNotes { get; set; } = [];
    public ICollection<ConceptProject> ConceptProjects { get; set; } = [];
    public ICollection<ConceptTag> ConceptTags { get; set; } = [];
}

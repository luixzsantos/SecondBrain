namespace SecondBrain.Domain.Entities;

// Rótulo simples (ex: "backend", "go", "database") para agrupar Concepts por tema.
public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ConceptTag> ConceptTags { get; set; } = [];
}

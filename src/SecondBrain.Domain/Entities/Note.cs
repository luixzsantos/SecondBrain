namespace SecondBrain.Domain.Entities;

// Uma anotação de estudo. Pode estar relacionada a vários Concepts (ver ConceptNote) —
// "Redis Streams na prática" pode explicar tanto Redis quanto Filas, por exemplo.
public class Note
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ConceptNote> ConceptNotes { get; set; } = [];
}

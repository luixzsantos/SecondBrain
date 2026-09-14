namespace SecondBrain.Domain.Entities;

// Relação N:N tipada Concept<->Tag.
public class ConceptTag
{
    public Guid ConceptId { get; set; }
    public Concept Concept { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}

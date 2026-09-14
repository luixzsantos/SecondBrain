namespace SecondBrain.Domain.Entities;

// Relação N:N tipada Concept<->Project ("usado em").
public class ConceptProject
{
    public Guid ConceptId { get; set; }
    public Concept Concept { get; set; } = null!;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}

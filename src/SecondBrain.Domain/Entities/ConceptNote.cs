namespace SecondBrain.Domain.Entities;

// Relação N:N tipada Concept<->Note ("explicado em"). Ver decisão de não usar
// uma tabela KnowledgeRelation genérica/polimórfica — cada par de tipos tem sua própria tabela.
public class ConceptNote
{
    public Guid ConceptId { get; set; }
    public Concept Concept { get; set; } = null!;

    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}

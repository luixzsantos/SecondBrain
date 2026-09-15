namespace SecondBrain.Domain.Entities;

// Relação Concept<->Concept: o grafo de conhecimento em si. Guardada uma vez, num
// sentido (SourceConceptId -> TargetConceptId), mas exibida nos dois lados — ver
// ConceptRepository.GetByIdWithRelationsAsync, que busca onde o conceito é Source OU Target.
public class ConceptRelation
{
    public Guid SourceConceptId { get; set; }
    public Concept SourceConcept { get; set; } = null!;

    public Guid TargetConceptId { get; set; }
    public Concept TargetConcept { get; set; } = null!;

    public ConceptRelationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}

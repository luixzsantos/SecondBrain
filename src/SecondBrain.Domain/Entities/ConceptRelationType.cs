namespace SecondBrain.Domain.Entities;

// Começa pequeno de propósito (ver decisão de não usar KnowledgeRelation genérico) —
// dá pra crescer esse enum quando fizer falta de verdade, sem migrar nada existente.
public enum ConceptRelationType
{
    RelatedTo,
    AlternativeTo,
}

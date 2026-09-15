using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.Interfaces;

// Abstração de acesso a dados — a implementação concreta (EF Core + PostgreSQL)
// vive em SecondBrain.Infrastructure. A Application não sabe (nem precisa saber) qual banco existe.
public interface IConceptRepository
{
    Task<List<Concept>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Concept>> GetAllByTagAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task<Concept?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Concept?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Concept concept, CancellationToken cancellationToken = default);
    void Remove(Concept concept);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Traz o Concept já com Notes/Projects/Tags relacionados carregados — usado na
    // "página do conceito" (GET /api/concepts/{id}).
    Task<Concept?> GetByIdWithRelationsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> NoteLinkExistsAsync(Guid conceptId, Guid noteId, CancellationToken cancellationToken = default);
    Task AddNoteLinkAsync(Guid conceptId, Guid noteId, CancellationToken cancellationToken = default);
    Task<bool> RemoveNoteLinkAsync(Guid conceptId, Guid noteId, CancellationToken cancellationToken = default);

    Task<bool> ProjectLinkExistsAsync(Guid conceptId, Guid projectId, CancellationToken cancellationToken = default);
    Task AddProjectLinkAsync(Guid conceptId, Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> RemoveProjectLinkAsync(Guid conceptId, Guid projectId, CancellationToken cancellationToken = default);

    Task<bool> TagLinkExistsAsync(Guid conceptId, Guid tagId, CancellationToken cancellationToken = default);
    Task AddTagLinkAsync(Guid conceptId, Guid tagId, CancellationToken cancellationToken = default);
    Task<bool> RemoveTagLinkAsync(Guid conceptId, Guid tagId, CancellationToken cancellationToken = default);

    // Grafo de conhecimento: relação Concept<->Concept. "Exists"/"Remove" ignoram o sentido
    // (source/target) em que a relação foi criada — pra quem usa, "A relacionado a B" e
    // "B relacionado a A" são a mesma relação.
    Task<bool> RelationExistsAsync(Guid conceptId, Guid relatedConceptId, ConceptRelationType type, CancellationToken cancellationToken = default);
    Task AddRelationAsync(Guid conceptId, Guid relatedConceptId, ConceptRelationType type, CancellationToken cancellationToken = default);
    Task<bool> RemoveRelationAsync(Guid conceptId, Guid relatedConceptId, ConceptRelationType type, CancellationToken cancellationToken = default);
}

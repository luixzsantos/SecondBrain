using SecondBrain.Application.DTOs;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.Interfaces;

public interface IConceptService
{
    Task<List<ConceptDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<ConceptDto>> GetAllByTagAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task<ConceptDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConceptDetailDto> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConceptDto> CreateAsync(CreateConceptRequest request, CancellationToken cancellationToken = default);
    Task<ConceptDto> UpdateAsync(Guid id, UpdateConceptRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task LinkNoteAsync(Guid conceptId, Guid noteId, CancellationToken cancellationToken = default);
    Task UnlinkNoteAsync(Guid conceptId, Guid noteId, CancellationToken cancellationToken = default);

    Task LinkProjectAsync(Guid conceptId, Guid projectId, CancellationToken cancellationToken = default);
    Task UnlinkProjectAsync(Guid conceptId, Guid projectId, CancellationToken cancellationToken = default);

    Task LinkTagAsync(Guid conceptId, Guid tagId, CancellationToken cancellationToken = default);
    Task UnlinkTagAsync(Guid conceptId, Guid tagId, CancellationToken cancellationToken = default);

    Task LinkRelationAsync(Guid conceptId, Guid relatedConceptId, ConceptRelationType type, CancellationToken cancellationToken = default);
    Task UnlinkRelationAsync(Guid conceptId, Guid relatedConceptId, ConceptRelationType type, CancellationToken cancellationToken = default);
}

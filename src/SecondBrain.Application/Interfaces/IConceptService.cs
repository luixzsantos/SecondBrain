using SecondBrain.Application.DTOs;

namespace SecondBrain.Application.Interfaces;

public interface IConceptService
{
    Task<List<ConceptDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConceptDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConceptDto> CreateAsync(CreateConceptRequest request, CancellationToken cancellationToken = default);
    Task<ConceptDto> UpdateAsync(Guid id, UpdateConceptRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

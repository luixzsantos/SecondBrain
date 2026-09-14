using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.Interfaces;

// Abstração de acesso a dados — a implementação concreta (EF Core + PostgreSQL)
// vive em SecondBrain.Infrastructure. A Application não sabe (nem precisa saber) qual banco existe.
public interface IConceptRepository
{
    Task<List<Concept>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Concept?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Concept?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Concept concept, CancellationToken cancellationToken = default);
    void Remove(Concept concept);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}

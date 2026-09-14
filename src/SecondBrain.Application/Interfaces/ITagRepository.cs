using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.Interfaces;

public interface ITagRepository
{
    Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Tag tag, CancellationToken cancellationToken = default);
    void Remove(Tag tag);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}

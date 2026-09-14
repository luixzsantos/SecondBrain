using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.Interfaces;

public interface INoteRepository
{
    Task<List<Note>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Note note, CancellationToken cancellationToken = default);
    void Remove(Note note);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}

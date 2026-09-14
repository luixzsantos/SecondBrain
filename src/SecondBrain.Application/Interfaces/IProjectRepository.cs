using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.Interfaces;

public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    void Remove(Project project);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}

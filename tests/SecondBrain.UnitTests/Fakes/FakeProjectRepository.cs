using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.UnitTests.Fakes;

public class FakeProjectRepository : IProjectRepository
{
    public readonly List<Project> Projects = [];

    public Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Projects.OrderBy(p => p.Name).ToList());

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Projects.FirstOrDefault(p => p.Id == id));

    public Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        Projects.Add(project);
        return Task.CompletedTask;
    }

    public void Remove(Project project) => Projects.Remove(project);

    public Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
}

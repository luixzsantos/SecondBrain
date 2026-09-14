using Microsoft.EntityFrameworkCore;
using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Infrastructure.Persistence;

public class ProjectRepository(SecondBrainDbContext context) : IProjectRepository
{
    public async Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Projects.AsNoTracking().OrderBy(p => p.Name).ToListAsync(cancellationToken);

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default) =>
        await context.Projects.AddAsync(project, cancellationToken);

    public void Remove(Project project) => context.Projects.Remove(project);

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken) > 0;
}

using Microsoft.EntityFrameworkCore;
using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Infrastructure.Persistence;

public class ConceptRepository(SecondBrainDbContext context) : IConceptRepository
{
    public async Task<List<Concept>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Concepts
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<Concept?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Concepts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Concept?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await context.Concepts.FirstOrDefaultAsync(
            c => c.Name.ToLower() == name.Trim().ToLower(), cancellationToken);

    public async Task AddAsync(Concept concept, CancellationToken cancellationToken = default) =>
        await context.Concepts.AddAsync(concept, cancellationToken);

    public void Remove(Concept concept) => context.Concepts.Remove(concept);

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken) > 0;
}

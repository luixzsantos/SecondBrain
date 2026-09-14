using Microsoft.EntityFrameworkCore;
using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Infrastructure.Persistence;

public class TagRepository(SecondBrainDbContext context) : ITagRepository
{
    public async Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Tags.AsNoTracking().OrderBy(t => t.Name).ToListAsync(cancellationToken);

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == name.Trim().ToLower(), cancellationToken);

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default) =>
        await context.Tags.AddAsync(tag, cancellationToken);

    public void Remove(Tag tag) => context.Tags.Remove(tag);

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken) > 0;
}

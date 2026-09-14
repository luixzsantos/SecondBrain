using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.UnitTests.Fakes;

public class FakeTagRepository : ITagRepository
{
    public readonly List<Tag> Tags = [];

    public Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Tags.OrderBy(t => t.Name).ToList());

    public Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tags.FirstOrDefault(t => t.Id == id));

    public Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tags.FirstOrDefault(t => t.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        Tags.Add(tag);
        return Task.CompletedTask;
    }

    public void Remove(Tag tag) => Tags.Remove(tag);

    public Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
}

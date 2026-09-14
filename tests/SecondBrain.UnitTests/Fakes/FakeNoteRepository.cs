using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.UnitTests.Fakes;

public class FakeNoteRepository : INoteRepository
{
    public readonly List<Note> Notes = [];

    public Task<List<Note>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Notes.OrderBy(n => n.Title).ToList());

    public Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Notes.FirstOrDefault(n => n.Id == id));

    public Task AddAsync(Note note, CancellationToken cancellationToken = default)
    {
        Notes.Add(note);
        return Task.CompletedTask;
    }

    public void Remove(Note note) => Notes.Remove(note);

    public Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
}

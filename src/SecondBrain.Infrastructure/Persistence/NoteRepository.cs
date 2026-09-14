using Microsoft.EntityFrameworkCore;
using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Infrastructure.Persistence;

public class NoteRepository(SecondBrainDbContext context) : INoteRepository
{
    public async Task<List<Note>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Notes.AsNoTracking().OrderBy(n => n.Title).ToListAsync(cancellationToken);

    public async Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Notes.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task AddAsync(Note note, CancellationToken cancellationToken = default) =>
        await context.Notes.AddAsync(note, cancellationToken);

    public void Remove(Note note) => context.Notes.Remove(note);

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken) > 0;
}

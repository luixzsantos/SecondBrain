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

    public async Task<Concept?> GetByIdWithRelationsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Concepts
            .Include(c => c.ConceptNotes).ThenInclude(cn => cn.Note)
            .Include(c => c.ConceptProjects).ThenInclude(cp => cp.Project)
            .Include(c => c.ConceptTags).ThenInclude(ct => ct.Tag)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> NoteLinkExistsAsync(Guid conceptId, Guid noteId, CancellationToken cancellationToken = default) =>
        context.ConceptNotes.AnyAsync(cn => cn.ConceptId == conceptId && cn.NoteId == noteId, cancellationToken);

    public async Task AddNoteLinkAsync(Guid conceptId, Guid noteId, CancellationToken cancellationToken = default) =>
        await context.ConceptNotes.AddAsync(
            new ConceptNote { ConceptId = conceptId, NoteId = noteId, CreatedAt = DateTime.UtcNow }, cancellationToken);

    public async Task<bool> RemoveNoteLinkAsync(Guid conceptId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var link = await context.ConceptNotes
            .FirstOrDefaultAsync(cn => cn.ConceptId == conceptId && cn.NoteId == noteId, cancellationToken);
        if (link is null)
        {
            return false;
        }

        context.ConceptNotes.Remove(link);
        return true;
    }

    public Task<bool> ProjectLinkExistsAsync(Guid conceptId, Guid projectId, CancellationToken cancellationToken = default) =>
        context.ConceptProjects.AnyAsync(cp => cp.ConceptId == conceptId && cp.ProjectId == projectId, cancellationToken);

    public async Task AddProjectLinkAsync(Guid conceptId, Guid projectId, CancellationToken cancellationToken = default) =>
        await context.ConceptProjects.AddAsync(
            new ConceptProject { ConceptId = conceptId, ProjectId = projectId, CreatedAt = DateTime.UtcNow }, cancellationToken);

    public async Task<bool> RemoveProjectLinkAsync(Guid conceptId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var link = await context.ConceptProjects
            .FirstOrDefaultAsync(cp => cp.ConceptId == conceptId && cp.ProjectId == projectId, cancellationToken);
        if (link is null)
        {
            return false;
        }

        context.ConceptProjects.Remove(link);
        return true;
    }

    public Task<bool> TagLinkExistsAsync(Guid conceptId, Guid tagId, CancellationToken cancellationToken = default) =>
        context.ConceptTags.AnyAsync(ct => ct.ConceptId == conceptId && ct.TagId == tagId, cancellationToken);

    public async Task AddTagLinkAsync(Guid conceptId, Guid tagId, CancellationToken cancellationToken = default) =>
        await context.ConceptTags.AddAsync(
            new ConceptTag { ConceptId = conceptId, TagId = tagId, CreatedAt = DateTime.UtcNow }, cancellationToken);

    public async Task<bool> RemoveTagLinkAsync(Guid conceptId, Guid tagId, CancellationToken cancellationToken = default)
    {
        var link = await context.ConceptTags
            .FirstOrDefaultAsync(ct => ct.ConceptId == conceptId && ct.TagId == tagId, cancellationToken);
        if (link is null)
        {
            return false;
        }

        context.ConceptTags.Remove(link);
        return true;
    }
}

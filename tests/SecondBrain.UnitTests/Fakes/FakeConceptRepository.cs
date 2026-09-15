using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.UnitTests.Fakes;

// Repositório em memória — evita subir Postgres/EF nos testes unitários e evita
// depender de uma lib de mock pra um contrato simples como este. Recebe os outros
// fakes pra conseguir montar as relações em GetByIdWithRelationsAsync.
public class FakeConceptRepository(
    FakeNoteRepository notes,
    FakeProjectRepository projects,
    FakeTagRepository tags) : IConceptRepository
{
    private readonly List<Concept> _concepts = [];
    private readonly List<(Guid ConceptId, Guid NoteId)> _noteLinks = [];
    private readonly List<(Guid ConceptId, Guid ProjectId)> _projectLinks = [];
    private readonly List<(Guid ConceptId, Guid TagId)> _tagLinks = [];
    private readonly List<(Guid SourceId, Guid TargetId, ConceptRelationType Type)> _relations = [];

    private static int LevelSortKey(Concept c) => c.Level switch
    {
        ConceptLevel.Basico => 0,
        ConceptLevel.Intermediario => 1,
        ConceptLevel.Avancado => 2,
        _ => 3,
    };

    public Task<List<Concept>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_concepts.OrderBy(LevelSortKey).ThenBy(c => c.Name).ToList());

    public Task<List<Concept>> GetAllByTagAsync(Guid tagId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tagLinks
            .Where(l => l.TagId == tagId)
            .Select(l => _concepts.First(c => c.Id == l.ConceptId))
            .OrderBy(LevelSortKey)
            .ThenBy(c => c.Name)
            .ToList());

    public Task<Concept?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_concepts.FirstOrDefault(c => c.Id == id));

    public Task<Concept?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        Task.FromResult(_concepts.FirstOrDefault(
            c => c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Concept concept, CancellationToken cancellationToken = default)
    {
        _concepts.Add(concept);
        return Task.CompletedTask;
    }

    public void Remove(Concept concept) => _concepts.Remove(concept);

    public Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public Task<Concept?> GetByIdWithRelationsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var concept = _concepts.FirstOrDefault(c => c.Id == id);
        if (concept is null)
        {
            return Task.FromResult<Concept?>(null);
        }

        concept.ConceptNotes = _noteLinks
            .Where(l => l.ConceptId == id)
            .Select(l => new ConceptNote { ConceptId = id, NoteId = l.NoteId, Note = notes.Notes.First(n => n.Id == l.NoteId) })
            .ToList();

        concept.ConceptProjects = _projectLinks
            .Where(l => l.ConceptId == id)
            .Select(l => new ConceptProject { ConceptId = id, ProjectId = l.ProjectId, Project = projects.Projects.First(p => p.Id == l.ProjectId) })
            .ToList();

        concept.ConceptTags = _tagLinks
            .Where(l => l.ConceptId == id)
            .Select(l => new ConceptTag { ConceptId = id, TagId = l.TagId, Tag = tags.Tags.First(t => t.Id == l.TagId) })
            .ToList();

        concept.RelationsAsSource = _relations
            .Where(r => r.SourceId == id)
            .Select(r => new ConceptRelation { SourceConceptId = id, TargetConceptId = r.TargetId, Type = r.Type, TargetConcept = _concepts.First(c => c.Id == r.TargetId) })
            .ToList();

        concept.RelationsAsTarget = _relations
            .Where(r => r.TargetId == id)
            .Select(r => new ConceptRelation { SourceConceptId = r.SourceId, TargetConceptId = id, Type = r.Type, SourceConcept = _concepts.First(c => c.Id == r.SourceId) })
            .ToList();

        return Task.FromResult<Concept?>(concept);
    }

    public Task<bool> NoteLinkExistsAsync(Guid conceptId, Guid noteId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_noteLinks.Contains((conceptId, noteId)));

    public Task AddNoteLinkAsync(Guid conceptId, Guid noteId, CancellationToken cancellationToken = default)
    {
        _noteLinks.Add((conceptId, noteId));
        return Task.CompletedTask;
    }

    public Task<bool> RemoveNoteLinkAsync(Guid conceptId, Guid noteId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_noteLinks.Remove((conceptId, noteId)));

    public Task<bool> ProjectLinkExistsAsync(Guid conceptId, Guid projectId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_projectLinks.Contains((conceptId, projectId)));

    public Task AddProjectLinkAsync(Guid conceptId, Guid projectId, CancellationToken cancellationToken = default)
    {
        _projectLinks.Add((conceptId, projectId));
        return Task.CompletedTask;
    }

    public Task<bool> RemoveProjectLinkAsync(Guid conceptId, Guid projectId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_projectLinks.Remove((conceptId, projectId)));

    public Task<bool> TagLinkExistsAsync(Guid conceptId, Guid tagId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tagLinks.Contains((conceptId, tagId)));

    public Task AddTagLinkAsync(Guid conceptId, Guid tagId, CancellationToken cancellationToken = default)
    {
        _tagLinks.Add((conceptId, tagId));
        return Task.CompletedTask;
    }

    public Task<bool> RemoveTagLinkAsync(Guid conceptId, Guid tagId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tagLinks.Remove((conceptId, tagId)));

    public Task<bool> RelationExistsAsync(Guid conceptId, Guid relatedConceptId, ConceptRelationType type, CancellationToken cancellationToken = default) =>
        Task.FromResult(_relations.Any(r => r.Type == type &&
            ((r.SourceId == conceptId && r.TargetId == relatedConceptId) ||
             (r.SourceId == relatedConceptId && r.TargetId == conceptId))));

    public Task AddRelationAsync(Guid conceptId, Guid relatedConceptId, ConceptRelationType type, CancellationToken cancellationToken = default)
    {
        _relations.Add((conceptId, relatedConceptId, type));
        return Task.CompletedTask;
    }

    public Task<bool> RemoveRelationAsync(Guid conceptId, Guid relatedConceptId, ConceptRelationType type, CancellationToken cancellationToken = default)
    {
        var index = _relations.FindIndex(r => r.Type == type &&
            ((r.SourceId == conceptId && r.TargetId == relatedConceptId) ||
             (r.SourceId == relatedConceptId && r.TargetId == conceptId)));
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        _relations.RemoveAt(index);
        return Task.FromResult(true);
    }
}

using Microsoft.Extensions.Logging.Abstractions;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Exceptions;
using SecondBrain.Application.Services;
using SecondBrain.UnitTests.Fakes;
using Xunit;

namespace SecondBrain.UnitTests.Services;

public class NoteServiceTests
{
    private static (NoteService Service, FakeConceptRepository Concepts) CreateService()
    {
        var notesRepo = new FakeNoteRepository();
        var projectsRepo = new FakeProjectRepository();
        var tagsRepo = new FakeTagRepository();
        var conceptsRepo = new FakeConceptRepository(notesRepo, projectsRepo, tagsRepo);
        var service = new NoteService(notesRepo, conceptsRepo, NullLogger<NoteService>.Instance);

        return (service, conceptsRepo);
    }

    [Fact]
    public async Task CreateAsync_SemConceptIds_CriaNoteIsolada()
    {
        var (service, _) = CreateService();

        var note = await service.CreateAsync(new CreateNoteRequest { Title = "Redis Streams", Content = "..." });

        Assert.Equal("Redis Streams", note.Title);
    }

    [Fact]
    public async Task CreateAsync_ComConceptIdInexistente_LancaNotFoundException()
    {
        var (service, _) = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(
            new CreateNoteRequest { Title = "Nota", Content = "...", ConceptIds = [Guid.NewGuid()] }));
    }

    [Fact]
    public async Task GetByIdAsync_ComIdInexistente_LancaNotFoundException()
    {
        var (service, _) = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_AtualizaTitleEContent()
    {
        var (service, _) = CreateService();
        var created = await service.CreateAsync(new CreateNoteRequest { Title = "Old", Content = "Old content" });

        var updated = await service.UpdateAsync(created.Id, new UpdateNoteRequest { Title = "New", Content = "New content" });

        Assert.Equal("New", updated.Title);
        Assert.Equal("New content", updated.Content);
    }

    [Fact]
    public async Task DeleteAsync_ComIdInexistente_LancaNotFoundException()
    {
        var (service, _) = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }
}

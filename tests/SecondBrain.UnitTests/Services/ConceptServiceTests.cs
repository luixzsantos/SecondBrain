using Microsoft.Extensions.Logging.Abstractions;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Exceptions;
using SecondBrain.Application.Services;
using SecondBrain.Domain.Entities;
using SecondBrain.UnitTests.Fakes;
using Xunit;

namespace SecondBrain.UnitTests.Services;

public class ConceptServiceTests
{
    private sealed class Context
    {
        public required ConceptService Service { get; init; }
        public required FakeNoteRepository Notes { get; init; }
        public required FakeProjectRepository Projects { get; init; }
        public required FakeTagRepository Tags { get; init; }
    }

    private static Context CreateContext()
    {
        var notes = new FakeNoteRepository();
        var projects = new FakeProjectRepository();
        var tags = new FakeTagRepository();
        var concepts = new FakeConceptRepository(notes, projects, tags);
        var service = new ConceptService(concepts, notes, projects, tags, NullLogger<ConceptService>.Instance);

        return new Context { Service = service, Notes = notes, Projects = projects, Tags = tags };
    }

    [Fact]
    public async Task CreateAsync_ComNomeNovo_CriaConcept()
    {
        var ctx = CreateContext();

        var result = await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis", Description = "Cache" });

        Assert.Equal("Redis", result.Name);
        Assert.Equal("Cache", result.Description);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ComNomeDuplicado_LancaConflictException()
    {
        var ctx = CreateContext();
        await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" });

        await Assert.ThrowsAsync<ConflictException>(
            () => ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" }));
    }

    [Fact]
    public async Task CreateAsync_ComNomeDuplicadoDiferentesCaixas_LancaConflictException()
    {
        var ctx = CreateContext();
        await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" });

        await Assert.ThrowsAsync<ConflictException>(
            () => ctx.Service.CreateAsync(new CreateConceptRequest { Name = "REDIS" }));
    }

    [Fact]
    public async Task GetByIdAsync_ComIdInexistente_LancaNotFoundException()
    {
        var ctx = CreateContext();

        await Assert.ThrowsAsync<NotFoundException>(() => ctx.Service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_ComDadosValidos_AtualizaERetornaConcept()
    {
        var ctx = CreateContext();
        var created = await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis", Description = "Old" });

        var updated = await ctx.Service.UpdateAsync(created.Id, new UpdateConceptRequest { Name = "Redis", Description = "New" });

        Assert.Equal("New", updated.Description);
        Assert.True(updated.UpdatedAt >= created.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ParaNomeJaUsadoPorOutroConcept_LancaConflictException()
    {
        var ctx = CreateContext();
        await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" });
        var docker = await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Docker" });

        await Assert.ThrowsAsync<ConflictException>(
            () => ctx.Service.UpdateAsync(docker.Id, new UpdateConceptRequest { Name = "Redis" }));
    }

    [Fact]
    public async Task DeleteAsync_ComIdExistente_RemoveConcept()
    {
        var ctx = CreateContext();
        var created = await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" });

        await ctx.Service.DeleteAsync(created.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => ctx.Service.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_ComIdInexistente_LancaNotFoundException()
    {
        var ctx = CreateContext();

        await Assert.ThrowsAsync<NotFoundException>(() => ctx.Service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllAsync_RetornaConceptsOrdenadosPorNome()
    {
        var ctx = CreateContext();
        await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" });
        await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Docker" });

        var all = await ctx.Service.GetAllAsync();

        Assert.Equal(["Docker", "Redis"], all.Select(c => c.Name));
    }

    [Fact]
    public async Task GetAllByTagAsync_RetornaSoConceptsComEssaTag()
    {
        var ctx = CreateContext();
        var redis = await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" });
        await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Docker" });
        var tag = new Tag { Id = Guid.NewGuid(), Name = "database" };
        await ctx.Tags.AddAsync(tag);
        await ctx.Service.LinkTagAsync(redis.Id, tag.Id);

        var filtered = await ctx.Service.GetAllByTagAsync(tag.Id);

        Assert.Equal(["Redis"], filtered.Select(c => c.Name));
    }

    [Fact]
    public async Task GetAllByTagAsync_ComTagInexistente_LancaNotFoundException()
    {
        var ctx = CreateContext();

        await Assert.ThrowsAsync<NotFoundException>(() => ctx.Service.GetAllByTagAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task LinkNoteAsync_RelacionaNoteAoConcept_ApareceNoDetalhe()
    {
        var ctx = CreateContext();
        var concept = await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" });
        var note = new Note { Id = Guid.NewGuid(), Title = "Redis Streams", Content = "...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await ctx.Notes.AddAsync(note);

        await ctx.Service.LinkNoteAsync(concept.Id, note.Id);
        var detail = await ctx.Service.GetDetailByIdAsync(concept.Id);

        Assert.Single(detail.Notes);
        Assert.Equal("Redis Streams", detail.Notes[0].Title);
    }

    [Fact]
    public async Task LinkNoteAsync_ComConceptInexistente_LancaNotFoundException()
    {
        var ctx = CreateContext();
        var note = new Note { Id = Guid.NewGuid(), Title = "Nota", Content = "...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await ctx.Notes.AddAsync(note);

        await Assert.ThrowsAsync<NotFoundException>(() => ctx.Service.LinkNoteAsync(Guid.NewGuid(), note.Id));
    }

    [Fact]
    public async Task LinkNoteAsync_JaRelacionado_LancaConflictException()
    {
        var ctx = CreateContext();
        var concept = await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" });
        var note = new Note { Id = Guid.NewGuid(), Title = "Nota", Content = "...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await ctx.Notes.AddAsync(note);
        await ctx.Service.LinkNoteAsync(concept.Id, note.Id);

        await Assert.ThrowsAsync<ConflictException>(() => ctx.Service.LinkNoteAsync(concept.Id, note.Id));
    }

    [Fact]
    public async Task UnlinkNoteAsync_RemoveRelacao()
    {
        var ctx = CreateContext();
        var concept = await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" });
        var note = new Note { Id = Guid.NewGuid(), Title = "Nota", Content = "...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await ctx.Notes.AddAsync(note);
        await ctx.Service.LinkNoteAsync(concept.Id, note.Id);

        await ctx.Service.UnlinkNoteAsync(concept.Id, note.Id);
        var detail = await ctx.Service.GetDetailByIdAsync(concept.Id);

        Assert.Empty(detail.Notes);
    }

    [Fact]
    public async Task UnlinkNoteAsync_SemRelacaoExistente_LancaNotFoundException()
    {
        var ctx = CreateContext();
        var concept = await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" });

        await Assert.ThrowsAsync<NotFoundException>(() => ctx.Service.UnlinkNoteAsync(concept.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task LinkProjectAsync_RelacionaProjectAoConcept_ApareceNoDetalhe()
    {
        var ctx = CreateContext();
        var concept = await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" });
        var project = new Project { Id = Guid.NewGuid(), Name = "Notification Engine", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await ctx.Projects.AddAsync(project);

        await ctx.Service.LinkProjectAsync(concept.Id, project.Id);
        var detail = await ctx.Service.GetDetailByIdAsync(concept.Id);

        Assert.Single(detail.Projects);
        Assert.Equal("Notification Engine", detail.Projects[0].Name);
    }

    [Fact]
    public async Task LinkTagAsync_RelacionaTagAoConcept_ApareceNoDetalhe()
    {
        var ctx = CreateContext();
        var concept = await ctx.Service.CreateAsync(new CreateConceptRequest { Name = "Redis" });
        var tag = new Tag { Id = Guid.NewGuid(), Name = "database" };
        await ctx.Tags.AddAsync(tag);

        await ctx.Service.LinkTagAsync(concept.Id, tag.Id);
        var detail = await ctx.Service.GetDetailByIdAsync(concept.Id);

        Assert.Single(detail.Tags);
        Assert.Equal("database", detail.Tags[0].Name);
    }
}

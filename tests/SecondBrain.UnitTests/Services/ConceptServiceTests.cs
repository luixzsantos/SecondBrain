using Microsoft.Extensions.Logging.Abstractions;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Exceptions;
using SecondBrain.Application.Services;
using SecondBrain.UnitTests.Fakes;
using Xunit;

namespace SecondBrain.UnitTests.Services;

public class ConceptServiceTests
{
    private static ConceptService CreateService(out FakeConceptRepository repository)
    {
        repository = new FakeConceptRepository();
        return new ConceptService(repository, NullLogger<ConceptService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ComNomeNovo_CriaConcept()
    {
        var service = CreateService(out _);

        var result = await service.CreateAsync(new CreateConceptRequest { Name = "Redis", Description = "Cache" });

        Assert.Equal("Redis", result.Name);
        Assert.Equal("Cache", result.Description);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ComNomeDuplicado_LancaConflictException()
    {
        var service = CreateService(out _);
        await service.CreateAsync(new CreateConceptRequest { Name = "Redis" });

        await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(new CreateConceptRequest { Name = "Redis" }));
    }

    [Fact]
    public async Task CreateAsync_ComNomeDuplicadoDiferentesCaixas_LancaConflictException()
    {
        var service = CreateService(out _);
        await service.CreateAsync(new CreateConceptRequest { Name = "Redis" });

        await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(new CreateConceptRequest { Name = "REDIS" }));
    }

    [Fact]
    public async Task GetByIdAsync_ComIdInexistente_LancaNotFoundException()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_ComDadosValidos_AtualizaERetornaConcept()
    {
        var service = CreateService(out _);
        var created = await service.CreateAsync(new CreateConceptRequest { Name = "Redis", Description = "Old" });

        var updated = await service.UpdateAsync(created.Id, new UpdateConceptRequest { Name = "Redis", Description = "New" });

        Assert.Equal("New", updated.Description);
        Assert.True(updated.UpdatedAt >= created.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ParaNomeJaUsadoPorOutroConcept_LancaConflictException()
    {
        var service = CreateService(out _);
        await service.CreateAsync(new CreateConceptRequest { Name = "Redis" });
        var docker = await service.CreateAsync(new CreateConceptRequest { Name = "Docker" });

        await Assert.ThrowsAsync<ConflictException>(
            () => service.UpdateAsync(docker.Id, new UpdateConceptRequest { Name = "Redis" }));
    }

    [Fact]
    public async Task DeleteAsync_ComIdExistente_RemoveConcept()
    {
        var service = CreateService(out _);
        var created = await service.CreateAsync(new CreateConceptRequest { Name = "Redis" });

        await service.DeleteAsync(created.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_ComIdInexistente_LancaNotFoundException()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllAsync_RetornaConceptsOrdenadosPorNome()
    {
        var service = CreateService(out _);
        await service.CreateAsync(new CreateConceptRequest { Name = "Redis" });
        await service.CreateAsync(new CreateConceptRequest { Name = "Docker" });

        var all = await service.GetAllAsync();

        Assert.Equal(["Docker", "Redis"], all.Select(c => c.Name));
    }
}

using Microsoft.Extensions.Logging.Abstractions;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Exceptions;
using SecondBrain.Application.Services;
using SecondBrain.UnitTests.Fakes;
using Xunit;

namespace SecondBrain.UnitTests.Services;

public class TagServiceTests
{
    private static TagService CreateService(out FakeTagRepository repository)
    {
        repository = new FakeTagRepository();
        return new TagService(repository, NullLogger<TagService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ComNomeNovo_CriaTag()
    {
        var service = CreateService(out _);

        var tag = await service.CreateAsync(new CreateTagRequest { Name = "database" });

        Assert.Equal("database", tag.Name);
    }

    [Fact]
    public async Task CreateAsync_ComNomeDuplicado_LancaConflictException()
    {
        var service = CreateService(out _);
        await service.CreateAsync(new CreateTagRequest { Name = "backend" });

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(new CreateTagRequest { Name = "backend" }));
    }

    [Fact]
    public async Task DeleteAsync_ComIdInexistente_LancaNotFoundException()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllAsync_RetornaTagsOrdenadasPorNome()
    {
        var service = CreateService(out _);
        await service.CreateAsync(new CreateTagRequest { Name = "go" });
        await service.CreateAsync(new CreateTagRequest { Name = "csharp" });

        var all = await service.GetAllAsync();

        Assert.Equal(["csharp", "go"], all.Select(t => t.Name));
    }
}

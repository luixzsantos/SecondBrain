using Microsoft.Extensions.Logging.Abstractions;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Exceptions;
using SecondBrain.Application.Services;
using SecondBrain.Domain.Entities;
using SecondBrain.UnitTests.Fakes;
using Xunit;

namespace SecondBrain.UnitTests.Services;

public class ProjectServiceTests
{
    private static ProjectService CreateService(out FakeProjectRepository repository)
    {
        repository = new FakeProjectRepository();
        return new ProjectService(repository, NullLogger<ProjectService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ComDadosValidos_CriaProjectAtivo()
    {
        var service = CreateService(out _);

        var project = await service.CreateAsync(new CreateProjectRequest { Name = "SecondBrain" });

        Assert.Equal("SecondBrain", project.Name);
        Assert.Equal(ProjectStatus.Active, project.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ComIdInexistente_LancaNotFoundException()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_AtualizaStatus()
    {
        var service = CreateService(out _);
        var created = await service.CreateAsync(new CreateProjectRequest { Name = "SecondBrain" });

        var updated = await service.UpdateAsync(created.Id,
            new UpdateProjectRequest { Name = "SecondBrain", Status = ProjectStatus.Paused });

        Assert.Equal(ProjectStatus.Paused, updated.Status);
    }

    [Fact]
    public async Task DeleteAsync_ComIdInexistente_LancaNotFoundException()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }
}

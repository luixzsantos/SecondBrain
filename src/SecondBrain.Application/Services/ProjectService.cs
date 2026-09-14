using Microsoft.Extensions.Logging;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Exceptions;
using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.Services;

public class ProjectService(IProjectRepository repository, ILogger<ProjectService> logger) : IProjectService
{
    public async Task<List<ProjectDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var projects = await repository.GetAllAsync(cancellationToken);
        return projects.Select(ToDto).ToList();
    }

    public async Task<ProjectDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Project '{id}' não encontrado.");

        return ToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Status = request.Status,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await repository.AddAsync(project, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Project criado: {ProjectId} ({Name})", project.Id, project.Name);

        return ToDto(project);
    }

    public async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Project '{id}' não encontrado.");

        project.Name = request.Name.Trim();
        project.Description = request.Description?.Trim();
        project.Status = request.Status;
        project.UpdatedAt = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);

        return ToDto(project);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Project '{id}' não encontrado.");

        repository.Remove(project);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private static ProjectDto ToDto(Project project) =>
        new(project.Id, project.Name, project.Description, project.Status, project.CreatedAt, project.UpdatedAt);
}

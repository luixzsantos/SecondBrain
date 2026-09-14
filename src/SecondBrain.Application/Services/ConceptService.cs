using Microsoft.Extensions.Logging;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Exceptions;
using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.Services;

public class ConceptService(IConceptRepository repository, ILogger<ConceptService> logger) : IConceptService
{
    public async Task<List<ConceptDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var concepts = await repository.GetAllAsync(cancellationToken);
        return concepts.Select(ToDto).ToList();
    }

    public async Task<ConceptDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var concept = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Concept '{id}' não encontrado.");

        return ToDto(concept);
    }

    public async Task<ConceptDto> CreateAsync(CreateConceptRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetByNameAsync(request.Name, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException($"Já existe um concept com o nome '{request.Name}'.");
        }

        var now = DateTime.UtcNow;
        var concept = new Concept
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        await repository.AddAsync(concept, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Concept criado: {ConceptId} ({ConceptName})", concept.Id, concept.Name);

        return ToDto(concept);
    }

    public async Task<ConceptDto> UpdateAsync(Guid id, UpdateConceptRequest request, CancellationToken cancellationToken = default)
    {
        var concept = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Concept '{id}' não encontrado.");

        var existing = await repository.GetByNameAsync(request.Name, cancellationToken);
        if (existing is not null && existing.Id != id)
        {
            throw new ConflictException($"Já existe um concept com o nome '{request.Name}'.");
        }

        concept.Name = request.Name.Trim();
        concept.Description = request.Description?.Trim();
        concept.UpdatedAt = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Concept atualizado: {ConceptId}", concept.Id);

        return ToDto(concept);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var concept = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Concept '{id}' não encontrado.");

        repository.Remove(concept);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Concept removido: {ConceptId}", id);
    }

    private static ConceptDto ToDto(Concept concept) => new(
        concept.Id,
        concept.Name,
        concept.Description,
        concept.CreatedAt,
        concept.UpdatedAt
    );
}

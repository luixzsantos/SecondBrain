using Microsoft.Extensions.Logging;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Exceptions;
using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.Services;

public class TagService(ITagRepository repository, ILogger<TagService> logger) : ITagService
{
    public async Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tags = await repository.GetAllAsync(cancellationToken);
        return tags.Select(ToDto).ToList();
    }

    public async Task<TagDto> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var existing = await repository.GetByNameAsync(name, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException($"Já existe uma tag com o nome '{name}'.");
        }

        var tag = new Tag { Id = Guid.NewGuid(), Name = name };
        await repository.AddAsync(tag, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Tag criada: {TagId} ({Name})", tag.Id, tag.Name);

        return ToDto(tag);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Tag '{id}' não encontrada.");

        repository.Remove(tag);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private static TagDto ToDto(Tag tag) => new(tag.Id, tag.Name);
}

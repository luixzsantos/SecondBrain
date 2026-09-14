using Microsoft.Extensions.Logging;
using SecondBrain.Application.DTOs;
using SecondBrain.Application.Exceptions;
using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.Services;

public class NoteService(
    INoteRepository repository,
    IConceptRepository conceptRepository,
    ILogger<NoteService> logger) : INoteService
{
    public async Task<List<NoteDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var notes = await repository.GetAllAsync(cancellationToken);
        return notes.Select(ToDto).ToList();
    }

    public async Task<NoteDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Note '{id}' não encontrada.");

        return ToDto(note);
    }

    public async Task<NoteDto> CreateAsync(CreateNoteRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Content = request.Content,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await repository.AddAsync(note, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        foreach (var conceptId in request.ConceptIds.Distinct())
        {
            var concept = await conceptRepository.GetByIdAsync(conceptId, cancellationToken)
                ?? throw new NotFoundException($"Concept '{conceptId}' não encontrado.");

            await conceptRepository.AddNoteLinkAsync(concept.Id, note.Id, cancellationToken);
        }

        await conceptRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Note criada: {NoteId} ({Title})", note.Id, note.Title);

        return ToDto(note);
    }

    public async Task<NoteDto> UpdateAsync(Guid id, UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        var note = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Note '{id}' não encontrada.");

        note.Title = request.Title.Trim();
        note.Content = request.Content;
        note.UpdatedAt = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);

        return ToDto(note);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Note '{id}' não encontrada.");

        repository.Remove(note);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Note removida: {NoteId}", id);
    }

    private static NoteDto ToDto(Note note) => new(note.Id, note.Title, note.Content, note.CreatedAt, note.UpdatedAt);
}

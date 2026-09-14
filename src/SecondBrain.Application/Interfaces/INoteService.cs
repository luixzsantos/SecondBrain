using SecondBrain.Application.DTOs;

namespace SecondBrain.Application.Interfaces;

public interface INoteService
{
    Task<List<NoteDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<NoteDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NoteDto> CreateAsync(CreateNoteRequest request, CancellationToken cancellationToken = default);
    Task<NoteDto> UpdateAsync(Guid id, UpdateNoteRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

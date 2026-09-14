using SecondBrain.Application.DTOs;

namespace SecondBrain.Application.Interfaces;

public interface ITagService
{
    Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TagDto> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

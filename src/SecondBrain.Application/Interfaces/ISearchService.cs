using SecondBrain.Application.DTOs;

namespace SecondBrain.Application.Interfaces;

public interface ISearchService
{
    Task<SearchResponse> SearchAsync(string query, CancellationToken cancellationToken = default);
}

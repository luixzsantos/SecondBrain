using SecondBrain.Application.DTOs;
using SecondBrain.Application.Interfaces;

namespace SecondBrain.Application.Services;

public class SearchService(ISearchRepository repository) : ISearchService
{
    public async Task<SearchResponse> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var trimmed = query.Trim();
        if (trimmed.Length == 0)
        {
            return new SearchResponse(query, []);
        }

        var concepts = await repository.SearchConceptsAsync(trimmed, cancellationToken);
        var notes = await repository.SearchNotesAsync(trimmed, cancellationToken);
        var projects = await repository.SearchProjectsAsync(trimmed, cancellationToken);

        var results = new List<SearchResultDto>();
        results.AddRange(concepts.Select(c => new SearchResultDto("concept", c.Id, c.Name)));
        results.AddRange(notes.Select(n => new SearchResultDto("note", n.Id, n.Title)));
        results.AddRange(projects.Select(p => new SearchResultDto("project", p.Id, p.Name)));

        return new SearchResponse(query, results);
    }
}

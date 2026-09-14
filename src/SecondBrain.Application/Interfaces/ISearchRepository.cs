using SecondBrain.Domain.Entities;

namespace SecondBrain.Application.Interfaces;

// Busca full-text (Postgres tsvector/tsquery) espalhada pelas três tabelas.
// Ver decisão de não usar coluna gerada + índice GIN ainda (V0.2): calcula o
// tsvector em tempo de consulta — funciona correto, otimiza depois se doer (dataset pessoal, baixo volume).
public interface ISearchRepository
{
    Task<List<Concept>> SearchConceptsAsync(string query, CancellationToken cancellationToken = default);
    Task<List<Note>> SearchNotesAsync(string query, CancellationToken cancellationToken = default);
    Task<List<Project>> SearchProjectsAsync(string query, CancellationToken cancellationToken = default);
}

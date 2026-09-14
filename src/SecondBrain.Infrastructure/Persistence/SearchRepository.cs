using Microsoft.EntityFrameworkCore;
using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.Infrastructure.Persistence;

// Full-text search nativo do Postgres (tsvector/tsquery), calculado em tempo de consulta —
// sem coluna gerada + índice GIN ainda (ver nota de decisão/aprendizado: dataset pessoal,
// baixo volume, não vale a complexidade extra até isso doer de verdade).
// Config "simple" (sem stemming) em vez de "portuguese": o dicionário é bilíngue
// (termos técnicos em inglês + texto em português) e stemming de português aplicado a
// palavras em inglês dava resultado imprevisível.
public class SearchRepository(SecondBrainDbContext context) : ISearchRepository
{
    private const string TsConfig = "simple";

    public async Task<List<Concept>> SearchConceptsAsync(string query, CancellationToken cancellationToken = default) =>
        await context.Concepts
            .AsNoTracking()
            .Where(c => EF.Functions.ToTsVector(TsConfig, c.Name + " " + (c.Description ?? ""))
                .Matches(EF.Functions.PlainToTsQuery(TsConfig, query)))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<List<Note>> SearchNotesAsync(string query, CancellationToken cancellationToken = default) =>
        await context.Notes
            .AsNoTracking()
            .Where(n => EF.Functions.ToTsVector(TsConfig, n.Title + " " + n.Content)
                .Matches(EF.Functions.PlainToTsQuery(TsConfig, query)))
            .OrderBy(n => n.Title)
            .ToListAsync(cancellationToken);

    public async Task<List<Project>> SearchProjectsAsync(string query, CancellationToken cancellationToken = default) =>
        await context.Projects
            .AsNoTracking()
            .Where(p => EF.Functions.ToTsVector(TsConfig, p.Name + " " + (p.Description ?? ""))
                .Matches(EF.Functions.PlainToTsQuery(TsConfig, query)))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
}

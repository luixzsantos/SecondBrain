using SecondBrain.Application.Interfaces;
using SecondBrain.Domain.Entities;

namespace SecondBrain.UnitTests.Fakes;

// Repositório em memória — evita subir Postgres/EF nos testes unitários e evita
// depender de uma lib de mock pra um contrato simples como este.
public class FakeConceptRepository : IConceptRepository
{
    private readonly List<Concept> _concepts = [];

    public Task<List<Concept>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_concepts.OrderBy(c => c.Name).ToList());

    public Task<Concept?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_concepts.FirstOrDefault(c => c.Id == id));

    public Task<Concept?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        Task.FromResult(_concepts.FirstOrDefault(
            c => c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Concept concept, CancellationToken cancellationToken = default)
    {
        _concepts.Add(concept);
        return Task.CompletedTask;
    }

    public void Remove(Concept concept) => _concepts.Remove(concept);

    public Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
}

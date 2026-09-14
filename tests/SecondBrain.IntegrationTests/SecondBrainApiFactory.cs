using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SecondBrain.Infrastructure.Persistence;

namespace SecondBrain.IntegrationTests;

// Sobe a API real (Program.cs), mas troca o Postgres por EF Core InMemory —
// evita depender de um banco de verdade nos testes de integração da API.
// Limitação conhecida: InMemory não valida tudo que o Postgres validaria
// (ex: constraints de unicidade não são garantidas da mesma forma), então
// isso cobre o fluxo HTTP/serialização/DI, não a camada de banco em si.
public class SecondBrainApiFactory : WebApplicationFactory<Program>
{
    // Gerado uma única vez por factory (não dentro da lambda abaixo!) — AddDbContext
    // reconfigura as options a cada novo scope (cada request HTTP), então um
    // Guid.NewGuid() ali dentro criaria um banco InMemory diferente por request.
    private readonly string _databaseName = $"secondbrain-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<SecondBrainDbContext>>();
            services.AddDbContext<SecondBrainDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}

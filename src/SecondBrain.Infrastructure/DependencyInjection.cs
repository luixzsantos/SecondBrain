using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecondBrain.Application.Interfaces;
using SecondBrain.Infrastructure.Persistence;

namespace SecondBrain.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' não configurada (appsettings ou variável de ambiente ConnectionStrings__DefaultConnection).");

        services.AddDbContext<SecondBrainDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IConceptRepository, ConceptRepository>();

        return services;
    }
}

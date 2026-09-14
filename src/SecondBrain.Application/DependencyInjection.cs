using Microsoft.Extensions.DependencyInjection;
using SecondBrain.Application.Interfaces;
using SecondBrain.Application.Services;

namespace SecondBrain.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IConceptService, ConceptService>();
        return services;
    }
}

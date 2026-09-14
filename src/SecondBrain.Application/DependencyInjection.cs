using Microsoft.Extensions.DependencyInjection;
using SecondBrain.Application.Interfaces;
using SecondBrain.Application.Services;

namespace SecondBrain.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IConceptService, ConceptService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ISearchService, SearchService>();
        return services;
    }
}

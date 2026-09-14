using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SecondBrain.Application.DTOs;
using SecondBrain.Domain.Entities;
using Xunit;

namespace SecondBrain.IntegrationTests;

public class ProjectsControllerTests(SecondBrainApiFactory factory) : IClassFixture<SecondBrainApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = TestJson.Options;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PostThenGet_FluxoCompletoDeCriarEBuscarProject()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/projects",
            new CreateProjectRequest { Name = "SecondBrain", Description = "Dicionário técnico pessoal" });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>(JsonOptions);
        Assert.Equal(ProjectStatus.Active, created!.Status);

        var getResponse = await _client.GetAsync($"/api/projects/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Put_AtualizaStatusParaArchived()
    {
        var created = await (await _client.PostAsJsonAsync("/api/projects", new CreateProjectRequest { Name = "Projeto X" }))
            .Content.ReadFromJsonAsync<ProjectDto>(JsonOptions);

        var updateResponse = await _client.PutAsJsonAsync($"/api/projects/{created!.Id}",
            new UpdateProjectRequest { Name = "Projeto X", Status = ProjectStatus.Archived });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProjectDto>(JsonOptions);
        Assert.Equal(ProjectStatus.Archived, updated!.Status);
    }
}

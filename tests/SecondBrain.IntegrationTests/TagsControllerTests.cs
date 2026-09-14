using System.Net;
using System.Net.Http.Json;
using SecondBrain.Application.DTOs;
using Xunit;

namespace SecondBrain.IntegrationTests;

public class TagsControllerTests(SecondBrainApiFactory factory) : IClassFixture<SecondBrainApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_ComNomeNovo_Retorna201()
    {
        var response = await _client.PostAsJsonAsync("/api/tags", new CreateTagRequest { Name = "database" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComNomeDuplicado_Retorna409()
    {
        await _client.PostAsJsonAsync("/api/tags", new CreateTagRequest { Name = "backend" });

        var response = await _client.PostAsJsonAsync("/api/tags", new CreateTagRequest { Name = "backend" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ComIdInexistente_Retorna404()
    {
        var response = await _client.DeleteAsync($"/api/tags/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

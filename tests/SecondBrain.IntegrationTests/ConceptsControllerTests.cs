using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SecondBrain.Application.DTOs;
using Xunit;

namespace SecondBrain.IntegrationTests;

public class ConceptsControllerTests(SecondBrainApiFactory factory) : IClassFixture<SecondBrainApiFactory>
{
    // A API serializa em camelCase ("id", "name"...); sem isso, ReadFromJsonAsync
    // (case-sensitive por padrão) não casa "id" com a propriedade "Id" do record.
    private static readonly JsonSerializerOptions JsonOptions = TestJson.Options;

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PostThenGet_FluxoCompletoDeCriarEBuscarConcept()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/concepts",
            new CreateConceptRequest { Name = "Goroutines", Description = "Concorrencia leve em Go" });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ConceptDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);

        var getResponse = await _client.GetAsync($"/api/concepts/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ConceptDto>(JsonOptions);
        Assert.Equal("Goroutines", fetched!.Name);
    }

    [Fact]
    public async Task GetById_ComIdInexistente_Retorna404()
    {
        var response = await _client.GetAsync($"/api/concepts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComNomeVazio_Retorna400()
    {
        var response = await _client.PostAsJsonAsync("/api/concepts", new CreateConceptRequest { Name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteThenGet_RemoveConceptCorretamente()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/concepts", new CreateConceptRequest { Name = "Docker" });
        var created = await createResponse.Content.ReadFromJsonAsync<ConceptDto>(JsonOptions);
        Assert.NotEqual(Guid.Empty, created!.Id);

        var deleteResponse = await _client.DeleteAsync($"/api/concepts/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/concepts/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SecondBrain.Application.DTOs;
using Xunit;

namespace SecondBrain.IntegrationTests;

public class NotesControllerTests(SecondBrainApiFactory factory) : IClassFixture<SecondBrainApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = TestJson.Options;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PostThenGet_FluxoCompletoDeCriarEBuscarNote()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/notes",
            new CreateNoteRequest { Title = "Redis Streams na prática", Content = "..." });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<NoteDto>(JsonOptions);

        var getResponse = await _client.GetAsync($"/api/notes/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Post_ComConceptIdInexistente_Retorna404()
    {
        var response = await _client.PostAsJsonAsync("/api/notes",
            new CreateNoteRequest { Title = "Nota", Content = "...", ConceptIds = [Guid.NewGuid()] });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComTitleVazio_Retorna400()
    {
        var response = await _client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Title = "", Content = "..." });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SecondBrain.Application.DTOs;
using SecondBrain.Domain.Entities;
using Xunit;

namespace SecondBrain.IntegrationTests;

public class ConceptRelationsTests(SecondBrainApiFactory factory) : IClassFixture<SecondBrainApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = TestJson.Options;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task LinkNoteProjectTag_AparecemNaPaginaDoConceito()
    {
        var concept = await CreateAsync<ConceptDto>("/api/concepts", new CreateConceptRequest { Name = "Redis" });
        var note = await CreateAsync<NoteDto>("/api/notes", new CreateNoteRequest { Title = "Redis Streams", Content = "..." });
        var project = await CreateAsync<ProjectDto>("/api/projects", new CreateProjectRequest { Name = "Notification Engine" });
        var tag = await CreateAsync<TagDto>("/api/tags", new CreateTagRequest { Name = "database" });

        Assert.Equal(HttpStatusCode.NoContent, (await _client.PostAsync($"/api/concepts/{concept.Id}/notes/{note.Id}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.PostAsync($"/api/concepts/{concept.Id}/projects/{project.Id}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.PostAsync($"/api/concepts/{concept.Id}/tags/{tag.Id}", null)).StatusCode);

        var detail = await (await _client.GetAsync($"/api/concepts/{concept.Id}")).Content.ReadFromJsonAsync<ConceptDetailDto>(JsonOptions);

        Assert.Single(detail!.Notes);
        Assert.Single(detail.Projects);
        Assert.Single(detail.Tags);
        Assert.Equal("Redis Streams", detail.Notes[0].Title);
    }

    [Fact]
    public async Task LinkNote_Duplicado_Retorna409()
    {
        var concept = await CreateAsync<ConceptDto>("/api/concepts", new CreateConceptRequest { Name = "Docker" });
        var note = await CreateAsync<NoteDto>("/api/notes", new CreateNoteRequest { Title = "Docker Compose", Content = "..." });

        await _client.PostAsync($"/api/concepts/{concept.Id}/notes/{note.Id}", null);
        var response = await _client.PostAsync($"/api/concepts/{concept.Id}/notes/{note.Id}", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UnlinkNote_RemoveDaPaginaDoConceito()
    {
        var concept = await CreateAsync<ConceptDto>("/api/concepts", new CreateConceptRequest { Name = "PostgreSQL" });
        var note = await CreateAsync<NoteDto>("/api/notes", new CreateNoteRequest { Title = "Full-text search", Content = "..." });
        await _client.PostAsync($"/api/concepts/{concept.Id}/notes/{note.Id}", null);

        var unlinkResponse = await _client.DeleteAsync($"/api/concepts/{concept.Id}/notes/{note.Id}");
        Assert.Equal(HttpStatusCode.NoContent, unlinkResponse.StatusCode);

        var detail = await (await _client.GetAsync($"/api/concepts/{concept.Id}")).Content.ReadFromJsonAsync<ConceptDetailDto>(JsonOptions);
        Assert.Empty(detail!.Notes);
    }

    [Fact]
    public async Task LinkNote_ComNoteInexistente_Retorna404()
    {
        var concept = await CreateAsync<ConceptDto>("/api/concepts", new CreateConceptRequest { Name = "Go" });

        var response = await _client.PostAsync($"/api/concepts/{concept.Id}/notes/{Guid.NewGuid()}", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllConcepts_ComTagId_RetornaSoOsRelacionados()
    {
        var tagged = await CreateAsync<ConceptDto>("/api/concepts", new CreateConceptRequest { Name = "Kubernetes" });
        await CreateAsync<ConceptDto>("/api/concepts", new CreateConceptRequest { Name = "Terraform" });
        var tag = await CreateAsync<TagDto>("/api/tags", new CreateTagRequest { Name = "devops" });
        await _client.PostAsync($"/api/concepts/{tagged.Id}/tags/{tag.Id}", null);

        var filtered = await _client.GetFromJsonAsync<List<ConceptDto>>($"/api/concepts?tagId={tag.Id}", JsonOptions);

        Assert.Single(filtered!);
        Assert.Equal("Kubernetes", filtered![0].Name);
    }

    [Fact]
    public async Task GetAllConcepts_ComTagIdInexistente_Retorna404()
    {
        var response = await _client.GetAsync($"/api/concepts?tagId={Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LinkRelation_ApareceNaPaginaDosDoisConceitos()
    {
        var redis = await CreateAsync<ConceptDto>("/api/concepts", new CreateConceptRequest { Name = "Redis (relação)" });
        var streams = await CreateAsync<ConceptDto>("/api/concepts", new CreateConceptRequest { Name = "Redis Streams (relação)" });

        var linkResponse = await _client.PostAsJsonAsync(
            $"/api/concepts/{redis.Id}/relations/{streams.Id}", new LinkConceptRelationRequest { Type = ConceptRelationType.RelatedTo });
        Assert.Equal(HttpStatusCode.NoContent, linkResponse.StatusCode);

        var redisDetail = await (await _client.GetAsync($"/api/concepts/{redis.Id}")).Content.ReadFromJsonAsync<ConceptDetailDto>(JsonOptions);
        var streamsDetail = await (await _client.GetAsync($"/api/concepts/{streams.Id}")).Content.ReadFromJsonAsync<ConceptDetailDto>(JsonOptions);

        Assert.Single(redisDetail!.Relations);
        Assert.Equal("Redis Streams (relação)", redisDetail.Relations[0].ConceptName);
        Assert.Single(streamsDetail!.Relations);
        Assert.Equal("Redis (relação)", streamsDetail.Relations[0].ConceptName);
    }

    [Fact]
    public async Task LinkRelation_ComSiMesmo_Retorna409()
    {
        var concept = await CreateAsync<ConceptDto>("/api/concepts", new CreateConceptRequest { Name = "Go (relação)" });

        var response = await _client.PostAsJsonAsync(
            $"/api/concepts/{concept.Id}/relations/{concept.Id}", new LinkConceptRelationRequest { Type = ConceptRelationType.RelatedTo });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UnlinkRelation_RemoveDosDoisLados()
    {
        var a = await CreateAsync<ConceptDto>("/api/concepts", new CreateConceptRequest { Name = "Kafka (relação)" });
        var b = await CreateAsync<ConceptDto>("/api/concepts", new CreateConceptRequest { Name = "RabbitMQ (relação)" });
        await _client.PostAsJsonAsync($"/api/concepts/{a.Id}/relations/{b.Id}", new LinkConceptRelationRequest { Type = ConceptRelationType.AlternativeTo });

        var unlinkResponse = await _client.DeleteAsync($"/api/concepts/{b.Id}/relations/{a.Id}?type=AlternativeTo");
        Assert.Equal(HttpStatusCode.NoContent, unlinkResponse.StatusCode);

        var detail = await (await _client.GetAsync($"/api/concepts/{a.Id}")).Content.ReadFromJsonAsync<ConceptDetailDto>(JsonOptions);
        Assert.Empty(detail!.Relations);
    }

    private async Task<T> CreateAsync<T>(string url, object body)
    {
        var response = await _client.PostAsJsonAsync(url, body);
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return result!;
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SecondBrain.IntegrationTests;

// Opções de desserialização pros testes: a API serializa em camelCase e enums como
// string (ver Program.cs) — sem isso, ReadFromJsonAsync não casa "id" com "Id" e
// falha ao desserializar um ProjectStatus vindo como "Active" em vez de número.
internal static class TestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}

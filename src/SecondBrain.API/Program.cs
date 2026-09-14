using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using SecondBrain.API.Middleware;
using SecondBrain.Application;
using SecondBrain.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    // Enums (ex: ProjectStatus) como texto ("Active") em vez de número no JSON —
    // número puro é opaco pra quem consome a API e frágil se a ordem do enum mudar.
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SecondBrain API",
        Version = "v1",
        Description = "Dicionário técnico pessoal + mapa de conhecimento.",
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SecondBrain API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Necessário para o WebApplicationFactory<Program> usado pelos testes de integração.
public partial class Program;

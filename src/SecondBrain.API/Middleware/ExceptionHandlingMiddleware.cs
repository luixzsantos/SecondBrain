using System.Net;
using System.Text.Json;
using SecondBrain.Application.Exceptions;

namespace SecondBrain.API.Middleware;

// Middleware global de erro: traduz exceções de Application em respostas HTTP consistentes
// e evita vazar stack trace / detalhe interno pro cliente (ver princípio de segurança do projeto).
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, title) = ex switch
            {
                NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                ConflictException => (HttpStatusCode.Conflict, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado."),
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                logger.LogError(ex, "Erro não tratado ao processar {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            else
            {
                logger.LogWarning("{ExceptionType}: {Message}", ex.GetType().Name, ex.Message);
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var problem = new
            {
                status = (int)statusCode,
                title,
                traceId = context.TraceIdentifier,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}

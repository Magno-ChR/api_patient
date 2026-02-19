using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using patient.domain.Results;

namespace api_patient.Middleware;

/// <summary>
/// Devuelve respuestas de error (403 Forbidden, etc.) con el formato Result/Error en el cuerpo.
/// </summary>
public class ResultFormatMiddleware : IAuthorizationMiddlewareResultHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var error = new Error("Auth.Forbidden", "No tiene permisos para acceder a este recurso.", ErrorType.Forbidden);
            var body = Result.Failure(error);
            await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}

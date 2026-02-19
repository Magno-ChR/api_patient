using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using patient.domain.Results;

namespace patient.infrastructure.Extensions;

public static class AuthenticationExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Registra la autenticación JWT Bearer para validar tokens emitidos por la API de seguridad.
    /// No registra generación de tokens. Si no hay Secret configurado, no hace nada.
    /// 401 devuelve el formato Result/Error en el cuerpo.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration["Jwt:Secret"] ?? configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(secret))
            return services;

        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
            };
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    var error = new Error("Auth.Unauthorized", "Token no proporcionado o no válido.", ErrorType.Unauthorized);
                    var body = Result.Failure(error);
                    var json = JsonSerializer.Serialize(body, JsonOptions);
                    await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(json));
                },
                OnAuthenticationFailed = async context =>
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    var error = new Error("Auth.Unauthorized", "Token inválido o expirado.", ErrorType.Unauthorized);
                    var body = Result.Failure(error);
                    var json = JsonSerializer.Serialize(body, JsonOptions);
                    await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(json));
                }
            };
        });

        return services;
    }
}

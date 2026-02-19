namespace patient.infrastructure.Security;

/// <summary>
/// Opciones de configuración JWT para validación del token emitido por la API de seguridad.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
}

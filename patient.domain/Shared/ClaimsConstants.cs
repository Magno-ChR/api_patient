namespace patient.domain.Shared;

/// <summary>
/// Nombres de claims usados en el token JWT emitido por la API de seguridad.
/// Usar estas constantes para leer uid, pid y no hardcodear cadenas.
/// </summary>
public static class ClaimsConstants
{
    /// <summary>Claim del identificador del usuario (Guid en string).</summary>
    public const string UserId = "uid";

    /// <summary>Claim del identificador de la persona/paciente (Guid en string).</summary>
    public const string PersonId = "pid";

    /// <summary>Nombre del claim de rol. Los roles en el token van en ClaimTypes.Role (patient, doctor, admin, delivery).</summary>
    public const string Role = "role";
}

namespace Infrastructure.Utilities;

public class JwtSettings
{
    /// <summary>
    /// The issuer of the token (who issues the token).
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The audience of the token (who the token is intended for).
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// The secret key used to sign the token.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token expiry duration in minutes.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60; // Default to 60 minutes
}

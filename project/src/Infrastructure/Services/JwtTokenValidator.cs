using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Domain.Models;
using Infrastructure.Utilities;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class JwtTokenValidator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenValidator(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
    }

    public Result<object> ValidateJwtToken(string jwt)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        // โหลด Public Key
        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(File.ReadAllBytes(RsaKeyPaths.PublicKeyPath), out _);
        var publicKey = new RsaSecurityKey(rsa);

        try
        {
            tokenHandler.ValidateToken(
                jwt,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = publicKey,
                },
                out _
            );

            return Result<object>.Success("JWT is valid.");
        }
        catch (Exception ex)
        {
            return Result<object>.Failure($"JWT validation failed: {ex.Message}");
        }
    }
}

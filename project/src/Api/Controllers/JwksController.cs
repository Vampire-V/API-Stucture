using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middleware;

[ApiController]
[Route("api/[controller]")]
public class JwksController : ControllerBase
{
    private readonly RsaKeyService _rsaKeyService;

    public JwksController(RsaKeyService rsaKeyService)
    {
        _rsaKeyService = rsaKeyService;
    }

    /// <summary>
    /// Exposes the Public Key in JWKS format.
    /// </summary>
    /// <returns>JWKS containing the public key information</returns>
    [HttpGet("publish-key")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // ดึงข้อมูล publish key สําเร็จ
    public IActionResult GetJWKS()
    {
        try
        {
            using var rsa = _rsaKeyService.LoadPublicKey(); // Use the RsaKeyService to get the public key

            var parameters = rsa.ExportParameters(false);

            // Build the JWKS structure
            var jwk = new
            {
                kty = "RSA", // Key Type
                kid = "RSA-1", // Key Identifier (unique for key rotation)
                use = "sig", // Public Key Use (e.g., signing)
                alg = "RS256", // Algorithm
                n = Base64UrlEncode(parameters.Modulus!),
                e = Base64UrlEncode(parameters.Exponent!),
            };

            return Ok(new { keys = new[] { jwk } });
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new { message = "An error occurred while generating JWKS.", error = ex.Message }
            );
        }
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}

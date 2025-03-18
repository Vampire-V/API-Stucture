using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "RefreshToken is required.")]
    public required string RefreshToken { get; set; }
}

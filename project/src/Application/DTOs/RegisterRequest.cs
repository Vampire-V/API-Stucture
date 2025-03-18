using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(
        100,
        MinimumLength = 6,
        ErrorMessage = "Password must be between 6 and 100 characters."
    )]
    public required string Password { get; set; }

    [Required(ErrorMessage = "ConfirmPassword is required.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public required string ConfirmPassword { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}

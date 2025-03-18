using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class User : BaseEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public required string Email { get; set; }
    public bool IsLockedOut { get; set; } = false;

    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public required string PasswordHash { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime? RefreshTokenExpiry { get; set; }

    public ICollection<Role> Roles { get; set; } = new List<Role>();
}

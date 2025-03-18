using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Role : BaseEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public required string Name { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}

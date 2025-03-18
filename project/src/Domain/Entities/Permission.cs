using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Permission : BaseEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public required string Name { get; set; }
}

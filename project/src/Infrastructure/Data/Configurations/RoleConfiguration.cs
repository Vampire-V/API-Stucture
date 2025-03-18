using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public static class RoleConfiguration
{
    public static void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();

        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);

        builder
            .HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity(j => j.ToTable("role_permissions"));
    }
}

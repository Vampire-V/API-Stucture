using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public static class PermissionConfiguration
{
    public static void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
    }
}

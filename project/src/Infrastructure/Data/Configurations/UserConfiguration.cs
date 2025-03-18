using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public static class UserConfiguration
{
    public static void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd(); // กำหนดให้ EF Core สร้าง Guid โดยอัตโนมัติ

        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);

        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.IsLockedOut).HasDefaultValue(false);
        builder
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity(j => j.ToTable("user_roles")); // Pivot Table ยังต้องระบุชื่อ
    }
}

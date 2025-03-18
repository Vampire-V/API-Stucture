using Domain.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Users = Set<User>();
        Roles = Set<Role>();
        Permissions = Set<Permission>();
    }

    // DbSet สำหรับ Entity ต่าง ๆ
    public DbSet<User> Users { get; private set; }
    public DbSet<Role> Roles { get; private set; }
    public DbSet<Permission> Permissions { get; private set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Apply Snake Case Naming Convention to all Tables and Columns
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Convert Table Name to Snake Case
            entity.SetTableName(entity.GetTableName()?.ToSnakeCase());

            // Convert Column Names to Snake Case
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.Name.ToSnakeCase());
            }

            // Convert Foreign Keys to Snake Case
            foreach (var key in entity.GetForeignKeys())
            {
                foreach (var property in key.Properties)
                {
                    property.SetColumnName(property.Name.ToSnakeCase());
                }
            }
        }

        // Apply Global Configuration for BaseEntity
        modelBuilder
            .Model.GetEntityTypes()
            .Where(entityType => typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            .Select(entityType => entityType.ClrType)
            .ToList()
            .ForEach(entityType =>
            {
                modelBuilder
                    .Entity(entityType)
                    .Property(nameof(BaseEntity.CreateDate))
                    .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
                    .HasColumnName("create_date");

                modelBuilder
                    .Entity(entityType)
                    .Property(nameof(BaseEntity.UpdateDate))
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
                    .HasColumnName("update_date");
            });

        // Apply Static Configurations
        Configurations.UserConfiguration.Configure(modelBuilder.Entity<User>());
        Configurations.RoleConfiguration.Configure(modelBuilder.Entity<Role>());
        Configurations.PermissionConfiguration.Configure(modelBuilder.Entity<Permission>());
    }
}

using Microsoft.EntityFrameworkCore;
using Mango.Services.Admin.Accounts.Domain.Entities;

namespace Mango.Services.Admin.Accounts.Infrastructure.Data;

/// <summary>
/// Database context for Admin Accounts microservice.
/// </summary>
public class AdminAccountsDbContext : DbContext
{
    public DbSet<AdminUser> AdminUsers { get; set; } = null!;
    public DbSet<AdminApiKey> AdminApiKeys { get; set; } = null!;

    public AdminAccountsDbContext(DbContextOptions<AdminAccountsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AdminUser configuration
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Role)
                .HasConversion<int>();

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            // Unique index on Email
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            // Navigation to ApiKey (one-to-one)
            entity.HasOne(e => e.ApiKey)
                .WithOne(a => a.AdminUser)
                .HasForeignKey<AdminApiKey>(a => a.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AdminApiKey configuration
        modelBuilder.Entity<AdminApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.KeyHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.KeyPrefix)
                .IsRequired()
                .HasMaxLength(8);

            entity.Property(e => e.AdminUserId)
                .IsRequired();

            entity.Property(e => e.IsRevoked)
                .HasDefaultValue(false);

            // Unique index on KeyPrefix
            entity.HasIndex(e => e.KeyPrefix)
                .IsUnique();

            // Index on AdminUserId for quick lookups
            entity.HasIndex(e => e.AdminUserId);

            // Index on ExpiresAt and IsRevoked for validation queries
            entity.HasIndex(e => new { e.ExpiresAt, e.IsRevoked });
        });
    }
}

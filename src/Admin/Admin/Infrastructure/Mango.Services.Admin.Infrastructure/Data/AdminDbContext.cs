using Microsoft.EntityFrameworkCore;
using Mango.Services.Admin.Domain.Entities;

namespace Mango.Services.Admin.Infrastructure.Data;

/// <summary>
/// Database context for Admin microservice.
/// Stores audit logs and admin-related data.
/// </summary>
public class AdminDbContext : DbContext
{
    public DbSet<AdminAuditLog> AdminAuditLogs { get; set; } = null!;

    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AdminAuditLog configuration
        modelBuilder.Entity<AdminAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Changes)
                .HasMaxLength(4000);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45); // IPv6 max length

            // Indexes for common queries
            entity.HasIndex(e => e.AdminUserId)
                .HasDatabaseName("IX_AdminUserId");

            entity.HasIndex(e => e.EntityType)
                .HasDatabaseName("IX_EntityType");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_CreatedAt");

            entity.HasIndex(e => new { e.AdminUserId, e.CreatedAt })
                .HasDatabaseName("IX_AdminUserId_CreatedAt");
        });
    }
}

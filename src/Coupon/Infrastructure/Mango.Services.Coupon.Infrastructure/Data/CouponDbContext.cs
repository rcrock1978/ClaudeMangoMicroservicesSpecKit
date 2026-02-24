using Microsoft.EntityFrameworkCore;
using CouponEntity = Mango.Services.Coupon.Domain.Entities.Coupon;

namespace Mango.Services.Coupon.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for Coupon service.
/// Handles Coupon entity persistence to SQL Server.
/// </summary>
public class CouponDbContext : DbContext
{
    public CouponDbContext(DbContextOptions<CouponDbContext> options) : base(options)
    {
    }

    public DbSet<CouponEntity> Coupons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Coupon Configuration
        modelBuilder.Entity<CouponEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.DiscountType)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Percentage");

            entity.Property(e => e.DiscountValue)
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.MinimumCartValue)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            entity.Property(e => e.MaximumDiscountAmount)
                .HasPrecision(10, 2);

            entity.Property(e => e.StartDate)
                .IsRequired();

            entity.Property(e => e.EndDate)
                .IsRequired();

            entity.Property(e => e.MaxUsageCount)
                .HasDefaultValue(0);

            entity.Property(e => e.CurrentUsageCount)
                .HasDefaultValue(0);

            entity.Property(e => e.MaxUsagePerUser)
                .HasDefaultValue(0);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes for query performance
            entity.HasIndex(e => e.Code)
                .IsUnique();

            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.EndDate);
            entity.HasIndex(e => e.IsActive);
        });
    }
}

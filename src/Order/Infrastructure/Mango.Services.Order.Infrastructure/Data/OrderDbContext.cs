using Microsoft.EntityFrameworkCore;
using OrderEntity = Mango.Services.Order.Domain.Entities.Order;
using OrderItemEntity = Mango.Services.Order.Domain.Entities.OrderItem;

namespace Mango.Services.Order.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for Order service.
/// Handles Order and OrderItem entity persistence to SQL Server.
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderItemEntity> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Order Configuration
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.OrderStatus)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.OrderDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.BillingAddress)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.PaymentMethod)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PaymentTransactionId)
                .HasMaxLength(256);

            entity.Property(e => e.DiscountAmount)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            entity.Property(e => e.ShippingCost)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            entity.Property(e => e.Tax)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Soft delete filter
            entity.HasQueryFilter(o => !o.IsDeleted);

            // Foreign key to OrderItems
            entity.HasMany<OrderItemEntity>()
                .WithOne()
                .HasForeignKey(oi => oi.OrderId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for query performance
            entity.HasIndex(e => e.UserId).HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => e.OrderNumber).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => e.OrderStatus).HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => e.OrderDate).HasFilter("[IsDeleted] = 0");
        });

        // OrderItem Configuration
        modelBuilder.Entity<OrderItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OrderId)
                .IsRequired();

            entity.Property(e => e.ProductId)
                .IsRequired();

            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(e => e.UnitPrice)
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.Quantity)
                .IsRequired();

            entity.Property(e => e.TotalPrice)
                .HasPrecision(12, 2)
                .IsRequired();

            entity.Property(e => e.DiscountAmount)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductId);
        });
    }
}

using Microsoft.EntityFrameworkCore;
using ShoppingCartEntity = Mango.Services.ShoppingCart.Domain.Entities.ShoppingCart;
using CartItemEntity = Mango.Services.ShoppingCart.Domain.Entities.CartItem;

namespace Mango.Services.ShoppingCart.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for Shopping Cart Service.
/// Manages all shopping cart and cart item entities.
/// </summary>
public class ShoppingCartDbContext : DbContext
{
    public ShoppingCartDbContext(DbContextOptions<ShoppingCartDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Shopping carts table.
    /// </summary>
    public DbSet<ShoppingCartEntity> ShoppingCarts { get; set; } = null!;

    /// <summary>
    /// Cart items table.
    /// </summary>
    public DbSet<CartItemEntity> CartItems { get; set; } = null!;

    /// <summary>
    /// Configure entity relationships and constraints.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ShoppingCart configuration
        modelBuilder.Entity<ShoppingCartEntity>(b =>
        {
            b.ToTable("ShoppingCarts");
            b.HasKey(x => x.Id);

            b.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(256);

            b.Property(x => x.CouponCode)
                .HasMaxLength(50);

            b.Property(x => x.DiscountAmount)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            b.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            b.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationship: ShoppingCart -> CartItems
            b.HasMany<CartItemEntity>()
                .WithOne()
                .HasForeignKey(x => x.ShoppingCartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for quick lookup by UserId
            b.HasIndex(x => x.UserId)
                .IsUnique();

            b.HasIndex(x => x.CreatedAt);
        });

        // CartItem configuration
        modelBuilder.Entity<CartItemEntity>(b =>
        {
            b.ToTable("CartItems");
            b.HasKey(x => x.Id);

            b.Property(x => x.ProductId)
                .IsRequired();

            b.Property(x => x.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(x => x.ProductPrice)
                .HasPrecision(10, 2)
                .IsRequired();

            b.Property(x => x.ProductImageUrl)
                .HasMaxLength(500);

            b.Property(x => x.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            b.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            b.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            b.HasIndex(x => x.ShoppingCartId);
            b.HasIndex(x => new { x.ShoppingCartId, x.ProductId })
                .IsUnique();
        });
    }
}

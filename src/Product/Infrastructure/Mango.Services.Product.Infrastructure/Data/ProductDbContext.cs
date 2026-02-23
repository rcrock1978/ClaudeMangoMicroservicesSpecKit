using Microsoft.EntityFrameworkCore;
using ProductEntity = Mango.Services.Product.Domain.Entities.Product;
using CategoryEntity = Mango.Services.Product.Domain.Entities.Category;

namespace Mango.Services.Product.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for Product service.
/// Handles Category and Product entity persistence to SQL Server.
/// </summary>
public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<ProductEntity> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category Configuration
        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Soft delete filter
            entity.HasQueryFilter(c => !c.IsDeleted);

            // Indexes
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });

        // Product Configuration
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);

            entity.Property(e => e.CategoryId)
                .IsRequired();

            entity.Property(e => e.IsAvailable)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Soft delete filter
            entity.HasQueryFilter(p => !p.IsDeleted);

            // Foreign key
            entity.HasOne<CategoryEntity>()
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for query performance
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsAvailable).HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => e.Name).HasFilter("[IsDeleted] = 0");
        });
    }
}

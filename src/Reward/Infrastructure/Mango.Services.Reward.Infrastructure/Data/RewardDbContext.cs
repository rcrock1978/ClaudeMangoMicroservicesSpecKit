using Microsoft.EntityFrameworkCore;
using RewardPointEntity = Mango.Services.Reward.Domain.Entities.RewardPoint;
using RewardTierEntity = Mango.Services.Reward.Domain.Entities.RewardTier;

namespace Mango.Services.Reward.Infrastructure.Data;

public class RewardDbContext : DbContext
{
    public RewardDbContext(DbContextOptions<RewardDbContext> options) : base(options) { }

    public DbSet<RewardPointEntity> RewardPoints { get; set; }
    public DbSet<RewardTierEntity> RewardTiers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RewardPoint Configuration
        modelBuilder.Entity<RewardPointEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("RewardPoints");

            entity.Property(e => e.UserId).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Points).IsRequired();
            entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(p => !p.IsDeleted);
            entity.HasIndex(e => e.UserId).HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => e.OrderId).HasFilter("[IsDeleted] = 0");
        });

        // RewardTier Configuration
        modelBuilder.Entity<RewardTierEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("RewardTiers");

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Benefits).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Name).IsUnique();
        });
    }
}

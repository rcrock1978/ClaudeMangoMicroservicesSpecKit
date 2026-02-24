namespace Mango.Services.Payment.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Mango.Services.Payment.Domain;

/// <summary>
/// Entity Framework Core database context for Payment service.
/// Manages Payment, PaymentRefund, and PaymentLog entities.
/// </summary>
public class PaymentDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of PaymentDbContext.
    /// </summary>
    /// <param name="options">Database context options</param>
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet for Payment entities.
    /// </summary>
    public DbSet<Payment> Payments { get; set; } = null!;

    /// <summary>
    /// DbSet for PaymentRefund entities.
    /// </summary>
    public DbSet<PaymentRefund> PaymentRefunds { get; set; } = null!;

    /// <summary>
    /// DbSet for PaymentLog entities.
    /// </summary>
    public DbSet<PaymentLog> PaymentLogs { get; set; } = null!;

    /// <summary>
    /// Configure the model builder with entity configurations.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Payment table
        modelBuilder.Entity<Payment>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Payment>()
            .HasMany(p => p.Refunds)
            .WithOne(r => r.Payment)
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>()
            .HasMany(p => p.Logs)
            .WithOne(l => l.Payment)
            .HasForeignKey(l => l.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add indexes for common queries on Payment
        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.OrderId)
            .HasDatabaseName("IX_Payment_OrderId");

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.UserId)
            .HasDatabaseName("IX_Payment_UserId");

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.Status)
            .HasDatabaseName("IX_Payment_Status");

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.TransactionId)
            .HasDatabaseName("IX_Payment_TransactionId");

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.CreatedAt)
            .HasDatabaseName("IX_Payment_CreatedAt");

        modelBuilder.Entity<Payment>()
            .HasIndex(p => new { p.OrderId, p.Status })
            .HasDatabaseName("IX_Payment_OrderId_Status");

        // Configure PaymentRefund table
        modelBuilder.Entity<PaymentRefund>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<PaymentRefund>()
            .HasIndex(r => r.PaymentId)
            .HasDatabaseName("IX_PaymentRefund_PaymentId");

        modelBuilder.Entity<PaymentRefund>()
            .HasIndex(r => r.Status)
            .HasDatabaseName("IX_PaymentRefund_Status");

        modelBuilder.Entity<PaymentRefund>()
            .HasIndex(r => r.CreatedAt)
            .HasDatabaseName("IX_PaymentRefund_CreatedAt");

        // Configure PaymentLog table
        modelBuilder.Entity<PaymentLog>()
            .HasKey(l => l.Id);

        modelBuilder.Entity<PaymentLog>()
            .HasIndex(l => l.PaymentId)
            .HasDatabaseName("IX_PaymentLog_PaymentId");

        modelBuilder.Entity<PaymentLog>()
            .HasIndex(l => l.TransactionId)
            .HasDatabaseName("IX_PaymentLog_TransactionId");

        modelBuilder.Entity<PaymentLog>()
            .HasIndex(l => l.Timestamp)
            .HasDatabaseName("IX_PaymentLog_Timestamp");

        modelBuilder.Entity<PaymentLog>()
            .HasIndex(l => l.EventType)
            .HasDatabaseName("IX_PaymentLog_EventType");

        // Configure column properties
        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Payment>()
            .Property(p => p.RefundedAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<PaymentRefund>()
            .Property(r => r.RefundAmount)
            .HasColumnType("decimal(18,2)");

        // Set default values
        modelBuilder.Entity<Payment>()
            .Property(p => p.Currency)
            .HasDefaultValue("USD");

        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasDefaultValue(PaymentStatus.Pending);

        modelBuilder.Entity<PaymentRefund>()
            .Property(r => r.Status)
            .HasDefaultValue(PaymentStatus.Processing);
    }
}

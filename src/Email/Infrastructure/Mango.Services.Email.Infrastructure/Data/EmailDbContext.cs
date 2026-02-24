using Microsoft.EntityFrameworkCore;
using EmailTemplateEntity = Mango.Services.Email.Domain.Entities.EmailTemplate;
using EmailLogEntity = Mango.Services.Email.Domain.Entities.EmailLog;

namespace Mango.Services.Email.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for Email service.
/// Handles EmailTemplate and EmailLog entity persistence to SQL Server.
/// </summary>
public class EmailDbContext : DbContext
{
    public EmailDbContext(DbContextOptions<EmailDbContext> options) : base(options)
    {
    }

    public DbSet<EmailTemplateEntity> EmailTemplates { get; set; }
    public DbSet<EmailLogEntity> EmailLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // EmailTemplate Configuration
        modelBuilder.Entity<EmailTemplateEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("EmailTemplates");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode();

            entity.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode();

            entity.Property(e => e.Body)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.Variables)
                .HasMaxLength(1000)
                .IsUnicode();

            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode();

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Soft delete filter
            entity.HasQueryFilter(t => !t.IsDeleted);

            // Unique index on name for active templates
            entity.HasIndex(e => new { e.Name, e.IsActive })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Index for querying active templates
            entity.HasIndex(e => e.IsActive).HasFilter("[IsDeleted] = 0");
        });

        // EmailLog Configuration
        modelBuilder.Entity<EmailLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("EmailLogs");

            entity.Property(e => e.RecipientEmail)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.RecipientName)
                .HasMaxLength(250);

            entity.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Body)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.TemplateName)
                .HasMaxLength(100);

            entity.Property(e => e.ErrorMessage)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.IsSent)
                .HasDefaultValue(false);

            entity.Property(e => e.AttemptCount)
                .HasDefaultValue(0);

            entity.Property(e => e.UserId)
                .HasMaxLength(256);

            entity.Property(e => e.EmailType)
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Soft delete filter
            entity.HasQueryFilter(l => !l.IsDeleted);

            // Indexes for query performance
            entity.HasIndex(e => e.RecipientEmail).HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => e.UserId).HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => e.OrderId).HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => new { e.IsSent, e.AttemptCount }).HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => e.CreatedAt).HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => e.EmailType).HasFilter("[IsDeleted] = 0");
        });
    }
}

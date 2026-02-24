using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mango.Services.Admin.Infrastructure.Data;

/// <summary>
/// Design-time factory for creating AdminDbContext instances.
/// Required by Entity Framework Core for migration generation.
/// </summary>
public class AdminDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
{
    /// <summary>
    /// Creates a new instance of AdminDbContext for design-time operations.
    /// </summary>
    /// <param name="args">Command-line arguments (unused).</param>
    /// <returns>A configured AdminDbContext instance.</returns>
    public AdminDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AdminDbContext>();

        // Use SQL Server with connection string from environment or default
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__AdminDb")
            ?? "Server=localhost;Database=MangoAdminDb;User Id=sa;Password=Admin@123;TrustServerCertificate=true;";

        optionsBuilder.UseSqlServer(connectionString, x =>
        {
            x.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
        });

        return new AdminDbContext(optionsBuilder.Options);
    }
}

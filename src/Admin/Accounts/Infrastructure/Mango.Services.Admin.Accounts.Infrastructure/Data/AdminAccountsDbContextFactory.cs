using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mango.Services.Admin.Accounts.Infrastructure.Data;

/// <summary>
/// Factory for creating DbContext instances at design time for migrations.
/// </summary>
public class AdminAccountsDbContextFactory : IDesignTimeDbContextFactory<AdminAccountsDbContext>
{
    public AdminAccountsDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Server=localhost;Database=MangoAdminAccountsDb;User Id=sa;Password=Admin@123;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<AdminAccountsDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AdminAccountsDbContext(optionsBuilder.Options);
    }
}

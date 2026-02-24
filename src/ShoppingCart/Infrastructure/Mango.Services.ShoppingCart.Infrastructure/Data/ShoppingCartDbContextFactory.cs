using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mango.Services.ShoppingCart.Infrastructure.Data;

/// <summary>
/// Design-time factory for creating ShoppingCartDbContext instances.
/// Required for EF Core migrations during development.
/// </summary>
public class ShoppingCartDbContextFactory : IDesignTimeDbContextFactory<ShoppingCartDbContext>
{
    public ShoppingCartDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Server=localhost,1433;Database=Mango_ShoppingCart;User Id=sa;Password=YourPassword123!;Encrypt=false;TrustServerCertificate=true;";

        var optionsBuilder = new DbContextOptionsBuilder<ShoppingCartDbContext>()
            .UseSqlServer(connectionString);

        return new ShoppingCartDbContext(optionsBuilder.Options);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mango.Services.Product.Infrastructure.Data;

/// <summary>
/// Design-time factory for creating ProductDbContext instances.
/// Required for EF Core migrations during development.
/// </summary>
public class ProductDbContextFactory : IDesignTimeDbContextFactory<ProductDbContext>
{
    public ProductDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Server=localhost,1433;Database=Mango_Product;User Id=sa;Password=YourPassword123!;Encrypt=false;TrustServerCertificate=true;";

        var optionsBuilder = new DbContextOptionsBuilder<ProductDbContext>()
            .UseSqlServer(connectionString);

        return new ProductDbContext(optionsBuilder.Options);
    }
}

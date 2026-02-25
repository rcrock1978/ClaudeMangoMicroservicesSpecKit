using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Mango.Services.Admin.API;
using Mango.Services.Admin.Infrastructure.Data;

namespace Mango.Services.Admin.IntegrationTests.Fixtures;

/// <summary>
/// Test fixture for Admin Service API integration testing.
/// Sets up in-memory database and test server with mocked external services.
/// </summary>
public class AdminApiTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string DbName = "AdminServiceTestDb";
    public HttpClient Client { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all DbContext-related services for AdminDbContext
            var toRemove = services
                .Where(d => d.ServiceType.IsGenericType &&
                            d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) &&
                            d.ServiceType.GenericTypeArguments[0] == typeof(AdminDbContext))
                .ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing with consistent name
            services.AddDbContext<AdminDbContext>(options =>
                options.UseInMemoryDatabase(DbName),
                ServiceLifetime.Scoped);
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        Client = CreateClient();

        // Initialize database schema
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        // In-memory database will be automatically cleaned up
        await base.DisposeAsync();
    }
}

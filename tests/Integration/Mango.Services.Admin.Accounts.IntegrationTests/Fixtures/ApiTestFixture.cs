using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Mango.Services.Admin.Accounts.API;
using Mango.Services.Admin.Accounts.Infrastructure.Data;
using Mango.Services.Admin.Accounts.Infrastructure.Services;
using Mango.Services.Admin.Accounts.Domain.Entities;

namespace Mango.Services.Admin.Accounts.IntegrationTests.Fixtures;

/// <summary>
/// Test fixture for Admin.Accounts API integration testing.
/// Sets up in-memory database and test server.
/// </summary>
public class ApiTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private string _dbConnectionString = "Data Source=:memory:";
    private const string DbName = "AdminAccountsTestDb";
    public HttpClient Client { get; private set; } = null!;
    public string? ValidApiKey { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all DbContext-related services for AdminAccountsDbContext
            var toRemove = services
                .Where(d => d.ServiceType.IsGenericType &&
                            d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) &&
                            d.ServiceType.GenericTypeArguments[0] == typeof(AdminAccountsDbContext))
                .ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing with consistent name
            services.AddDbContext<AdminAccountsDbContext>(options =>
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
        var context = scope.ServiceProvider.GetRequiredService<AdminAccountsDbContext>();
        await context.Database.EnsureCreatedAsync();

        // Create a test admin user with valid API key
        await CreateTestAdminUserAsync(context);
    }

    private async Task CreateTestAdminUserAsync(AdminAccountsDbContext context)
    {
        // Create test admin user
        var testUser = new AdminUser
        {
            Email = "test@admin.com",
            FullName = "Test Admin",
            Role = AdminRole.SUPER_ADMIN,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.AdminUsers.Add(testUser);
        await context.SaveChangesAsync();

        // Create hashing service
        var hashingService = new ApiKeyHashingService();

        // Generate a test API key
        ValidApiKey = System.Security.Cryptography.RandomNumberGenerator
            .GetBytes(32)
            .Aggregate("", (s, b) => s + b.ToString("x2"));

        var keyHash = hashingService.HashKey(ValidApiKey);
        var apiKey = new AdminApiKey
        {
            AdminUserId = testUser.Id,
            KeyHash = keyHash,
            KeyPrefix = ValidApiKey.Substring(0, Math.Min(8, ValidApiKey.Length)),
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        context.AdminApiKeys.Add(apiKey);
        await context.SaveChangesAsync();
    }

    public new async Task DisposeAsync()
    {
        // In-memory database will be automatically cleaned up
        await base.DisposeAsync();
    }
}

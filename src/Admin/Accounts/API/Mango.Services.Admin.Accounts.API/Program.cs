using Microsoft.EntityFrameworkCore;
using Serilog;
using MediatR;
using Mango.Services.Admin.Accounts.Infrastructure.Data;
using Mango.Services.Admin.Accounts.Application.Interfaces;
using Mango.Services.Admin.Accounts.Infrastructure.Repositories;
using Mango.Services.Admin.Accounts.Infrastructure.Services;
using Mango.Services.Admin.Accounts.API.Middleware;
using Mango.Services.Admin.Accounts.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Database - only register for non-test environments
if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("AdminAccountsDb");
    builder.Services.AddDbContext<AdminAccountsDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// Repositories
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IAdminApiKeyRepository, AdminApiKeyRepository>();

// Services
builder.Services.AddScoped<IApiKeyHashingService, ApiKeyHashingService>();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Mango.Services.Admin.Accounts.Application.MediatR.BaseCommand).Assembly));

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    // API documentation can be added later
}

// Custom middleware
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

// Standard middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdminAccountsDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error applying database migrations");
    }
}

app.Run();

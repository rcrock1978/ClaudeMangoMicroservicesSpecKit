using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Mango.Services.Admin.Infrastructure.Data;
using Mango.Services.Admin.Infrastructure.Repositories;
using Mango.Services.Admin.Infrastructure.Services;
using Mango.Services.Admin.Infrastructure.HttpClients;
using Mango.Services.Admin.Application.Interfaces;
using Mango.Services.Admin.Application.MediatR.Queries;
using Mango.Services.Admin.API.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/admin-service-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext
builder.Services.AddDbContext<AdminDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AdminDb")
        ?? "Server=localhost;Database=MangoAdminDb;User Id=sa;Password=Admin@123;TrustServerCertificate=true;";

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
    });
});

// Register Repositories
builder.Services.AddScoped<IAdminAuditLogRepository, AdminAuditLogRepository>();

// Register Services
builder.Services.AddScoped<IDataAggregationService, DataAggregationService>();

// Register HTTP clients for microservices
builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductService"] ?? "http://localhost:5001");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:OrderService"] ?? "http://localhost:5004");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:PaymentService"] ?? "http://localhost:5005");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IRewardServiceClient, RewardServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:RewardService"] ?? "http://localhost:5007");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ICouponServiceClient, CouponServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:CouponService"] ?? "http://localhost:5006");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add MediatR for CQRS
builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssembly(typeof(GetDashboardKpisQuery).Assembly));

// Add memory cache for dashboard KPI caching
builder.Services.AddMemoryCache();

// Add authentication for API key
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });

// Add authorization
builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApiGateway", policy =>
    {
        policy.WithOrigins(builder.Configuration["ApiGateway:BaseUrl"] ?? "http://localhost:5000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Migrate database on startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migration completed successfully");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Database migration failed");
    throw;
}

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Admin Service API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowApiGateway");
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/api/admin/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    service = "Mango.Services.Admin"
}))
.WithName("HealthCheck");

app.MapControllers();

Log.Information("Admin Service starting...");
await app.RunAsync();

Log.Information("Admin Service stopped");

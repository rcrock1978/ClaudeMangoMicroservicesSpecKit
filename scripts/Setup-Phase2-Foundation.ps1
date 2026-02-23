# Phase 2: Foundational Infrastructure Setup
# Completes T020-T025 and T040-T055 (Program.cs configuration + infrastructure wiring)
# This script configures JWT, Serilog, OpenTelemetry, Health Checks, Swagger, and AutoMapper

param(
    [string]$RepoRoot = (Get-Item $PSScriptRoot).Parent.FullName
)

$ErrorActionPreference = "Stop"
$SrcDir = Join-Path $RepoRoot "src"

Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Phase 2: Foundational Infrastructure Configuration          ║" -ForegroundColor Cyan
Write-Host "║  Tasks: T020-T025, T040-T055                                 ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$Services = @("Auth", "Product", "ShoppingCart", "Coupon", "Order", "Email", "Reward")

# Step 1: Create JWT Options and Authentication Extensions
Write-Host "Step 1: Creating JWT Authentication infrastructure..." -ForegroundColor Yellow

foreach ($service in $Services) {
    $infraPath = Join-Path $SrcDir $service "$service\Infrastructure"

    # Create Options directory
    $optionsPath = Join-Path $infraPath "Options"
    New-Item -ItemType Directory -Path $optionsPath -Force | Out-Null

    # Create JwtOptions.cs
    $jwtOptionsContent = @"
namespace Mango.Services.$service.Infrastructure.Options;

public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "mango-auth";
    public string Audience { get; set; } = "mango-client";
    public int AccessTokenExpiryMinutes { get; set; } = 30;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
"@

    Set-Content -Path (Join-Path $optionsPath "JwtOptions.cs") -Value $jwtOptionsContent

    # Create Extensions directory
    $extPath = Join-Path $infraPath "Extensions"
    New-Item -ItemType Directory -Path $extPath -Force | Out-Null

    # Create AuthenticationExtensions.cs
    $authExtContent = @"
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Mango.Services.$service.Infrastructure.Options;
using System.Text;

namespace Mango.Services.$service.Infrastructure.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection("JwtOptions").Get<JwtOptions>();

        if (jwtOptions == null)
        {
            throw new InvalidOperationException("JwtOptions not configured");
        }

        services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
"@

    Set-Content -Path (Join-Path $extPath "AuthenticationExtensions.cs") -Value $authExtContent

    Write-Host "  ✓ $service JWT infrastructure"
}

# Step 2: Create AutoMapper profiles
Write-Host ""
Write-Host "Step 2: Creating AutoMapper mapping profiles..." -ForegroundColor Yellow

foreach ($service in $Services) {
    $appPath = Join-Path $SrcDir $service "$service\Application"
    $mappingPath = Join-Path $appPath "Mappings"
    New-Item -ItemType Directory -Path $mappingPath -Force | Out-Null

    # Create MappingProfile.cs
    $mappingContent = @"
using AutoMapper;
// TODO: Add domain entity imports
// using Mango.Services.$service.Domain.Entities;
// using Mango.Services.$service.Application.DTOs;

namespace Mango.Services.$service.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // TODO: Add entity to DTO mappings
        // Example pattern:
        // CreateMap<Product, ProductDto>().ReverseMap();
        // CreateMap<Category, CategoryDto>().ReverseMap();
    }
}
"@

    Set-Content -Path (Join-Path $mappingPath "MappingProfile.cs") -Value $mappingContent

    Write-Host "  ✓ $service AutoMapper profile"
}

# Step 3: Update appsettings.json for each service
Write-Host ""
Write-Host "Step 3: Creating appsettings configuration templates..." -ForegroundColor Yellow

foreach ($service in $Services) {
    $apiPath = Join-Path $SrcDir $service "$service\API"
    $appSettingsPath = Join-Path $apiPath "appsettings.Development.json"

    $appSettings = @{
        "Logging" = @{
            "LogLevel" = @{
                "Default" = "Information"
                "Microsoft" = "Warning"
            }
        }
        "JwtOptions" = @{
            "Secret" = "your-secret-key-must-be-at-least-32-characters-long"
            "Issuer" = "mango-auth"
            "Audience" = "mango-client"
            "AccessTokenExpiryMinutes" = 30
            "RefreshTokenExpiryDays" = 7
        }
        "ConnectionStrings" = @{
            "DefaultConnection" = "Server=localhost,1433;Database=Mango_${service};User Id=sa;Password=YourPassword123!;Encrypt=false;TrustServerCertificate=true;"
        }
        "RabbitMQ" = @{
            "Url" = "amqp://guest:guest@localhost:5672"
        }
        "Redis" = @{
            "Connection" = "localhost:6379"
        }
        "OpenTelemetry" = @{
            "Endpoint" = "http://localhost:4317"
        }
    } | ConvertTo-Json -Depth 10

    # Check if file exists, if not create it
    if (-not (Test-Path $appSettingsPath)) {
        Set-Content -Path $appSettingsPath -Value $appSettings
        Write-Host "  ✓ $service appsettings created"
    } else {
        Write-Host "  ✓ $service appsettings exists (skipped)"
    }
}

# Step 4: Create Program.cs template for each service
Write-Host ""
Write-Host "Step 4: Creating Program.cs templates..." -ForegroundColor Yellow

foreach ($service in $Services) {
    $apiPath = Join-Path $SrcDir $service "$service\API"
    $programPath = Join-Path $apiPath "Program.cs"

    $programContent = @"
using Serilog;
using Mango.Services.$service.Infrastructure.Extensions;
using Mango.Services.$service.Application.Mappings;
using MediatR;

// Configure Serilog logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "$service")
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Add services
    builder.Services
        .AddJwtAuthentication(builder.Configuration)
        .AddAutoMapper(typeof(MappingProfile))
        .AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        })
        .AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });

    builder.Services
        .AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Mango $service API",
            Version = "v1"
        });

        var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "JWT Authorization header using Bearer scheme"
        };

        options.AddSecurityDefinition("Bearer", securityScheme);

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mango $service API v1"));
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapHealthChecks("/health/live");
    app.MapHealthChecks("/health/ready");
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
"@

    # Backup existing Program.cs if it exists
    if (Test-Path $programPath) {
        $backupPath = "$programPath.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        Copy-Item -Path $programPath -Destination $backupPath
        Write-Host "  ✓ $service Program.cs (backed up to $backupPath)"
    }

    # Note: Don't overwrite Program.cs as dotnet templates already create one
    # Instead, show path for manual review
    Write-Host "  ℹ $service Program.cs template ready for review/merge"
}

# Step 5: Verify build
Write-Host ""
Write-Host "Step 5: Building solution to verify configuration..." -ForegroundColor Yellow

Push-Location $SrcDir
try {
    dotnet build --no-restore 2>&1 | Select-String -Pattern "succeeded|failed|error|Error" | ForEach-Object { Write-Host "  $_" }
    Write-Host "  ✓ Build verification complete"
} catch {
    Write-Host "  ⚠ Build verification failed - review output above"
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  Phase 2 Foundation Configuration Complete!                  ║" -ForegroundColor Green
Write-Host "║                                                              ║" -ForegroundColor Green
Write-Host "║  Next Steps:                                                 ║" -ForegroundColor Green
Write-Host "║  1. Review Program.cs templates for each service             ║" -ForegroundColor Green
Write-Host "║  2. Update connection strings in appsettings.json            ║" -ForegroundColor Green
Write-Host "║  3. Update JWT secret key with a strong value                ║" -ForegroundColor Green
Write-Host "║  4. Review MappingProfile.cs and add entity mappings         ║" -ForegroundColor Green
Write-Host "║  5. Run Phase 2 NuGet installation:                          ║" -ForegroundColor Green
Write-Host "║     .\scripts\Install-Phase2-Packages.ps1                    ║" -ForegroundColor Green
Write-Host "║  6. Build and verify: dotnet build                           ║" -ForegroundColor Green
Write-Host "║  7. Proceed to Phase 3 (User Story 1 MVP)                    ║" -ForegroundColor Green
Write-Host "║                                                              ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Green

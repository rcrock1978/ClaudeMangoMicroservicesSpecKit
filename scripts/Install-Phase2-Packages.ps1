# Phase 2: Install all NuGet dependencies for microservices
# This script completes T020-T025 package requirements

param(
    [string]$RepoRoot = (Get-Item $PSScriptRoot).Parent.FullName
)

$ErrorActionPreference = "Stop"
$SrcDir = Join-Path $RepoRoot "src"

Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Phase 2: Installing NuGet Packages                          ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Services to process
$Services = @("Auth", "Product", "ShoppingCart", "Coupon", "Order", "Email", "Reward")

# Common packages for ALL services (Application + Infrastructure layers)
$CommonApplicationPackages = @(
    "MediatR:8.0.1"
    "MediatR.Extensions.Microsoft.DependencyInjection:11.1.1"
    "FluentValidation:11.9.2"
    "AutoMapper:13.0.1"
    "AutoMapper.Extensions.Microsoft.DependencyInjection:12.0.1"
)

# Common packages for ALL API projects
$CommonApiPackages = @(
    "Serilog:4.1.1"
    "Serilog.AspNetCore:8.2.1"
    "Serilog.Sinks.Console:6.0.0"
    "Serilog.Enrichers.CorrelationId:2.0.0"
    "OpenTelemetry:1.10.0"
    "OpenTelemetry.Exporter.Trace.Otlp:1.10.0"
    "OpenTelemetry.Instrumentation.AspNetCore:1.10.0"
    "OpenTelemetry.Instrumentation.EntityFrameworkCore:1.0.0-beta.11"
    "OpenTelemetry.Instrumentation.Runtime:0.2.0-alpha.2"
    "Microsoft.AspNetCore.Authentication.JwtBearer:10.0.0"
    "System.IdentityModel.Tokens.Jwt:8.3.0"
    "Microsoft.AspNetCore.HealthChecks:2.2.0"
    "Microsoft.Extensions.Diagnostics.HealthChecks:10.0.0"
    "Swashbuckle.AspNetCore:6.1.7"
    "Swashbuckle.AspNetCore.Annotations:6.1.7"
)

# Infrastructure packages (EF Core, MassTransit)
$InfrastructurePackages = @(
    "Microsoft.EntityFrameworkCore:10.0.0"
    "Microsoft.EntityFrameworkCore.SqlServer:10.0.0"
    "Microsoft.EntityFrameworkCore.Tools:10.0.0"
    "MassTransit:8.2.3"
    "MassTransit.EntityFrameworkCore:8.2.3"
    "StackExchange.Redis:2.8.25"
    "Microsoft.Extensions.Caching.StackExchangeRedis:10.0.0"
)

# Install packages for each service
foreach ($service in $Services) {
    Write-Host ""
    Write-Host "Processing $service service..." -ForegroundColor Yellow

    # Application layer
    Write-Host "  Installing Application layer packages..." -ForegroundColor Magenta
    $appPath = Join-Path $SrcDir $service "$service\Application\Mango.Services.$service.Application.csproj"
    if (Test-Path $appPath) {
        foreach ($pkg in $CommonApplicationPackages) {
            $parts = $pkg -split ":"
            $name = $parts[0]
            $version = $parts[1]
            Push-Location (Split-Path $appPath)
            & dotnet add package $name --version $version 2>&1 | Out-Null
            Write-Host "    ✓ $name"
            Pop-Location
        }
    }

    # Infrastructure layer
    Write-Host "  Installing Infrastructure layer packages..." -ForegroundColor Magenta
    $infraPath = Join-Path $SrcDir $service "$service\Infrastructure\Mango.Services.$service.Infrastructure.csproj"
    if (Test-Path $infraPath) {
        foreach ($pkg in $InfrastructurePackages) {
            $parts = $pkg -split ":"
            $name = $parts[0]
            $version = $parts[1]
            Push-Location (Split-Path $infraPath)
            & dotnet add package $name --version $version 2>&1 | Out-Null
            Write-Host "    ✓ $name"
            Pop-Location
        }
    }

    # API layer
    Write-Host "  Installing API layer packages..." -ForegroundColor Magenta
    $apiPath = Join-Path $SrcDir $service "$service\API\Mango.Services.$service.API.csproj"
    if (Test-Path $apiPath) {
        foreach ($pkg in $CommonApiPackages) {
            $parts = $pkg -split ":"
            $name = $parts[0]
            $version = $parts[1]
            Push-Location (Split-Path $apiPath)
            & dotnet add package $name --version $version 2>&1 | Out-Null
            Write-Host "    ✓ $name"
            Pop-Location
        }
    }
}

# Order-specific: Stripe payment service
Write-Host ""
Write-Host "Processing Order service (Stripe payment)..." -ForegroundColor Yellow
$orderApiPath = Join-Path $SrcDir "Order" "Order\API\Mango.Services.Order.API.csproj"
if (Test-Path $orderApiPath) {
    Push-Location (Split-Path $orderApiPath)
    & dotnet add package Stripe --version 48.3.0 2>&1 | Out-Null
    Write-Host "  ✓ Stripe"
    Pop-Location
}

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  Phase 2 NuGet Installation Complete!                         ║" -ForegroundColor Green
Write-Host "║  Next: Run ./scripts/Setup-Phase2-Foundation.ps1              ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Green

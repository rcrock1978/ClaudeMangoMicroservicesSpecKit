# Phase 1: Project Scaffolding for Mango Microservices E-Commerce Platform
# This script creates all solution structure, projects, and references for Phase 1 (Setup)
# Usage: .\Setup-Phase1-Projects.ps1

param(
    [string]$RepoRoot = (Get-Item $PSScriptRoot).Parent.FullName
)

$ErrorActionPreference = "Stop"
$srcDir = Join-Path $RepoRoot "src"
$testsDir = Join-Path $RepoRoot "tests"
$dockerDir = Join-Path $RepoRoot "docker"
$k8sDir = Join-Path $RepoRoot "k8s"

Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Mango Microservices - Phase 1: Project Scaffolding          ║" -ForegroundColor Cyan
Write-Host "║  Repository Root: $RepoRoot" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Step 1: Create directories
Write-Host "Step 1: Creating directory structure..." -ForegroundColor Yellow
@($srcDir, $testsDir, $dockerDir, $k8sDir, "$k8sDir/infrastructure") | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -ItemType Directory -Path $_ -Force | Out-Null
        Write-Host "  ✓ Created $_"
    } else {
        Write-Host "  ✓ Already exists: $_"
    }
}

# Step 2: Create solution file (T001)
Write-Host ""
Write-Host "Step 2: Creating solution file..." -ForegroundColor Yellow
Push-Location $srcDir
if (-not (Test-Path "Mango.Microservices.sln")) {
    & dotnet new sln --name "Mango.Microservices" --force
    Write-Host "  ✓ Created Mango.Microservices.sln"
} else {
    Write-Host "  ✓ Solution already exists"
}

# Step 3: Create service projects (T002-T009 + T010-T011)
Write-Host ""
Write-Host "Step 3: Creating microservice projects (9 services)..." -ForegroundColor Yellow

$services = @("Auth", "Product", "ShoppingCart", "Coupon", "Order", "Email", "Reward")
$projectsCreated = @()

foreach ($service in $services) {
    Write-Host "  Creating Mango.Services.$($service)API..." -ForegroundColor Magenta

    $layers = @("Domain", "Application", "Infrastructure", "API")

    foreach ($layer in $layers) {
        $projectName = "Mango.Services.$($service).$layer"
        $projectDir = Join-Path $srcDir "$($service)" $layer

        if (-not (Test-Path "$projectDir/$projectName.csproj")) {
            # Create directory
            New-Item -ItemType Directory -Path $projectDir -Force | Out-Null

            # Create project
            Push-Location $projectDir
            if ($layer -eq "API") {
                & dotnet new webapi --name $projectName --force
            } else {
                & dotnet new classlib --name $projectName --force
            }
            Pop-Location

            Write-Host "    ✓ $projectName"
            $projectsCreated += "$projectDir/$projectName.csproj"
        } else {
            Write-Host "    ✓ $projectName (already exists)"
            $projectsCreated += "$projectDir/$projectName.csproj"
        }
    }
}

# Step 4: Create Shared Libraries
Write-Host ""
Write-Host "Step 4: Creating shared libraries..." -ForegroundColor Yellow

# MessageBus Library (T002)
$mbDir = Join-Path $srcDir "Mango.MessageBus"
if (-not (Test-Path "$mbDir/Mango.MessageBus.csproj")) {
    New-Item -ItemType Directory -Path $mbDir -Force | Out-Null
    Push-Location $mbDir
    & dotnet new classlib --name "Mango.MessageBus" --force
    Pop-Location
    Write-Host "  ✓ Mango.MessageBus"
    $projectsCreated += "$mbDir/Mango.MessageBus.csproj"
} else {
    Write-Host "  ✓ Mango.MessageBus (already exists)"
    $projectsCreated += "$mbDir/Mango.MessageBus.csproj"
}

# Step 5: Create Gateway and Frontend (T010-T011)
Write-Host ""
Write-Host "Step 5: Creating gateway and frontend..." -ForegroundColor Yellow

# Gateway
$gwDir = Join-Path $srcDir "Mango.GatewaySolution"
if (-not (Test-Path "$gwDir/Mango.GatewaySolution.csproj")) {
    New-Item -ItemType Directory -Path $gwDir -Force | Out-Null
    Push-Location $gwDir
    & dotnet new webapi --name "Mango.GatewaySolution" --force
    Pop-Location
    Write-Host "  ✓ Mango.GatewaySolution"
    $projectsCreated += "$gwDir/Mango.GatewaySolution.csproj"
} else {
    Write-Host "  ✓ Mango.GatewaySolution (already exists)"
    $projectsCreated += "$gwDir/Mango.GatewaySolution.csproj"
}

# Web Frontend
$webDir = Join-Path $srcDir "Mango.Web"
if (-not (Test-Path "$webDir/Mango.Web.csproj")) {
    New-Item -ItemType Directory -Path $webDir -Force | Out-Null
    Push-Location $webDir
    & dotnet new mvc --name "Mango.Web" --force
    Pop-Location
    Write-Host "  ✓ Mango.Web"
    $projectsCreated += "$webDir/Mango.Web.csproj"
} else {
    Write-Host "  ✓ Mango.Web (already exists)"
    $projectsCreated += "$webDir/Mango.Web.csproj"
}

# Step 6: Create Test Projects (T012)
Write-Host ""
Write-Host "Step 6: Creating test projects..." -ForegroundColor Yellow

Push-Location $testsDir

foreach ($service in $services) {
    @("UnitTests", "IntegrationTests") | ForEach-Object {
        $testName = "Mango.Services.$($service).$_"
        if (-not (Test-Path "$testName/$testName.csproj")) {
            & dotnet new xunit --name $testName --force
            Write-Host "  ✓ $testName"
            $projectsCreated += "$testsDir/$testName/$testName.csproj"
        } else {
            Write-Host "  ✓ $testName (already exists)"
            $projectsCreated += "$testsDir/$testName/$testName.csproj"
        }
    }
}

# Contract Tests
if (-not (Test-Path "Mango.ContractTests/Mango.ContractTests.csproj")) {
    & dotnet new xunit --name "Mango.ContractTests" --force
    Write-Host "  ✓ Mango.ContractTests"
    $projectsCreated += "$testsDir/Mango.ContractTests/Mango.ContractTests.csproj"
} else {
    Write-Host "  ✓ Mango.ContractTests (already exists)"
    $projectsCreated += "$testsDir/Mango.ContractTests/Mango.ContractTests.csproj"
}

Pop-Location

# Step 7: Add all projects to solution
Write-Host ""
Write-Host "Step 7: Adding projects to solution..." -ForegroundColor Yellow

Push-Location $srcDir

foreach ($proj in $projectsCreated) {
    $projName = Split-Path $proj -Leaf
    $projName = $projName -replace "\.csproj$", ""

    $existing = & dotnet sln list | Select-String $projName
    if ($null -eq $existing) {
        & dotnet sln add $proj
        Write-Host "  ✓ Added $projName"
    } else {
        Write-Host "  ✓ Already in solution: $projName"
    }
}

# Step 8: Set up project references (Clean Architecture dependency flow)
Write-Host ""
Write-Host "Step 8: Configuring project references (Clean Architecture)..." -ForegroundColor Yellow

foreach ($service in $services) {
    $servicePath = Join-Path $srcDir $service

    # Domain: no dependencies
    # Application -> Domain
    Push-Location "$servicePath/Application"
    & dotnet add reference "../Domain/Mango.Services.$($service).Domain.csproj"
    Pop-Location
    Write-Host "  ✓ $service.Application -> $service.Domain"

    # Infrastructure -> Domain, Application
    Push-Location "$servicePath/Infrastructure"
    & dotnet add reference "../Domain/Mango.Services.$($service).Domain.csproj"
    & dotnet add reference "../Application/Mango.Services.$($service).Application.csproj"
    Pop-Location
    Write-Host "  ✓ $service.Infrastructure -> $service.Domain, $service.Application"

    # API -> Infrastructure, Application
    Push-Location "$servicePath/API"
    & dotnet add reference "../Infrastructure/Mango.Services.$($service).Infrastructure.csproj"
    & dotnet add reference "../Application/Mango.Services.$($service).Application.csproj"
    Pop-Location
    Write-Host "  ✓ $service.API -> $service.Infrastructure, $service.Application"
}

# Step 9: Create .editorconfig (T013)
Write-Host ""
Write-Host "Step 9: Creating .editorconfig..." -ForegroundColor Yellow

$editorConfigContent = @"
root = true

# All files
[*]
indent_style = space
indent_size = 4
trim_trailing_whitespace = true
insert_final_newline = true
charset = utf-8

# C# files
[*.cs]
csharp_indent_case_contents = true
csharp_indent_switch_labels = true
csharp_style_namespace_declarations = file_scoped:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_for_built_in_types = false:suggestion
csharp_style_var_elsewhere = false:suggestion
dotnet_style_require_accessibility_modifiers = for_non_interface_members:suggestion
dotnet_style_null_coalescing_operator = true:suggestion
dotnet_style_prefer_null_propagation = true:suggestion

# Project files
[*.csproj]
indent_size = 2

# YAML files
[*.{yml,yaml}]
indent_size = 2

# JSON files
[*.json]
indent_size = 2
"@

$editorConfigPath = Join-Path $srcDir ".editorconfig"
if (-not (Test-Path $editorConfigPath)) {
    Set-Content -Path $editorConfigPath -Value $editorConfigContent
    Write-Host "  ✓ Created .editorconfig"
} else {
    Write-Host "  ✓ .editorconfig already exists"
}

# Step 10: Create Docker compose files (T014-T015)
Write-Host ""
Write-Host "Step 10: Creating Docker Compose files..." -ForegroundColor Yellow

$dockerComposeContent = @"
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "YourPassword123!"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourPassword123! -Q 'SELECT 1' || exit 1"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - mango_network

  rabbitmq:
    image: rabbitmq:3.13-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - mango_network

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - mango_network

volumes:
  sqlserver_data:
  rabbitmq_data:
  redis_data:

networks:
  mango_network:
    driver: bridge
"@

$dockerComposePath = Join-Path $dockerDir "docker-compose.yml"
if (-not (Test-Path $dockerComposePath)) {
    Set-Content -Path $dockerComposePath -Value $dockerComposeContent
    Write-Host "  ✓ Created docker-compose.yml"
} else {
    Write-Host "  ✓ docker-compose.yml already exists"
}

$dockerComposeOverridePath = Join-Path $dockerDir "docker-compose.override.yml"
if (-not (Test-Path $dockerComposeOverridePath)) {
    Set-Content -Path $dockerComposeOverridePath -Value @"
version: '3.8'

# Development overrides - use for local development
# Run: docker-compose -f docker-compose.yml -f docker-compose.override.yml up
"@
    Write-Host "  ✓ Created docker-compose.override.yml"
} else {
    Write-Host "  ✓ docker-compose.override.yml already exists"
}

# Step 11: Verify solution builds
Write-Host ""
Write-Host "Step 11: Verifying solution builds..." -ForegroundColor Yellow

Push-Location $srcDir
try {
    $buildOutput = & dotnet build 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Solution builds successfully"
    } else {
        Write-Host "  ✗ Build failed. Output:" -ForegroundColor Red
        Write-Host $buildOutput
        exit 1
    }
} catch {
    Write-Host "  ✗ Build error: $_" -ForegroundColor Red
    exit 1
}
Pop-Location

# Summary
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  Phase 1 Setup Complete!                                      ║" -ForegroundColor Green
Write-Host "╠════════════════════════════════════════════════════════════════╣" -ForegroundColor Green
Write-Host "║  ✓ Solution file created                                      ║" -ForegroundColor Green
Write-Host "║  ✓ 9 microservices (7 core + Gateway + Web)                 ║" -ForegroundColor Green
Write-Host "║  ✓ 4 layers per service (Domain/App/Infra/API)              ║" -ForegroundColor Green
Write-Host "║  ✓ Test projects (Unit + Integration + Contract)             ║" -ForegroundColor Green
Write-Host "║  ✓ Shared MessageBus library                                  ║" -ForegroundColor Green
Write-Host "║  ✓ Docker Compose infrastructure                              ║" -ForegroundColor Green
Write-Host "║  ✓ Project references configured                              ║" -ForegroundColor Green
Write-Host "║  ✓ Solution compiles successfully                              ║" -ForegroundColor Green
Write-Host "╠════════════════════════════════════════════════════════════════╣" -ForegroundColor Green
Write-Host "║  Next Steps:                                                  ║" -ForegroundColor Green
Write-Host "║  1. Start infrastructure:                                     ║" -ForegroundColor Green
Write-Host "║     cd docker                                                 ║" -ForegroundColor Green
Write-Host "║     docker-compose up -d                                      ║" -ForegroundColor Green
Write-Host "║                                                               ║" -ForegroundColor Green
Write-Host "║  2. Verify connectivity:                                      ║" -ForegroundColor Green
Write-Host "║     docker-compose ps                                         ║" -ForegroundColor Green
Write-Host "║                                                               ║" -ForegroundColor Green
Write-Host "║  3. Proceed to Phase 2 (Foundational Infrastructure)         ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Green

Pop-Location

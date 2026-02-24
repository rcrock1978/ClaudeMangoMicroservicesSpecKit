# E2E Testing Guide - Phase 3

**Document Version:** 1.0
**Last Updated:** February 25, 2026

## Overview

This guide provides comprehensive instructions for running, understanding, and maintaining the End-to-End (E2E) integration tests for the Mango eCommerce Phase 3: Product Browsing & Checkout MVP.

## Table of Contents

1. [Setup Requirements](#setup-requirements)
2. [Test Environment Configuration](#test-environment-configuration)
3. [Running E2E Tests](#running-e2e-tests)
4. [Test Structure](#test-structure)
5. [Interpreting Test Results](#interpreting-test-results)
6. [Debugging Failed Tests](#debugging-failed-tests)
7. [CI/CD Integration](#cicd-integration)
8. [Best Practices](#best-practices)

## Setup Requirements

### System Requirements

- **OS:** Windows 10/11, macOS, or Linux
- **.NET SDK:** 10.0 or later
- **SQL Server:** 2019 Express or later (or use Docker)
- **RabbitMQ:** 3.12+ (or use Docker)
- **RAM:** Minimum 4GB (8GB recommended)
- **Disk Space:** 2GB minimum

### Software Requirements

```bash
# Required
- .NET 10.0 SDK
- Git

# Recommended for local development
- Visual Studio 2024 or VS Code
- Docker Desktop
- Postman (for API testing)
- SQL Server Management Studio
```

### Installation

```bash
# Install .NET 10.0 SDK
# https://dotnet.microsoft.com/download/dotnet/10.0

# Verify installation
dotnet --version

# Clone repository
git clone <repository-url>
cd mango-microservices

# Restore NuGet packages
dotnet restore
```

## Test Environment Configuration

### Local Development Setup

#### Option 1: Using Docker Compose (Recommended)

```bash
# Create docker-compose.yml (see Docker Compose Setup section)
docker-compose up -d

# Wait for services to be ready (30-60 seconds)
docker-compose ps
```

#### Option 2: Manual Local Setup

```bash
# 1. Start SQL Server locally or via Docker
docker run -e ACCEPT_EULA=Y -e SA_PASSWORD=Your@Password123 \
  -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

# 2. Start RabbitMQ
docker run -d --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3.12-management

# 3. Verify connectivity
# SQL Server: Server=localhost;User Id=sa;Password=Your@Password123
# RabbitMQ: http://localhost:15672 (guest/guest)
```

### Connection Strings

Update `appsettings.json` in each service:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Mango[ServiceName];User Id=sa;Password=Your@Password123;Encrypt=false;TrustServerCertificate=true;"
  },
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  }
}
```

### Database Initialization

```bash
# Initialize databases for each service
# Run from the service API project directory

# Product Service
cd src/Product/API/Mango.Services.Product.API
dotnet ef database update
cd ../../../..

# Shopping Cart Service
cd src/ShoppingCart/API/Mango.Services.ShoppingCart.API
dotnet ef database update
cd ../../../..

# Order Service
cd src/Order/API/Mango.Services.Order.API
dotnet ef database update
cd ../../../..

# Coupon Service
cd src/Coupon/API/Mango.Services.Coupon.API
dotnet ef database update
cd ../../../..

# Email Service
cd src/Email/API/Mango.Services.Email.API
dotnet ef database update
cd ../../../..

# Reward Service
cd src/Reward/API/Mango.Services.Reward.API
dotnet ef database update
cd ../../../..
```

## Running E2E Tests

### Option 1: Command Line (Recommended)

```bash
# Run all E2E tests with detailed output
dotnet test tests/Mango.E2E.Tests/Mango.E2E.Tests.csproj --logger "console;verbosity=detailed"

# Run with filtering by test class
dotnet test tests/Mango.E2E.Tests/Mango.E2E.Tests.csproj \
  --filter "FullyQualifiedName~CheckoutFlowTests"

# Run specific test
dotnet test tests/Mango.E2E.Tests/Mango.E2E.Tests.csproj \
  --filter "FullyQualifiedName~BrowseProducts_WithPagination_ReturnsPagedResults"

# Run with coverage report
dotnet test tests/Mango.E2E.Tests/Mango.E2E.Tests.csproj \
  /p:CollectCoverage=true \
  /p:CoverageFormat=opencover
```

### Option 2: Visual Studio

```
1. Open Test Explorer: Test > Test Explorer (Ctrl+E, T)
2. Navigate to Mango.E2E.Tests
3. Click "Run All" or right-click specific test to run
4. View results in Test Explorer window
```

### Option 3: VS Code

```bash
# Install C# extension and Test Explorer

# In VS Code:
1. Open Command Palette (Ctrl+Shift+P)
2. Type "Test: Run All Tests"
3. Or click on test gutter icons in editor
```

### Test Execution Order

Tests run in parallel by default. For sequential execution:

```bash
dotnet test tests/Mango.E2E.Tests/Mango.E2E.Tests.csproj \
  --logger "console;verbosity=detailed" -- MaxParallelThreads=1
```

## Test Structure

### Project Organization

```
Mango.E2E.Tests/
├── Fixtures/
│   ├── E2ETestFixture.cs              # Shared test infrastructure
│   └── E2ETestCollection.cs           # Test collection definition
├── Integration/
│   └── CheckoutFlowTests.cs           # 20 E2E tests
├── Helpers/
│   └── TestDataBuilder.cs             # Test data utilities
├── Common/
│   └── (shared utilities)
└── Mango.E2E.Tests.csproj             # Project file
```

### Test Naming Convention

Tests follow the pattern: `MethodUnderTest_Scenario_ExpectedResult`

Example:
```csharp
public async Task BrowseProducts_WithPagination_ReturnsPagedResults()
```

### Test Structure Pattern (AAA)

Every E2E test follows the Arrange-Act-Assert pattern:

```csharp
[Fact]
public async Task CreateOrder_WithDiscount_SuccessfullyCreatesOrder()
{
    // Arrange: Set up test data
    var userId = TestDataBuilder.CreateTestUserId();
    var orderRequest = new { /* ... */ };

    // Act: Execute the action
    var response = await _fixture.OrderClient!.PostAsJsonAsync("/api/orders", orderRequest);

    // Assert: Verify results
    response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
}
```

## Interpreting Test Results

### Successful Test Run

```
Total tests: 20
Passed: 20
Failed: 0
Skipped: 0

Execution time: 45.2 seconds
```

### Understanding Test Output

```
CheckoutFlowTests
  ✓ BrowseProducts_WithPagination_ReturnsPagedResults (125ms)
  ✓ SearchProducts_WithValidSearchTerm_ReturnsMatchingProducts (89ms)
  ✗ ApplyValidCoupon_WithValidCouponCode_CalculatesDiscount (500ms)
    Error: Expected status code 200, but got 404
```

### HTTP Status Code Reference

| Code | Meaning | Action |
|------|---------|--------|
| 200 | OK | Test should pass |
| 201 | Created | Test should pass (resource created) |
| 400 | Bad Request | Validation error - check request format |
| 404 | Not Found | Resource doesn't exist - check data |
| 500 | Internal Server | Service error - check service logs |

## Debugging Failed Tests

### Step 1: Check Service Health

```bash
# Verify each service is running
curl http://localhost:5001/health  # Product
curl http://localhost:5002/health  # Shopping Cart
curl http://localhost:5003/health  # Order
curl http://localhost:5004/health  # Coupon
curl http://localhost:5005/health  # Email
curl http://localhost:5006/health  # Reward

# Expected response: { "status": "Healthy" } or similar
```

### Step 2: Check Database Connectivity

```bash
# SQL Server connection
sqlcmd -S localhost -U sa -P "Your@Password123" -Q "SELECT 1"

# Should return: (1 row affected)
```

### Step 3: Check RabbitMQ

```bash
# RabbitMQ Management UI
# Open browser: http://localhost:15672
# Login: guest / guest

# Check:
# - Connections: Should show active connections
# - Queues: Should see message queues
# - Exchanges: Should be available
```

### Step 4: Examine Test Logs

```bash
# Run test with detailed logging
dotnet test tests/Mango.E2E.Tests/Mango.E2E.Tests.csproj \
  --logger "console;verbosity=diagnostic" 2>&1 | tee test-output.log

# Check the generated test-output.log file
```

### Step 5: Debug in IDE

```csharp
// Add breakpoint in test
[Fact]
public async Task CreateOrder_WithDiscount_SuccessfullyCreatesOrder()
{
    // Add breakpoint here
    var response = await _fixture.OrderClient!.PostAsJsonAsync("/api/orders", orderRequest);

    // Examine response
    var content = await response.Content.ReadAsStringAsync();
    System.Diagnostics.Debug.WriteLine($"Response: {content}");
}
```

### Common Issues and Solutions

#### Issue: "Address already in use"

```bash
# Port is already in use by another process
# Solution: Change port or stop conflicting service

# Find process using port 5001
lsof -i :5001  # macOS/Linux
netstat -ano | findstr :5001  # Windows

# Kill process
kill -9 <PID>  # macOS/Linux
taskkill /PID <PID> /F  # Windows
```

#### Issue: "Connection refused"

```bash
# Service is not running
# Solution: Start the service

dotnet run -p src/Product/API/Mango.Services.Product.API/
```

#### Issue: "Database connection timeout"

```bash
# SQL Server is not accessible
# Solution: Verify connection string

# Test connection
sqlcmd -S localhost -U sa -P "Your@Password123" -Q "SELECT 1"
```

#### Issue: "MassTransit timeout waiting for message"

```bash
# RabbitMQ is not running or not accessible
# Solution: Start RabbitMQ

docker run -d --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3.12-management
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: E2E Tests

on: [push, pull_request]

jobs:
  e2e-tests:
    runs-on: ubuntu-latest

    services:
      sql-server:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: YourPassword123
        options: >-
          --health-cmd="/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourPassword123 -Q 'SELECT 1'"
          --health-interval=10s
          --health-timeout=5s
          --health-retries=3
      rabbitmq:
        image: rabbitmq:3.12-management
        options: >-
          --health-cmd="rabbitmq-diagnostics -q ping"
          --health-interval=10s
          --health-timeout=5s
          --health-retries=3

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Run E2E Tests
        run: dotnet test tests/Mango.E2E.Tests/Mango.E2E.Tests.csproj --logger "console;verbosity=detailed"
```

### Jenkins Pipeline Example

```groovy
pipeline {
    agent any

    stages {
        stage('Setup') {
            steps {
                sh '''
                    docker-compose up -d
                    sleep 30
                '''
            }
        }

        stage('Build') {
            steps {
                sh 'dotnet build'
            }
        }

        stage('E2E Tests') {
            steps {
                sh 'dotnet test tests/Mango.E2E.Tests/Mango.E2E.Tests.csproj'
            }
        }

        stage('Cleanup') {
            steps {
                sh 'docker-compose down'
            }
        }
    }

    post {
        always {
            junit 'test-results/**/*.xml'
        }
    }
}
```

## Best Practices

### 1. Test Isolation

- Each test should be independent
- Use unique user IDs for each test
- Clean up test data after each test

```csharp
[Fact]
public async Task CreateOrder_WithDiscount_SuccessfullyCreatesOrder()
{
    // Use unique user ID to avoid test collision
    var userId = TestDataBuilder.CreateTestUserId();

    // ... test code ...
}
```

### 2. Async/Await Patterns

- Always use async methods for HTTP calls
- Never use `.Result` or `.Wait()`
- Use `async Task` for test methods

```csharp
// Good
public async Task TestMethod()
{
    var response = await _fixture.ProductClient!.GetAsync("/api/products");
}

// Bad - avoid blocking calls
public void TestMethod()
{
    var response = _fixture.ProductClient!.GetAsync("/api/products").Result;
}
```

### 3. Assertion Best Practices

- Use FluentAssertions for readable assertions
- Test one behavior per test
- Include meaningful error messages

```csharp
// Good
response.StatusCode.Should().Be(HttpStatusCode.OK)
    .Because("product should be retrieved successfully");

// Bad
Assert.True(response.IsSuccessStatusCode);
```

### 4. Test Data Management

- Use TestDataBuilder for consistency
- Avoid hardcoded values
- Generate unique identifiers

```csharp
// Good
var userId = TestDataBuilder.CreateTestUserId();

// Bad
var userId = "testuser123";  // Could conflict with other tests
```

### 5. Performance Considerations

- Keep tests fast (< 5 seconds per test)
- Minimize database operations
- Use in-memory databases when possible

```csharp
// Run multiple assertions in one test if they're related
[Fact]
public async Task OrderLifecycle_CompletesSuccessfully()
{
    // Create
    var response = await CreateOrder();
    response.StatusCode.Should().Be(HttpStatusCode.Created);

    // Confirm
    var confirmResponse = await ConfirmOrder(orderId);
    confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### 6. Error Handling

- Test both success and error cases
- Verify error messages are meaningful
- Handle transient failures gracefully

```csharp
// Good - test error scenario
[Fact]
public async Task ApplyInvalidCoupon_ReturnsBadRequest()
{
    var response = await _fixture.CouponClient!
        .GetAsync("/api/coupon/validate?code=INVALID&cartTotal=100");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

## Troubleshooting Checklist

- [ ] .NET SDK version 10.0 or higher installed
- [ ] SQL Server running and accessible
- [ ] RabbitMQ running and accessible
- [ ] All services registered in E2ETestFixture
- [ ] Connection strings in appsettings.json correct
- [ ] Databases created and migrations applied
- [ ] Firewall allows localhost connections
- [ ] No port conflicts on service ports
- [ ] Test data builder creating unique identifiers
- [ ] Async/await patterns used correctly

## Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Guide](https://fluentassertions.com/)
- [MassTransit Testing](https://masstransit.io/documentation/configuration/testing)
- [HTTP Status Codes](https://httpwg.org/specs/rfc9110.html#overview.of.status.codes)

## Support

For issues with E2E tests:

1. Check this guide's troubleshooting section
2. Review test output and logs
3. Consult service-specific documentation
4. Check GitHub issues or project board
5. Contact development team

---

*Last Updated: February 25, 2026*
*Version: 1.0*

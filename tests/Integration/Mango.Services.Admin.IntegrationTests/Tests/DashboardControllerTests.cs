using System.Net;
using FluentAssertions;
using Xunit;
using Mango.Services.Admin.IntegrationTests.Fixtures;

namespace Mango.Services.Admin.IntegrationTests.Tests;

/// <summary>
/// Integration tests for Admin Dashboard Controller endpoints.
/// Tests API contract, HTTP status codes, and data aggregation.
/// </summary>
public class DashboardControllerTests : IClassFixture<AdminApiTestFixture>
{
    private readonly AdminApiTestFixture _fixture;
    private readonly HttpClient _client;

    public DashboardControllerTests(AdminApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Arrange
        var endpoint = "/api/admin/dashboard/health";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task GetDashboardKpis_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/dashboard/kpis";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRevenueMetrics_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/dashboard/revenue";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProductMetrics_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/dashboard/products";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomerInsights_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/dashboard/customers";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCouponAnalytics_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/dashboard/coupons";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AllDashboardEndpoints_AreSecured()
    {
        // Arrange
        var endpoints = new[]
        {
            "/api/admin/dashboard/kpis",
            "/api/admin/dashboard/revenue",
            "/api/admin/dashboard/products",
            "/api/admin/dashboard/customers",
            "/api/admin/dashboard/coupons"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should()
                .Be(HttpStatusCode.Unauthorized,
                    $"Endpoint {endpoint} should require authentication");
        }
    }

    [Fact]
    public async Task HealthCheckEndpoint_IsPublic()
    {
        // Arrange
        var endpoint = "/api/admin/dashboard/health";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

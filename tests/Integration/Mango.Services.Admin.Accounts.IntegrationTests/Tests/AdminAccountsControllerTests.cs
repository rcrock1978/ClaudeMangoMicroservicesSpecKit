using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using Mango.Services.Admin.Accounts.Application.DTOs;
using Mango.Services.Admin.Accounts.IntegrationTests.Fixtures;

namespace Mango.Services.Admin.Accounts.IntegrationTests.Tests;

/// <summary>
/// Integration tests for Admin Accounts Controller endpoints.
/// Tests API contract, HTTP status codes, and response formats.
/// </summary>
public class AdminAccountsControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public AdminAccountsControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Arrange
        var endpoint = "/api/admin/accounts/health";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task ValidateApiKey_WithEmptyKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/accounts/validate";
        var request = new ValidateApiKeyRequest { ApiKey = "" };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(endpoint, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidateApiKey_WithInvalidKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/accounts/validate";
        var request = new ValidateApiKeyRequest { ApiKey = "invalid-key-12345" };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(endpoint, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Invalid API key");
    }

    [Fact]
    public async Task CreateAdminUser_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/accounts";
        var request = new CreateAdminUserRequest
        {
            Email = "admin@example.com",
            FullName = "Test Admin",
            Role = Domain.Entities.AdminRole.ADMIN
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(endpoint, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAdminUserById_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/accounts/1";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListAdminUsers_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/accounts";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAdminUser_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var endpoint = "/api/admin/accounts";
        var request = new CreateAdminUserRequest
        {
            Email = "", // Invalid: empty email
            FullName = "Test Admin"
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Add valid authorization header from fixture
        if (!string.IsNullOrEmpty(_fixture.ValidApiKey))
        {
            _client.DefaultRequestHeaders.Add("X-API-Key", _fixture.ValidApiKey);
        }

        // Act
        var response = await _client.PostAsync(endpoint, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _client.DefaultRequestHeaders.Remove("X-API-Key");
    }

    [Fact]
    public async Task UpdateAdminUser_WithValidId_ShouldAttemptUpdate()
    {
        // Arrange
        var endpoint = "/api/admin/accounts/1";
        var request = new UpdateAdminUserRequest
        {
            FullName = "Updated Name",
            IsActive = true
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync(endpoint, content);

        // Assert
        // Should return 401 because no API key provided, not 404
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateApiKey_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/accounts/1/api-keys";

        // Act
        var response = await _client.PostAsync(endpoint, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RevokeApiKey_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/accounts/1/api-keys";

        // Act
        var response = await _client.DeleteAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivateAdminUser_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = "/api/admin/accounts/1";

        // Act
        var response = await _client.DeleteAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

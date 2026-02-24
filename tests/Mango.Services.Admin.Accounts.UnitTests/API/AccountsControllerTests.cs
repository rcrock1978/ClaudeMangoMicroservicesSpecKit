using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mango.Services.Admin.Accounts.API.Controllers;
using Mango.Services.Admin.Accounts.Application.DTOs;
using Mango.Services.Admin.Accounts.Domain.Entities;
using Moq;

namespace Mango.Services.Admin.Accounts.UnitTests.API;

public class AccountsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AccountsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task ValidateApiKey_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new ValidateApiKeyRequest { ApiKey = "valid_key_123" };
        var expectedResponse = new ValidateApiKeyResponse
        {
            IsValid = true,
            AdminUserId = 1,
            AdminUser = new AdminUserDto
            {
                Id = 1,
                Email = "admin@test.com",
                FullName = "Test Admin",
                Role = AdminRole.SUPER_ADMIN,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ValidateApiKey(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidateApiKey_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("ApiKey", "Required");
        var request = new ValidateApiKeyRequest { ApiKey = "" };

        // Act
        var result = await _controller.ValidateApiKey(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void HealthCheck_ReturnsOkWithHealthyStatus()
    {
        // Act
        var result = _controller.HealthCheck();

        // Assert
        result.Should().NotBeNull();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }
}

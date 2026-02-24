using FluentAssertions;
using Mango.Services.Admin.Accounts.Domain.Entities;
using Moq;

namespace Mango.Services.Admin.Accounts.UnitTests.Domain;

public class AdminApiKeyTests
{
    [Fact]
    public void IsValid_WithActiveUnexpiredKey_ReturnsTrue()
    {
        // Arrange
        var apiKey = new AdminApiKey
        {
            AdminUserId = 1,
            KeyHash = "hashed_key",
            KeyPrefix = "key12345",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        // Act
        var result = apiKey.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithRevokedKey_ReturnsFalse()
    {
        // Arrange
        var apiKey = new AdminApiKey
        {
            AdminUserId = 1,
            KeyHash = "hashed_key",
            KeyPrefix = "key12345",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        };

        // Act
        var result = apiKey.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithExpiredKey_ReturnsFalse()
    {
        // Arrange
        var apiKey = new AdminApiKey
        {
            AdminUserId = 1,
            KeyHash = "hashed_key",
            KeyPrefix = "key12345",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        // Act
        var result = apiKey.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithPastDate_ReturnsTrue()
    {
        // Arrange
        var apiKey = new AdminApiKey
        {
            AdminUserId = 1,
            KeyHash = "hashed_key",
            KeyPrefix = "key12345",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        // Act
        var result = apiKey.IsExpired();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WithFutureDate_ReturnsFalse()
    {
        // Arrange
        var apiKey = new AdminApiKey
        {
            AdminUserId = 1,
            KeyHash = "hashed_key",
            KeyPrefix = "key12345",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        // Act
        var result = apiKey.IsExpired();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateKey_WithValidKey_ReturnsTrue()
    {
        // Arrange
        var plainKey = "test_api_key_123";
        var hashingServiceMock = new Mock<IApiKeyHashingService>();
        hashingServiceMock
            .Setup(s => s.VerifyKey(plainKey, It.IsAny<string>()))
            .Returns(true);

        var apiKey = new AdminApiKey
        {
            AdminUserId = 1,
            KeyHash = "hashed_key",
            KeyPrefix = "test1234",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        // Act
        var result = apiKey.ValidateKey(plainKey, hashingServiceMock.Object);

        // Assert
        result.Should().BeTrue();
        hashingServiceMock.Verify(s => s.VerifyKey(plainKey, "hashed_key"), Times.Once);
    }

    [Fact]
    public void ValidateKey_WithInvalidKey_ReturnsFalse()
    {
        // Arrange
        var plainKey = "test_api_key_123";
        var wrongKey = "wrong_key";
        var hashingServiceMock = new Mock<IApiKeyHashingService>();
        hashingServiceMock
            .Setup(s => s.VerifyKey(plainKey, It.IsAny<string>()))
            .Returns(false);

        var apiKey = new AdminApiKey
        {
            AdminUserId = 1,
            KeyHash = "hashed_key",
            KeyPrefix = "test1234",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        // Act
        var result = apiKey.ValidateKey(wrongKey, hashingServiceMock.Object);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateKey_WithExpiredKey_ReturnsFalse()
    {
        // Arrange
        var plainKey = "test_api_key_123";
        var hashingServiceMock = new Mock<IApiKeyHashingService>();
        hashingServiceMock
            .Setup(s => s.VerifyKey(plainKey, It.IsAny<string>()))
            .Returns(true);

        var apiKey = new AdminApiKey
        {
            AdminUserId = 1,
            KeyHash = "hashed_key",
            KeyPrefix = "test1234",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        // Act
        var result = apiKey.ValidateKey(plainKey, hashingServiceMock.Object);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Revoke_SetsIsRevokedTrueAndRevokedAt()
    {
        // Arrange
        var beforeTime = DateTime.UtcNow;
        var apiKey = new AdminApiKey
        {
            AdminUserId = 1,
            KeyHash = "hashed_key",
            KeyPrefix = "test1234",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        // Act
        apiKey.Revoke();
        var afterTime = DateTime.UtcNow;

        // Assert
        apiKey.IsRevoked.Should().BeTrue();
        apiKey.RevokedAt.Should().NotBeNull();
        apiKey.RevokedAt.Should().BeOnOrAfter(beforeTime);
        apiKey.RevokedAt.Should().BeOnOrBefore(afterTime);
    }
}

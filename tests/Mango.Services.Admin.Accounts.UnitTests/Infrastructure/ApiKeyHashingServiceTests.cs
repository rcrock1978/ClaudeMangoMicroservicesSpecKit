using FluentAssertions;
using Mango.Services.Admin.Accounts.Infrastructure.Services;

namespace Mango.Services.Admin.Accounts.UnitTests.Infrastructure;

public class ApiKeyHashingServiceTests
{
    private readonly ApiKeyHashingService _service;

    public ApiKeyHashingServiceTests()
    {
        _service = new ApiKeyHashingService();
    }

    [Fact]
    public void HashKey_WithValidKey_ReturnsHashedString()
    {
        // Arrange
        var plainKey = "test_api_key_12345";

        // Act
        var hash = _service.HashKey(plainKey);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(plainKey);
        hash.Length.Should().BeGreaterThan(plainKey.Length);
    }

    [Fact]
    public void HashKey_WithEmptyKey_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => _service.HashKey("");
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void HashKey_SameKeyGeneratesDifferentHash()
    {
        // Arrange
        var plainKey = "test_api_key_12345";

        // Act
        var hash1 = _service.HashKey(plainKey);
        var hash2 = _service.HashKey(plainKey);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyKey_WithCorrectKey_ReturnsTrue()
    {
        // Arrange
        var plainKey = "test_api_key_12345";
        var hash = _service.HashKey(plainKey);

        // Act
        var result = _service.VerifyKey(plainKey, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyKey_WithIncorrectKey_ReturnsFalse()
    {
        // Arrange
        var plainKey = "test_api_key_12345";
        var wrongKey = "wrong_key_67890";
        var hash = _service.HashKey(plainKey);

        // Act
        var result = _service.VerifyKey(wrongKey, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyKey_WithEmptyKey_ReturnsFalse()
    {
        // Arrange
        var plainKey = "test_api_key_12345";
        var hash = _service.HashKey(plainKey);

        // Act
        var result = _service.VerifyKey("", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyKey_WithEmptyHash_ReturnsFalse()
    {
        // Arrange
        var plainKey = "test_api_key_12345";

        // Act
        var result = _service.VerifyKey(plainKey, "");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyKey_WithNullKey_ReturnsFalse()
    {
        // Arrange
        var plainKey = "test_api_key_12345";
        var hash = _service.HashKey(plainKey);

        // Act
        var result = _service.VerifyKey(null!, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyKey_WithInvalidHash_ReturnsFalse()
    {
        // Arrange
        var plainKey = "test_api_key_12345";
        var invalidHash = "not_a_valid_bcrypt_hash";

        // Act
        var result = _service.VerifyKey(plainKey, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HashAndVerify_RoundTrip_Success()
    {
        // Arrange
        var plainKey = "my_secure_api_key_abc123xyz";

        // Act
        var hash = _service.HashKey(plainKey);
        var verified = _service.VerifyKey(plainKey, hash);

        // Assert
        verified.Should().BeTrue();
    }
}

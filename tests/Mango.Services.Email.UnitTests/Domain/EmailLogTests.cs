using FluentAssertions;
using Mango.Services.Email.Domain.Entities;

namespace Mango.Services.Email.UnitTests.Domain;

/// <summary>
/// Unit tests for EmailLog entity and email tracking operations.
/// </summary>
public class EmailLogTests
{
    private EmailLog CreateValidLog()
    {
        return new EmailLog
        {
            Id = 1,
            RecipientEmail = "customer@example.com",
            RecipientName = "John Doe",
            Subject = "Order Confirmation",
            Body = "<p>Your order has been confirmed</p>",
            TemplateName = "OrderConfirmation",
            UserId = "user123",
            EmailType = "OrderConfirmation",
            OrderId = 456,
            AttemptCount = 0,
            IsSent = false
        };
    }

    [Fact]
    public void IsValid_WithValidLog_ShouldReturnTrue()
    {
        // Arrange
        var log = CreateValidLog();

        // Act
        var result = log.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithMissingEmail_ShouldReturnFalse()
    {
        // Arrange
        var log = CreateValidLog();
        log.RecipientEmail = string.Empty;

        // Act
        var result = log.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingSubject_ShouldReturnFalse()
    {
        // Arrange
        var log = CreateValidLog();
        log.Subject = string.Empty;

        // Act
        var result = log.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingBody_ShouldReturnFalse()
    {
        // Arrange
        var log = CreateValidLog();
        log.Body = string.Empty;

        // Act
        var result = log.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkAsSent_ShouldSetSentStatus()
    {
        // Arrange
        var log = CreateValidLog();
        var beforeTime = DateTime.UtcNow;

        // Act
        log.MarkAsSent();

        // Assert
        log.IsSent.Should().BeTrue();
        log.SentAt.Should().NotBeNull();
        log.SentAt.Should().BeOnOrAfter(beforeTime);
        log.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void MarkAsSent_ShouldUpdateLastAttemptTime()
    {
        // Arrange
        var log = CreateValidLog();
        var beforeTime = DateTime.UtcNow;

        // Act
        log.MarkAsSent();

        // Assert
        log.LastAttemptAt.Should().NotBeNull();
        log.LastAttemptAt.Should().BeOnOrAfter(beforeTime);
    }

    [Fact]
    public void RecordFailedAttempt_ShouldIncrementAttemptCount()
    {
        // Arrange
        var log = CreateValidLog();
        log.AttemptCount = 0;

        // Act
        log.RecordFailedAttempt("SMTP connection failed");

        // Assert
        log.AttemptCount.Should().Be(1);
    }

    [Fact]
    public void RecordFailedAttempt_ShouldSetErrorMessage()
    {
        // Arrange
        var log = CreateValidLog();
        var errorMsg = "SMTP connection timeout";

        // Act
        log.RecordFailedAttempt(errorMsg);

        // Assert
        log.ErrorMessage.Should().Be(errorMsg);
    }

    [Fact]
    public void RecordFailedAttempt_ShouldMarkAsNotSent()
    {
        // Arrange
        var log = CreateValidLog();
        log.IsSent = true;

        // Act
        log.RecordFailedAttempt("Delivery failed");

        // Assert
        log.IsSent.Should().BeFalse();
    }

    [Fact]
    public void RecordFailedAttempt_MultipleAttempts_ShouldCumulateCount()
    {
        // Arrange
        var log = CreateValidLog();

        // Act
        log.RecordFailedAttempt("Attempt 1 failed");
        log.RecordFailedAttempt("Attempt 2 failed");
        log.RecordFailedAttempt("Attempt 3 failed");

        // Assert
        log.AttemptCount.Should().Be(3);
    }

    [Fact]
    public void ShouldRetry_WithinMaxAttempts_ShouldReturnTrue()
    {
        // Arrange
        var log = CreateValidLog();
        log.AttemptCount = 2;
        log.IsSent = false;

        // Act
        var result = log.ShouldRetry(maxAttempts: 3);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_ExceededMaxAttempts_ShouldReturnFalse()
    {
        // Arrange
        var log = CreateValidLog();
        log.AttemptCount = 3;
        log.IsSent = false;

        // Act
        var result = log.ShouldRetry(maxAttempts: 3);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_AlreadySent_ShouldReturnFalse()
    {
        // Arrange
        var log = CreateValidLog();
        log.AttemptCount = 1;
        log.IsSent = true;

        // Act
        var result = log.ShouldRetry(maxAttempts: 3);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldSetDefaults()
    {
        // Arrange & Act
        var log = new EmailLog
        {
            RecipientEmail = "test@example.com",
            Subject = "Test",
            Body = "Test body"
        };

        // Assert
        log.IsSent.Should().BeFalse();
        log.AttemptCount.Should().Be(0);
        log.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        log.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void EmailLog_CanTrackOrderAssociation()
    {
        // Arrange
        var log = CreateValidLog();

        // Act
        log.OrderId = 789;

        // Assert
        log.OrderId.Should().Be(789);
    }

    [Fact]
    public void EmailLog_CanTrackUserAssociation()
    {
        // Arrange
        var log = CreateValidLog();

        // Act
        log.UserId = "user456";

        // Assert
        log.UserId.Should().Be("user456");
    }

    [Fact]
    public void EmailLog_CanCategorizeByType()
    {
        // Arrange
        var log = new EmailLog
        {
            RecipientEmail = "test@example.com",
            Subject = "Password Reset",
            Body = "Click to reset",
            EmailType = "PasswordReset"
        };

        // Act & Assert
        log.EmailType.Should().Be("PasswordReset");
    }
}

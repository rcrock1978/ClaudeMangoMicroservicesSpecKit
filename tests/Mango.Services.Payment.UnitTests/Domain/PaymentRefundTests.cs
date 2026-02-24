namespace Mango.Services.Payment.UnitTests.Domain;

using Xunit;
using Mango.Services.Payment.Domain;

/// <summary>
/// Unit tests for PaymentRefund entity.
/// </summary>
public class PaymentRefundTests
{
    [Fact]
    public void ValidateRefund_ValidData_ReturnsTrue()
    {
        // Arrange
        var refund = new PaymentRefund
        {
            PaymentId = 1,
            RefundAmount = 50m,
            Reason = "Customer requested"
        };

        // Act
        var result = refund.ValidateRefund(100m);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateRefund_ZeroAmount_ReturnsFalse()
    {
        // Arrange
        var refund = new PaymentRefund
        {
            PaymentId = 1,
            RefundAmount = 0m,
            Reason = "Customer requested"
        };

        // Act
        var result = refund.ValidateRefund(100m);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateRefund_ExceedsMaximum_ReturnsFalse()
    {
        // Arrange
        var refund = new PaymentRefund
        {
            PaymentId = 1,
            RefundAmount = 150m,
            Reason = "Customer requested"
        };

        // Act
        var result = refund.ValidateRefund(100m);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateRefund_MissingReason_ReturnsFalse()
    {
        // Arrange
        var refund = new PaymentRefund
        {
            PaymentId = 1,
            RefundAmount = 50m,
            Reason = ""
        };

        // Act
        var result = refund.ValidateRefund(100m);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MarkAsProcessed_ValidGatewayId_Succeeds()
    {
        // Arrange
        var refund = new PaymentRefund
        {
            PaymentId = 1,
            RefundAmount = 50m,
            Reason = "Customer requested",
            Status = PaymentStatus.Processing
        };

        // Act
        var result = refund.MarkAsProcessed("refund_123");

        // Assert
        Assert.True(result);
        Assert.Equal(PaymentStatus.Completed, refund.Status);
        Assert.Equal("refund_123", refund.GatewayRefundId);
        Assert.NotNull(refund.ProcessedDate);
        Assert.Null(refund.ErrorMessage);
    }

    [Fact]
    public void MarkAsProcessed_InvalidStatus_ReturnsFalse()
    {
        // Arrange
        var refund = new PaymentRefund
        {
            Status = PaymentStatus.Completed
        };

        // Act
        var result = refund.MarkAsProcessed("refund_123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MarkAsFailed_ValidError_Succeeds()
    {
        // Arrange
        var refund = new PaymentRefund
        {
            PaymentId = 1,
            Status = PaymentStatus.Processing
        };

        // Act
        var result = refund.MarkAsFailed("Gateway error");

        // Assert
        Assert.True(result);
        Assert.Equal(PaymentStatus.Failed, refund.Status);
        Assert.Equal("Gateway error", refund.ErrorMessage);
        Assert.NotNull(refund.ProcessedDate);
    }

    [Fact]
    public void MarkAsFailed_EmptyError_ReturnsFalse()
    {
        // Arrange
        var refund = new PaymentRefund();

        // Act
        var result = refund.MarkAsFailed("");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetValidationErrors_ValidRefund_ReturnsEmpty()
    {
        // Arrange
        var refund = new PaymentRefund
        {
            PaymentId = 1,
            RefundAmount = 50m,
            Reason = "Customer requested"
        };

        // Act
        var errors = refund.GetValidationErrors();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void GetValidationErrors_InvalidPaymentId_ReturnsError()
    {
        // Arrange
        var refund = new PaymentRefund
        {
            PaymentId = 0,
            RefundAmount = 50m,
            Reason = "Customer requested"
        };

        // Act
        var errors = refund.GetValidationErrors();

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains("PaymentId is required", errors);
    }
}

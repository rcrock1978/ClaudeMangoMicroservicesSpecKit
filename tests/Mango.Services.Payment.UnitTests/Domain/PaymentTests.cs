namespace Mango.Services.Payment.UnitTests.Domain;

using Xunit;
using Mango.Services.Payment.Domain;

/// <summary>
/// Unit tests for Payment entity.
/// </summary>
public class PaymentTests
{
    [Fact]
    public void InitiatePayment_ValidState_TransitionsToProcessing()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = 100m,
            Currency = "USD"
        };

        // Act
        var result = payment.InitiatePayment();

        // Assert
        Assert.True(result);
        Assert.Equal(PaymentStatus.Processing, payment.Status);
    }

    [Fact]
    public void InitiatePayment_InvalidAmount_ReturnsFalse()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = -10m,
            Currency = "USD"
        };

        // Act
        var result = payment.InitiatePayment();

        // Assert
        Assert.False(result);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void CompletePayment_ValidState_TransitionsToCompleted()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = 100m,
            Currency = "USD",
            Status = PaymentStatus.Processing
        };

        // Act
        var result = payment.CompletePayment("txn_123");

        // Assert
        Assert.True(result);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal("txn_123", payment.TransactionId);
        Assert.NotNull(payment.PaymentDate);
    }

    [Fact]
    public void CompletePayment_InvalidStatus_ReturnsFalse()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = 100m,
            Currency = "USD",
            Status = PaymentStatus.Completed
        };

        // Act
        var result = payment.CompletePayment("txn_123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FailPayment_ValidState_TransitionsToFailed()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = 100m,
            Currency = "USD",
            Status = PaymentStatus.Processing
        };

        // Act
        var result = payment.FailPayment("Card declined");

        // Assert
        Assert.True(result);
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal("Card declined", payment.ErrorMessage);
    }

    [Fact]
    public void RefundPayment_ValidState_UpdatesRefundAmount()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = 100m,
            Currency = "USD",
            Status = PaymentStatus.Completed,
            TransactionId = "txn_123"
        };

        // Act
        var result = payment.RefundPayment(50m, "Customer requested");

        // Assert
        Assert.True(result);
        Assert.Equal(50m, payment.RefundedAmount);
        Assert.NotNull(payment.RefundDate);
    }

    [Fact]
    public void RefundPayment_ExceedsAmount_ReturnsFalse()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = 100m,
            Currency = "USD",
            Status = PaymentStatus.Completed
        };

        // Act
        var result = payment.RefundPayment(150m, "Customer requested");

        // Assert
        Assert.False(result);
        Assert.Equal(0m, payment.RefundedAmount);
    }

    [Fact]
    public void RefundPayment_FullRefund_TransitionsToRefunded()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = 100m,
            Currency = "USD",
            Status = PaymentStatus.Completed
        };

        // Act
        var result = payment.RefundPayment(100m, "Full refund");

        // Assert
        Assert.True(result);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(100m, payment.RefundedAmount);
    }

    [Fact]
    public void CanBeRefunded_CompletedPayment_ReturnsTrue()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = 100m,
            Currency = "USD",
            Status = PaymentStatus.Completed,
            RefundedAmount = 0m
        };

        // Act
        var result = payment.CanBeRefunded();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanBeRefunded_FailedPayment_ReturnsFalse()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = 100m,
            Currency = "USD",
            Status = PaymentStatus.Failed
        };

        // Act
        var result = payment.CanBeRefunded();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetRefundableAmount_PartiallyRefunded_ReturnsCorrectAmount()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = 100m,
            Currency = "USD",
            RefundedAmount = 30m
        };

        // Act
        var refundable = payment.GetRefundableAmount();

        // Assert
        Assert.Equal(70m, refundable);
    }

    [Fact]
    public void CancelPayment_PendingStatus_Succeeds()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = 100m,
            Currency = "USD",
            Status = PaymentStatus.Pending
        };

        // Act
        var result = payment.CancelPayment();

        // Assert
        Assert.True(result);
        Assert.Equal(PaymentStatus.Cancelled, payment.Status);
    }

    [Fact]
    public void CancelPayment_CompletedStatus_Fails()
    {
        // Arrange
        var payment = new Payment
        {
            OrderId = 1,
            UserId = "user1",
            Amount = 100m,
            Currency = "USD",
            Status = PaymentStatus.Completed
        };

        // Act
        var result = payment.CancelPayment();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTerminal_CompletedStatus_ReturnsTrue()
    {
        // Arrange
        var payment = new Payment { Status = PaymentStatus.Completed };

        // Act
        var result = payment.IsTerminal();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTerminal_ProcessingStatus_ReturnsFalse()
    {
        // Arrange
        var payment = new Payment { Status = PaymentStatus.Processing };

        // Act
        var result = payment.IsTerminal();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateAmount_WithinLimits_ReturnsEmpty()
    {
        // Arrange
        var payment = new Payment { Amount = 50m, Currency = "USD" };

        // Act
        var errors = payment.ValidateAmount(1m, 100m);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateAmount_BelowMinimum_ReturnsError()
    {
        // Arrange
        var payment = new Payment { Amount = 0.25m, Currency = "USD" };

        // Act
        var errors = payment.ValidateAmount(1m, 100m);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains("must be at least", errors.First());
    }

    [Fact]
    public void ValidateAmount_ExceedsMaximum_ReturnsError()
    {
        // Arrange
        var payment = new Payment { Amount = 150m, Currency = "USD" };

        // Act
        var errors = payment.ValidateAmount(1m, 100m);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains("cannot exceed", errors.First());
    }

    [Fact]
    public void GetValidationErrors_ValidPayment_ReturnsEmpty()
    {
        // Arrange
        var payment = new Payment
        {
            Amount = 100m,
            UserId = "user1",
            OrderId = 1,
            Currency = "USD",
            Status = PaymentStatus.Completed,
            TransactionId = "txn_123"
        };

        // Act
        var errors = payment.GetValidationErrors();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void GetValidationErrors_MissingUserId_ReturnsError()
    {
        // Arrange
        var payment = new Payment
        {
            Amount = 100m,
            UserId = "",
            OrderId = 1,
            Currency = "USD"
        };

        // Act
        var errors = payment.GetValidationErrors();

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains("UserId is required", errors);
    }
}

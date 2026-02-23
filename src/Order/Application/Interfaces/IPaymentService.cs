namespace Mango.Services.Order.Application.Interfaces;

/// <summary>
/// Abstraction for payment processing, enabling provider-agnostic implementation.
/// Currently implemented via Stripe, but can be swapped for other providers (PayPal, Square, etc).
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Create a checkout session for order payment processing.
    /// Returns Stripe session ID for client-side redirect.
    /// </summary>
    Task<CreateCheckoutSessionResult> CreateCheckoutSessionAsync(
        OrderCheckoutDetails order,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate payment webhook and confirm order.
    /// Idempotent operation - safe to call multiple times for same payment.
    /// </summary>
    Task<ValidatePaymentResult> ValidatePaymentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refund a processed payment (for order cancellations).
    /// </summary>
    Task<RefundResult> RefundAsync(
        string paymentIntentId,
        decimal amount,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request DTO for checkout session creation.
/// </summary>
public class OrderCheckoutDetails
{
    public int OrderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
    public List<CheckoutItem> Items { get; set; } = new();
    public string? SuccessUrl { get; set; }
    public string? CancelUrl { get; set; }

    public class CheckoutItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}

/// <summary>
/// Response from checkout session creation.
/// </summary>
public class CreateCheckoutSessionResult
{
    public bool IsSuccess { get; set; }
    public string? SessionId { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of payment validation.
/// </summary>
public class ValidatePaymentResult
{
    public bool IsSuccess { get; set; }
    public bool IsPaymentConfirmed { get; set; }
    public string? PaymentIntentId { get; set; }
    public decimal Amount { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of refund operation.
/// </summary>
public class RefundResult
{
    public bool IsSuccess { get; set; }
    public string? RefundId { get; set; }
    public string? ErrorMessage { get; set; }
}

namespace Mango.MessageBus.Events;

/// <summary>
/// Event published when payment processing fails.
/// Consumed by Order Service and Email Service.
/// </summary>
public class PaymentFailedEvent
{
    /// <summary>
    /// Payment ID.
    /// </summary>
    public int PaymentId { get; set; }

    /// <summary>
    /// Associated order ID.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Customer/User ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Payment amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Reason for failure.
    /// </summary>
    public string FailureReason { get; set; } = string.Empty;

    /// <summary>
    /// Error code (if applicable).
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// When payment failed.
    /// </summary>
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;
}

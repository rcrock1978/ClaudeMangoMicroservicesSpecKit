namespace Mango.MessageBus.Events;

/// <summary>
/// Event published when a payment is refunded.
/// Consumed by Order Service and Email Service.
/// </summary>
public class PaymentRefundedEvent
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
    /// Original payment amount.
    /// </summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>
    /// Refund amount.
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Whether this is a full refund.
    /// </summary>
    public bool IsFullRefund { get; set; }

    /// <summary>
    /// Refund reason.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Refund transaction ID.
    /// </summary>
    public string? RefundTransactionId { get; set; }

    /// <summary>
    /// When refund was processed.
    /// </summary>
    public DateTime RefundedAt { get; set; } = DateTime.UtcNow;
}

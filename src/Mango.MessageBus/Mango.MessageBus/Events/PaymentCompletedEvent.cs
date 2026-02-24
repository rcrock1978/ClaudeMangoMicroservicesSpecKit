namespace Mango.MessageBus.Events;

/// <summary>
/// Event published when payment is successfully completed.
/// Consumed by Order Service, Reward Service, and Email Service.
/// </summary>
public class PaymentCompletedEvent
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
    /// Transaction ID from gateway.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Payment method used.
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Payment gateway.
    /// </summary>
    public string Gateway { get; set; } = "Stripe";

    /// <summary>
    /// Last 4 digits of card.
    /// </summary>
    public string? CardLast4 { get; set; }

    /// <summary>
    /// When payment was completed.
    /// </summary>
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

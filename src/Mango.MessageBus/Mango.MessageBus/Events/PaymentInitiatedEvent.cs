namespace Mango.MessageBus.Events;

/// <summary>
/// Event published when a payment is initiated.
/// Consumed by Email Service and Order Service.
/// </summary>
public class PaymentInitiatedEvent
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
    /// Payment method used.
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Payment gateway (Stripe or PayPal).
    /// </summary>
    public string Gateway { get; set; } = "Stripe";

    /// <summary>
    /// When payment was initiated.
    /// </summary>
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
}

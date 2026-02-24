namespace Mango.Services.Payment.Application.DTOs;

using Mango.Services.Payment.Domain;

/// <summary>
/// Data transfer object for payment information.
/// </summary>
public class PaymentDto
{
    /// <summary>
    /// Payment ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Associated order ID.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// User ID.
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
    /// Current payment status.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Payment method used.
    /// </summary>
    public PaymentMethod Method { get; set; }

    /// <summary>
    /// Payment gateway (Stripe or PayPal).
    /// </summary>
    public PaymentGateway Gateway { get; set; }

    /// <summary>
    /// Transaction ID from gateway.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Last 4 digits of card (PCI compliant display).
    /// </summary>
    public string? CardLast4 { get; set; }

    /// <summary>
    /// When payment was completed.
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Refunded amount.
    /// </summary>
    public decimal RefundedAmount { get; set; }

    /// <summary>
    /// When refund was processed.
    /// </summary>
    public DateTime? RefundDate { get; set; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// When created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

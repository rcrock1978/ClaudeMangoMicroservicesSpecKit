namespace Mango.Services.Payment.Application.DTOs;

using Mango.Services.Payment.Domain;

/// <summary>
/// Response containing current payment status and details.
/// </summary>
public class PaymentStatusResponse
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
    /// Current payment status.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Payment amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Refunded amount.
    /// </summary>
    public decimal RefundedAmount { get; set; }

    /// <summary>
    /// Remaining refundable amount.
    /// </summary>
    public decimal RefundableAmount { get; set; }

    /// <summary>
    /// Transaction ID from gateway.
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Last 4 digits of card (for security).
    /// </summary>
    public string? CardLast4 { get; set; }

    /// <summary>
    /// When payment was completed.
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// When refund was processed.
    /// </summary>
    public DateTime? RefundDate { get; set; }

    /// <summary>
    /// Error message if any.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Payment gateway used.
    /// </summary>
    public PaymentGateway Gateway { get; set; }

    /// <summary>
    /// When the response was generated.
    /// </summary>
    public DateTime ResponseTime { get; set; } = DateTime.UtcNow;
}

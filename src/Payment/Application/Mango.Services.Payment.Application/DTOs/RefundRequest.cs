namespace Mango.Services.Payment.Application.DTOs;

/// <summary>
/// Request to refund a payment.
/// </summary>
public class RefundRequest
{
    /// <summary>
    /// Payment ID to refund.
    /// </summary>
    public int PaymentId { get; set; }

    /// <summary>
    /// Amount to refund (may be partial).
    /// If null, full refund is requested.
    /// </summary>
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// Reason for the refund.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Optional metadata for the refund.
    /// </summary>
    public string? Metadata { get; set; }
}

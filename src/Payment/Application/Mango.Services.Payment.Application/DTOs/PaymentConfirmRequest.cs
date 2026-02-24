namespace Mango.Services.Payment.Application.DTOs;

/// <summary>
/// Request to confirm and process a payment.
/// </summary>
public class PaymentConfirmRequest
{
    /// <summary>
    /// Payment ID to confirm.
    /// </summary>
    public int PaymentId { get; set; }

    /// <summary>
    /// Payment intent ID from Stripe.
    /// </summary>
    public string? StripePaymentIntentId { get; set; }

    /// <summary>
    /// PayPal order ID.
    /// </summary>
    public string? PayPalOrderId { get; set; }

    /// <summary>
    /// Confirmation token or code.
    /// </summary>
    public string? ConfirmationToken { get; set; }

    /// <summary>
    /// Card details verification CVC/CVV.
    /// </summary>
    public string? Cvc { get; set; }

    /// <summary>
    /// Optional metadata for the confirmation.
    /// </summary>
    public string? Metadata { get; set; }
}

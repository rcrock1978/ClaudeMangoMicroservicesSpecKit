namespace Mango.Services.Payment.Application.DTOs;

using Mango.Services.Payment.Domain;

/// <summary>
/// Request to initiate a new payment.
/// </summary>
public class PaymentInitiateRequest
{
    /// <summary>
    /// Associated order ID.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Payment amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (USD, EUR, GBP, etc).
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Selected payment method.
    /// </summary>
    public PaymentMethod Method { get; set; }

    /// <summary>
    /// Preferred payment gateway (Stripe or PayPal).
    /// </summary>
    public PaymentGateway? Gateway { get; set; }

    /// <summary>
    /// Optional card holder name.
    /// </summary>
    public string? CardHolderName { get; set; }

    /// <summary>
    /// Optional card number (Stripe token or encrypted).
    /// </summary>
    public string? CardToken { get; set; }

    /// <summary>
    /// Optional card expiry month (MM).
    /// </summary>
    public string? CardExpiryMonth { get; set; }

    /// <summary>
    /// Optional card expiry year (YYYY).
    /// </summary>
    public string? CardExpiryYear { get; set; }

    /// <summary>
    /// Optional billing email.
    /// </summary>
    public string? BillingEmail { get; set; }

    /// <summary>
    /// Optional return URL for PayPal.
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Optional cancel URL for PayPal.
    /// </summary>
    public string? CancelUrl { get; set; }

    /// <summary>
    /// Payer's IP address for fraud detection.
    /// </summary>
    public string? PayerIpAddress { get; set; }
}

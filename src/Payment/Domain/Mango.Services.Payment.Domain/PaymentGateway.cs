namespace Mango.Services.Payment.Domain;

/// <summary>
/// Enumeration of supported payment gateways.
/// </summary>
public enum PaymentGateway
{
    /// <summary>
    /// Stripe payment gateway.
    /// </summary>
    Stripe = 0,

    /// <summary>
    /// PayPal payment gateway.
    /// </summary>
    PayPal = 1
}

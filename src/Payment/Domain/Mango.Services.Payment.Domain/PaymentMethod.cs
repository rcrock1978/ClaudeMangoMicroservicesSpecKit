namespace Mango.Services.Payment.Domain;

/// <summary>
/// Enumeration of supported payment methods.
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Credit card payment (Stripe/PayPal).
    /// </summary>
    CreditCard = 0,

    /// <summary>
    /// Debit card payment (Stripe/PayPal).
    /// </summary>
    DebitCard = 1,

    /// <summary>
    /// PayPal payment method.
    /// </summary>
    PayPal = 2,

    /// <summary>
    /// Apple Pay payment method.
    /// </summary>
    ApplePay = 3,

    /// <summary>
    /// Google Pay payment method.
    /// </summary>
    GooglePay = 4
}

namespace Mango.Services.Payment.Domain;

/// <summary>
/// Enumeration of payment statuses throughout the payment lifecycle.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment has been initiated but not yet processed.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment is currently being processed by the payment gateway.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Payment has been successfully completed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Payment processing failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Payment has been partially or fully refunded.
    /// </summary>
    Refunded = 4,

    /// <summary>
    /// Payment was cancelled before processing.
    /// </summary>
    Cancelled = 5
}

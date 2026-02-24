namespace Mango.Services.Payment.Domain;

/// <summary>
/// Audit log for payment transactions.
/// Tracks all payment events for compliance and debugging purposes.
/// </summary>
public class PaymentLog : BaseEntity
{
    /// <summary>
    /// Associated payment ID.
    /// </summary>
    public int PaymentId { get; set; }

    /// <summary>
    /// Navigation property to the associated payment.
    /// </summary>
    public Payment? Payment { get; set; }

    /// <summary>
    /// Payment gateway transaction ID.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Description of the event logged.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Payment status at the time of logging.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Detailed message describing what happened.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error message if an error occurred.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Raw response data from the payment gateway (JSON).
    /// Sensitive data should be encrypted or masked in logs.
    /// </summary>
    public string? ResponseData { get; set; }

    /// <summary>
    /// When the event was recorded (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional metadata or context (JSON).
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Validate that the log entry has required information.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(TransactionId) &&
               !string.IsNullOrWhiteSpace(EventType) &&
               !string.IsNullOrWhiteSpace(Message) &&
               PaymentId > 0;
    }
}

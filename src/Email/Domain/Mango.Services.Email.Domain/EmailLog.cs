namespace Mango.Services.Email.Domain.Entities;

/// <summary>
/// Email log entity for auditing sent emails.
/// Tracks all email sending attempts for compliance and troubleshooting.
/// </summary>
public class EmailLog : AuditableEntity
{
    /// <summary>
    /// Recipient email address.
    /// </summary>
    public string RecipientEmail { get; set; } = string.Empty;

    /// <summary>
    /// Recipient name (optional).
    /// </summary>
    public string? RecipientName { get; set; }

    /// <summary>
    /// Subject line of the sent email.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email body content (may be truncated for storage).
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Email template name used (if applicable).
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Whether the email was sent successfully.
    /// </summary>
    public bool IsSent { get; set; } = false;

    /// <summary>
    /// Number of send attempts made.
    /// </summary>
    public int AttemptCount { get; set; } = 0;

    /// <summary>
    /// Error message if sending failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when email was sent (if successful).
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Timestamp of last attempt.
    /// </summary>
    public DateTime? LastAttemptAt { get; set; }

    /// <summary>
    /// Associated order ID for order confirmation emails (optional).
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>
    /// Associated user ID.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Email type/category for filtering and reporting.
    /// </summary>
    public string? EmailType { get; set; }

    /// <summary>
    /// Validate that the log entry has required fields.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(RecipientEmail) &&
               !string.IsNullOrWhiteSpace(Subject) &&
               !string.IsNullOrWhiteSpace(Body);
    }

    /// <summary>
    /// Mark email as successfully sent.
    /// </summary>
    public void MarkAsSent()
    {
        IsSent = true;
        SentAt = DateTime.UtcNow;
        LastAttemptAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    /// <summary>
    /// Record a failed send attempt.
    /// </summary>
    public void RecordFailedAttempt(string error)
    {
        AttemptCount++;
        LastAttemptAt = DateTime.UtcNow;
        ErrorMessage = error;
        IsSent = false;
    }

    /// <summary>
    /// Check if email should be retried based on attempt count.
    /// </summary>
    public bool ShouldRetry(int maxAttempts = 3)
    {
        return !IsSent && AttemptCount < maxAttempts;
    }
}

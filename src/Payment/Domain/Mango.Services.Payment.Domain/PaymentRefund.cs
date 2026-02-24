namespace Mango.Services.Payment.Domain;

/// <summary>
/// Represents a refund transaction for a payment.
/// Tracks refund history and status for audit purposes.
/// </summary>
public class PaymentRefund : BaseEntity
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
    /// Amount being refunded.
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// Refund reason or description.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Current refund status.
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Processing;

    /// <summary>
    /// When the refund was processed (UTC).
    /// </summary>
    public DateTime? ProcessedDate { get; set; }

    /// <summary>
    /// Gateway refund transaction ID.
    /// </summary>
    public string? GatewayRefundId { get; set; }

    /// <summary>
    /// Error message if refund failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Validate that the refund amount is reasonable.
    /// </summary>
    /// <param name="maxRefundableAmount">Maximum amount that can be refunded</param>
    /// <returns>True if valid</returns>
    public bool ValidateRefund(decimal maxRefundableAmount)
    {
        if (RefundAmount <= 0)
        {
            return false;
        }

        if (RefundAmount > maxRefundableAmount)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(Reason))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Mark refund as processed successfully.
    /// </summary>
    /// <param name="gatewayRefundId">Refund transaction ID from gateway</param>
    /// <returns>True if marked as processed</returns>
    public bool MarkAsProcessed(string gatewayRefundId)
    {
        if (Status != PaymentStatus.Processing)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(gatewayRefundId))
        {
            return false;
        }

        Status = PaymentStatus.Completed;
        GatewayRefundId = gatewayRefundId;
        ProcessedDate = DateTime.UtcNow;
        ErrorMessage = null;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Mark refund as failed.
    /// </summary>
    /// <param name="errorMessage">Error reason</param>
    /// <returns>True if marked as failed</returns>
    public bool MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return false;
        }

        Status = PaymentStatus.Failed;
        ErrorMessage = errorMessage;
        ProcessedDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Get all validation errors for the refund.
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (PaymentId <= 0)
            errors.Add("PaymentId is required");

        if (RefundAmount <= 0)
            errors.Add("RefundAmount must be greater than 0");

        if (string.IsNullOrWhiteSpace(Reason))
            errors.Add("Reason is required");

        return errors;
    }
}

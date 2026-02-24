namespace Mango.Services.Payment.Domain;

/// <summary>
/// Payment aggregate root entity.
/// Represents a payment transaction with state machine pattern for lifecycle management.
/// Handles payment creation, status transitions, and refunds.
/// </summary>
public class Payment : BaseEntity
{
    /// <summary>
    /// Associated order ID.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// User/Customer ID who initiated the payment.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Payment amount in the specified currency.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (USD, EUR, GBP, etc).
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Current payment status.
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Selected payment method (CreditCard, PayPal, etc).
    /// </summary>
    public PaymentMethod Method { get; set; }

    /// <summary>
    /// Payment gateway used (Stripe or PayPal).
    /// </summary>
    public PaymentGateway Gateway { get; set; }

    /// <summary>
    /// Unique transaction ID from the payment gateway.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Last 4 digits of card (encrypted in DB, displayed in logs).
    /// </summary>
    public string? CardLast4 { get; set; }

    /// <summary>
    /// Card holder name (encrypted).
    /// </summary>
    public string? CardHolderName { get; set; }

    /// <summary>
    /// Card expiry month (encrypted).
    /// </summary>
    public string? CardExpiryMonth { get; set; }

    /// <summary>
    /// Card expiry year (encrypted).
    /// </summary>
    public string? CardExpiryYear { get; set; }

    /// <summary>
    /// When the payment was completed (UTC).
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Total amount refunded (may be partial or full).
    /// </summary>
    public decimal RefundedAmount { get; set; } = 0;

    /// <summary>
    /// When the refund was processed (UTC).
    /// </summary>
    public DateTime? RefundDate { get; set; }

    /// <summary>
    /// Error message if payment failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Raw response data from the payment gateway (JSON).
    /// </summary>
    public string? GatewayResponseData { get; set; }

    /// <summary>
    /// IP address of the payer for fraud detection.
    /// </summary>
    public string? PayerIpAddress { get; set; }

    /// <summary>
    /// Collection of refunds issued for this payment.
    /// </summary>
    public ICollection<PaymentRefund> Refunds { get; set; } = new List<PaymentRefund>();

    /// <summary>
    /// Collection of audit logs for this payment.
    /// </summary>
    public ICollection<PaymentLog> Logs { get; set; } = new List<PaymentLog>();

    /// <summary>
    /// Initiate a new payment - transition to Processing state.
    /// </summary>
    /// <returns>True if initialization succeeded</returns>
    public bool InitiatePayment()
    {
        if (Status != PaymentStatus.Pending)
        {
            return false;
        }

        if (Amount <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(UserId) || OrderId <= 0)
        {
            return false;
        }

        Status = PaymentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Mark payment as completed.
    /// </summary>
    /// <param name="transactionId">Transaction ID from gateway</param>
    /// <returns>True if completion succeeded</returns>
    public bool CompletePayment(string transactionId)
    {
        if (Status != PaymentStatus.Processing)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return false;
        }

        TransactionId = transactionId;
        Status = PaymentStatus.Completed;
        PaymentDate = DateTime.UtcNow;
        ErrorMessage = null;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Mark payment as failed.
    /// </summary>
    /// <param name="errorMessage">Reason for failure</param>
    /// <returns>True if failure recorded</returns>
    public bool FailPayment(string errorMessage)
    {
        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return false;
        }

        Status = PaymentStatus.Failed;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Process a refund (fully or partially).
    /// </summary>
    /// <param name="refundAmount">Amount to refund (must be <= Amount)</param>
    /// <param name="reason">Refund reason</param>
    /// <returns>True if refund was initiated</returns>
    public bool RefundPayment(decimal refundAmount, string reason)
    {
        if (Status != PaymentStatus.Completed)
        {
            return false;
        }

        if (refundAmount <= 0 || refundAmount > (Amount - RefundedAmount))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return false;
        }

        RefundedAmount += refundAmount;

        // Transition to Refunded if fully refunded
        if (RefundedAmount >= Amount)
        {
            Status = PaymentStatus.Refunded;
        }

        RefundDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Check if payment can be refunded.
    /// </summary>
    /// <returns>True if refund is allowed</returns>
    public bool CanBeRefunded()
    {
        return Status == PaymentStatus.Completed && RefundedAmount < Amount;
    }

    /// <summary>
    /// Get remaining refundable amount.
    /// </summary>
    public decimal GetRefundableAmount()
    {
        return Amount - RefundedAmount;
    }

    /// <summary>
    /// Cancel a payment (only from Pending or Processing).
    /// </summary>
    /// <returns>True if cancellation succeeded</returns>
    public bool CancelPayment()
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
        {
            return false;
        }

        Status = PaymentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Check if payment is in a terminal state.
    /// </summary>
    public bool IsTerminal()
    {
        return Status == PaymentStatus.Completed ||
               Status == PaymentStatus.Failed ||
               Status == PaymentStatus.Refunded ||
               Status == PaymentStatus.Cancelled;
    }

    /// <summary>
    /// Validate payment amount against configured limits.
    /// </summary>
    /// <param name="minAmount">Minimum allowed amount</param>
    /// <param name="maxAmount">Maximum allowed amount</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    public List<string> ValidateAmount(decimal minAmount, decimal maxAmount)
    {
        var errors = new List<string>();

        if (Amount < minAmount)
            errors.Add($"Amount must be at least {minAmount} {Currency}");

        if (Amount > maxAmount)
            errors.Add($"Amount cannot exceed {maxAmount} {Currency}");

        return errors;
    }

    /// <summary>
    /// Get all validation errors for the current payment state.
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (Amount <= 0)
            errors.Add("Amount must be greater than 0");

        if (string.IsNullOrWhiteSpace(UserId))
            errors.Add("UserId is required");

        if (OrderId <= 0)
            errors.Add("OrderId is required");

        if (string.IsNullOrWhiteSpace(Currency) || Currency.Length != 3)
            errors.Add("Currency must be a valid 3-letter code");

        if (Status == PaymentStatus.Processing && string.IsNullOrWhiteSpace(TransactionId))
            errors.Add("TransactionId is required for Processing status");

        if (Status == PaymentStatus.Completed && string.IsNullOrWhiteSpace(TransactionId))
            errors.Add("TransactionId is required for Completed status");

        if (Status == PaymentStatus.Failed && string.IsNullOrWhiteSpace(ErrorMessage))
            errors.Add("ErrorMessage is required for Failed status");

        return errors;
    }
}
